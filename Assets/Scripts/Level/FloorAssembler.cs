using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

// Builds a floor out of room prefabs (Assets/Rooms): picks rooms at random, joins them with straight corridors
// at their doorways, bricks up unused doorways, bakes the NavMesh, then adds the goblin jump/climb/hop links.
public static class FloorAssembler
{
    const int IgnoreRaycastLayer = 2;

    class Door
    {
        public RoomDoorway marker;
        public RoomModule room;
        public bool used;
        public Vector3 Position => marker.transform.position;
    }

    // onlyRoom: build a test floor with just this room (F8).
    public static LevelLayout Build(TuningConfig t, RoomLibrary library, GameObject onlyRoom = null)
    {
        var layout = new LevelLayout();
        var root = new GameObject("Floor");
        layout.root = root;

        var prefabs = new List<GameObject>();
        if (library != null) foreach (var r in library.rooms) if (r != null && r.GetComponent<RoomModule>() != null) prefabs.Add(r);

        var rooms = new List<RoomModule>();
        var blocked = new List<Bounds>();
        var doors = new List<Door>();

        if (onlyRoom != null) PlaceFirst(onlyRoom, root, rooms, blocked, doors);
        else if (prefabs.Count > 0)
        {
            PlaceFirst(prefabs[Random.Range(0, prefabs.Count)], root, rooms, blocked, doors);
            for (int i = 1; i < t.roomsPerFloor; i++)
                if (!TryAttachRoom(t, prefabs, root, rooms, blocked, doors)) break;
        }
        else
        {
            Debug.LogError("No room prefabs found. Run Grand Descent > Rebuild Starter Rooms, or add rooms to Assets/Rooms.");
            rooms.Add(FallbackRoom(root));
        }

        foreach (var d in doors) if (!d.used) SealDoorway(root, d.marker);
        Physics.SyncTransforms(); // so torches can find their walls

        CollectMarkers(t, layout, rooms);
        SurfaceTexturer.Apply(root, t);

        var surface = root.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = ~(1 << IgnoreRaycastLayer); // pits and ladder triggers aren't floor
        surface.BuildNavMesh();
        TraversalLinkBuilder.Build(root, t, surface.agentTypeID);

        foreach (var r in rooms) layout.roomNames.Add(r.displayName);
        return layout;
    }

    static void PlaceFirst(GameObject prefab, GameObject root, List<RoomModule> rooms, List<Bounds> blocked, List<Door> doors)
    {
        var room = Spawn(prefab, root, Vector3.zero, Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f));
        rooms.Add(room);
        blocked.Add(Shrink(room.CalculateBounds()));
        AddDoors(room, doors);
    }

    // One attempt per (open doorway, prefab, prefab doorway) until a room fits without overlapping anything.
    static bool TryAttachRoom(TuningConfig t, List<GameObject> prefabs, GameObject root, List<RoomModule> rooms, List<Bounds> blocked, List<Door> doors)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            var open = doors.FindAll(d => !d.used);
            if (open.Count == 0) return false;
            Door from = open[Random.Range(0, open.Count)];

            // Prefer a room that isn't on this floor yet.
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            if (attempt < 20)
                foreach (var r in rooms)
                    if (r.displayName == prefab.GetComponent<RoomModule>().displayName) { prefab = prefabs[Random.Range(0, prefabs.Count)]; break; }

            var room = Spawn(prefab, root, Vector3.zero, Quaternion.identity);
            var candidates = room.GetComponentsInChildren<RoomDoorway>();
            if (candidates.Length == 0) { Object.DestroyImmediate(room.gameObject); continue; }
            RoomDoorway to = candidates[Random.Range(0, candidates.Length)];

            // Turn the new room so its doorway faces back down the corridor, then slide it into place.
            float length = Random.Range(t.corridorLengthMin, t.corridorLengthMax);
            Vector3 outward = from.marker.Outward;
            float yaw = Mathf.Round(Vector3.SignedAngle(to.Outward, -outward, Vector3.up) / 90f) * 90f;
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 localDoor = to.transform.position - room.transform.position;
            Vector3 target = from.Position + outward * length;
            room.transform.SetPositionAndRotation(target - rot * localDoor, rot);
            Physics.SyncTransforms();

            Bounds roomBounds = Shrink(room.CalculateBounds());
            Bounds corridorBounds = CorridorBounds(from.Position, target, Mathf.Max(from.marker.width, to.width), t.corridorHeight);
            if (Overlaps(roomBounds, blocked) || Overlaps(Shrink(corridorBounds), blocked) || roomBounds.Intersects(Shrink(corridorBounds)))
            {
                Object.DestroyImmediate(room.gameObject);
                continue;
            }

            BuildCorridor(t, root, from.marker, to);
            from.used = true;
            rooms.Add(room);
            blocked.Add(roomBounds);
            blocked.Add(Shrink(corridorBounds));
            AddDoors(room, doors);
            doors.Find(d => d.marker == to).used = true;
            return true;
        }
        return false;
    }

    static RoomModule Spawn(GameObject prefab, GameObject root, Vector3 position, Quaternion rotation)
    {
        var go = Object.Instantiate(prefab, position, rotation, root.transform);
        go.name = prefab.name;
        return go.GetComponent<RoomModule>();
    }

    static void AddDoors(RoomModule room, List<Door> doors)
    {
        foreach (var d in room.GetComponentsInChildren<RoomDoorway>()) doors.Add(new Door { marker = d, room = room });
    }

    static Bounds Shrink(Bounds b)
    {
        b.Expand(-0.2f);
        return b;
    }

    static bool Overlaps(Bounds b, List<Bounds> others)
    {
        foreach (var o in others) if (b.Intersects(o)) return true;
        return false;
    }

    static Bounds CorridorBounds(Vector3 a, Vector3 b, float width, float height)
    {
        Vector3 side = Vector3.Cross(Vector3.up, (b - a).normalized) * (width / 2f + 0.5f);
        var bounds = new Bounds(a + side, Vector3.zero);
        bounds.Encapsulate(a - side);
        bounds.Encapsulate(b + side);
        bounds.Encapsulate(b - side + Vector3.up * (height + 0.5f));
        bounds.Encapsulate(a + Vector3.down * 0.5f);
        return bounds;
    }

    // Floor, two walls, a ceiling and one torch, from doorway a to doorway b.
    static void BuildCorridor(TuningConfig t, GameObject root, RoomDoorway a, RoomDoorway b)
    {
        Vector3 from = a.transform.position, to = b.transform.position;
        float length = Vector3.Distance(new Vector3(from.x, 0f, from.z), new Vector3(to.x, 0f, to.z));
        float width = Mathf.Max(a.width, b.width);
        float height = Mathf.Max(t.corridorHeight, Mathf.Max(a.height, b.height) + 0.5f);

        var corridor = new GameObject("Corridor");
        corridor.transform.SetParent(root.transform, false);
        corridor.transform.SetPositionAndRotation((from + to) / 2f, Quaternion.LookRotation(a.Outward));
        Transform c = corridor.transform;

        // A slope if the doorways aren't at the same height (rare; rooms usually keep doors at floor level 0).
        float rise = to.y - from.y;
        var floor = LevelBlocks.Block(corridor, "Corridor Floor", Vector3.down * 0.25f, new Vector3(width + 1f, 0.5f, length + 0.2f), Palette.Floor);
        floor.transform.localRotation = Quaternion.Euler(-Mathf.Atan2(rise, length) * Mathf.Rad2Deg, 0f, 0f);
        foreach (float sign in new[] { -1f, 1f })
            LevelBlocks.Block(corridor, "Corridor Wall", new Vector3(sign * (width / 2f + 0.25f), height / 2f, 0f), new Vector3(0.5f, height + Mathf.Abs(rise), length), Palette.Wall);
        LevelBlocks.Block(corridor, "Corridor Ceiling", new Vector3(0f, height + 0.25f + Mathf.Max(0f, rise), 0f), new Vector3(width + 1f, 0.5f, length), Palette.Wall);

        Vector3 torch = c.position + c.right * (width / 2f - 0.1f) + Vector3.up * t.torchHeight;
        LevelLighting.AddTorch(root, torch, t);
    }

    // An unused doorway gets a wall plug just outside the opening (part of its room, so it gets the room's walls).
    static void SealDoorway(GameObject root, RoomDoorway d)
    {
        var room = d.GetComponentInParent<RoomModule>();
        var plug = LevelBlocks.Block(room != null ? room.gameObject : root, "Sealed Doorway", Vector3.zero, new Vector3(d.width + 0.4f, d.height + 0.2f, 0.5f), Palette.Wall);
        plug.transform.SetPositionAndRotation(d.transform.position + d.Outward * 0.25f + Vector3.up * (d.height / 2f + 0.05f), Quaternion.LookRotation(d.Outward));
    }

    static void CollectMarkers(TuningConfig t, LevelLayout layout, List<RoomModule> rooms)
    {
        RoomModule first = rooms[0], last = rooms[rooms.Count - 1];
        bool haveStart = false, havePortal = false;

        foreach (var room in rooms)
        {
            foreach (var spot in room.GetComponentsInChildren<RoomSpot>())
            {
                Vector3 p = spot.transform.position;
                switch (spot.kind)
                {
                    case RoomSpot.Kind.EnemySpawn: layout.spawnPoints.Add(p); break;
                    case RoomSpot.Kind.Shrine: layout.shrineSpots.Add(p); break;
                    case RoomSpot.Kind.Barrel: layout.barrelSpots.Add(p); break;
                    case RoomSpot.Kind.Torch: LevelLighting.AddTorch(layout.root, p, t); break;
                    case RoomSpot.Kind.PlayerStart:
                        if (room == first && !haveStart)
                        {
                            layout.playerStart = p + Vector3.up * 0.05f;
                            layout.playerStartYaw = spot.transform.eulerAngles.y;
                            haveStart = true;
                        }
                        break;
                    case RoomSpot.Kind.Portal:
                        if (room == last && !havePortal)
                        {
                            layout.portalSpot = p;
                            layout.portalYaw = spot.transform.eulerAngles.y;
                            havePortal = true;
                        }
                        break;
                }
            }
        }

        if (!haveStart) layout.playerStart = GroundBelow(first.CalculateBounds().center) + Vector3.up * 0.05f;
        if (!havePortal) layout.portalSpot = GroundBelow(last.CalculateBounds().center);
        if (layout.spawnPoints.Count == 0)
        {
            Debug.LogWarning("No Enemy Spawn spots in these rooms; goblins will spawn at the portal spot.");
            layout.spawnPoints.Add(layout.portalSpot);
        }
    }

    static Vector3 GroundBelow(Vector3 p)
    {
        if (Physics.Raycast(p + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 60f, ~(1 << IgnoreRaycastLayer), QueryTriggerInteraction.Ignore))
            return hit.point;
        return new Vector3(p.x, 0f, p.z);
    }

    // Used only if there are no room prefabs at all: a plain box so the game still runs.
    static RoomModule FallbackRoom(GameObject root)
    {
        var go = new GameObject("Fallback Room");
        go.transform.SetParent(root.transform, false);
        var room = go.AddComponent<RoomModule>();
        room.displayName = "Fallback Room";
        LevelBlocks.Block(go, "Floor", Vector3.down * 0.25f, new Vector3(20f, 0.5f, 20f), Palette.Floor);
        LevelBlocks.Block(go, "Ceiling", Vector3.up * 5.25f, new Vector3(20f, 0.5f, 20f), Palette.Wall);
        LevelBlocks.Block(go, "Wall N", new Vector3(0f, 2.5f, 10.25f), new Vector3(21f, 5f, 0.5f), Palette.Wall);
        LevelBlocks.Block(go, "Wall S", new Vector3(0f, 2.5f, -10.25f), new Vector3(21f, 5f, 0.5f), Palette.Wall);
        LevelBlocks.Block(go, "Wall E", new Vector3(10.25f, 2.5f, 0f), new Vector3(0.5f, 5f, 21f), Palette.Wall);
        LevelBlocks.Block(go, "Wall W", new Vector3(-10.25f, 2.5f, 0f), new Vector3(0.5f, 5f, 21f), Palette.Wall);
        AddSpot(go, RoomSpot.Kind.PlayerStart, new Vector3(0f, 0f, -7f));
        AddSpot(go, RoomSpot.Kind.Portal, new Vector3(0f, 0f, 3f));
        AddSpot(go, RoomSpot.Kind.Torch, new Vector3(0f, 2.6f, 9.85f));
        foreach (var p in new[] { new Vector3(-7f, 0f, 7f), new Vector3(7f, 0f, 7f), new Vector3(-7f, 0f, -2f), new Vector3(7f, 0f, -2f) })
            AddSpot(go, RoomSpot.Kind.EnemySpawn, p);
        return room;
    }

    static void AddSpot(GameObject parent, RoomSpot.Kind kind, Vector3 position)
    {
        var go = new GameObject(kind.ToString());
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = position;
        go.AddComponent<RoomSpot>().kind = kind;
    }
}
