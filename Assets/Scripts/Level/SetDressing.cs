using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Clutter and wall decoration for a freshly built floor, placed at random from the floor's seed
// (so a loaded save gets the same room). Clutter stands against walls, away from doorways, ladders,
// stairs, pits, spawn points and the portal, and never where a passage is narrow. Crates and barrels
// are solid (goblins walk around them); bones, candles, buckets and signs are not.
// Walls get crown banners and sponsor posters; walls and floors get a few cracks.
public static class SetDressing
{
    const int IgnoreRaycast = 2;
    const int Solid = ~(1 << IgnoreRaycast);

    struct PropKind
    {
        public System.Func<EnvironmentArt, Texture2D> texture;
        public float height, radius;
        public bool solid;
    }

    static readonly PropKind[] Props =
    {
        new PropKind { texture = a => a.crate, height = 1.0f, radius = 0.45f, solid = true },
        new PropKind { texture = a => a.barrel, height = 1.1f, radius = 0.38f, solid = true },
        new PropKind { texture = a => a.bonePile, height = 0.9f, radius = 0.4f },
        new PropKind { texture = a => a.candles, height = 0.55f, radius = 0.2f },
        new PropKind { texture = a => a.mopBucket, height = 0.9f, radius = 0.3f },
        new PropKind { texture = a => a.wetFloorSign, height = 0.85f, radius = 0.3f },
    };

    static readonly List<Vector3> taken = new List<Vector3>();     // clutter placed so far
    static readonly List<Vector3> avoid = new List<Vector3>();     // doorways, ladders, spawns, portal...
    static readonly List<Vector3> decals = new List<Vector3>();
    static readonly List<Vector3> torches = new List<Vector3>();

    public static void Decorate(LevelLayout level, TuningConfig t)
    {
        var art = EnvironmentArt.Current;
        if (art == null || !PropSprites.Ready) return;
        Physics.SyncTransforms();
        taken.Clear();
        decals.Clear();
        torches.Clear();
        foreach (var torch in level.root.GetComponentsInChildren<TorchFlicker>()) torches.Add(torch.transform.position);
        CollectAvoid(level);

        var parent = new GameObject("Set Dressing").transform;
        parent.SetParent(level.root.transform, false);

        foreach (var room in level.root.GetComponentsInChildren<RoomModule>())
        {
            Bounds b = room.CalculateBounds();
            float area = b.size.x * b.size.z;
            int clutter = Mathf.RoundToInt(area / 100f * t.clutterPerHundredSquareMetres * Random.Range(0.7f, 1.3f));
            for (int i = 0; i < clutter; i++) TryPlaceClutter(room, b, art, parent);
            Cluster(room, b, art, parent, t);

            int banners = Random.Range(t.bannersPerRoomMin, t.bannersPerRoomMax + 1);
            for (int i = 0; i < banners; i++) TryWallDecal(room, b, art.bannerCrown, 1.1f, 2.2f, 2.9f, parent);
            if (Random.value < t.posterChance) TryWallDecal(room, b, art.posterSponsor, 0.75f, 1.5f, 1.7f, parent);
            int cracks = Random.Range(t.cracksPerRoomMin, t.cracksPerRoomMax + 1);
            for (int i = 0; i < cracks; i++)
            {
                if (Random.value < 0.5f) TryWallDecal(room, b, art.crack, 0.9f, 0.9f, Random.Range(0.8f, 2.6f), parent, spin: true);
                else TryFloorCrack(room, b, art.crack, parent);
            }
        }
    }

    static void CollectAvoid(LevelLayout level)
    {
        avoid.Clear();
        foreach (var d in level.root.GetComponentsInChildren<RoomDoorway>()) avoid.Add(d.transform.position);
        foreach (var l in level.root.GetComponentsInChildren<RoomLadder>()) avoid.Add(l.transform.position);
        avoid.AddRange(level.spawnPoints);
        avoid.AddRange(level.shrineSpots);
        avoid.AddRange(level.barrelSpots);
        avoid.Add(level.portalSpot);
        avoid.Add(level.playerStart);
    }

    // --- Clutter --------------------------------------------------------------------------------------------

    static void TryPlaceClutter(RoomModule room, Bounds b, EnvironmentArt art, Transform parent)
    {
        var kind = Props[Random.Range(0, Props.Length)];
        var tex = kind.texture(art);
        if (tex == null) return;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            if (!FindWallSpot(room, b, kind.radius, out Vector3 spot, out _)) continue;
            Spawn(kind, tex, spot, parent);
            return;
        }
    }

    // Now and then a little scene: a mop bucket with a wet floor sign, or candles by a bone pile.
    static void Cluster(RoomModule room, Bounds b, EnvironmentArt art, Transform parent, TuningConfig t)
    {
        if (Random.value > t.clutterClusterChance) return;
        bool janitor = Random.value < 0.5f;
        var first = janitor ? Props[4] : Props[2];
        var second = janitor ? Props[5] : Props[3];
        if (first.texture(art) == null || second.texture(art) == null) return;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            if (!FindWallSpot(room, b, 0.9f, out Vector3 spot, out Vector3 normal)) continue;
            Vector3 along = Vector3.Cross(Vector3.up, normal).normalized;
            Spawn(first, first.texture(art), spot - along * 0.35f, parent);
            Spawn(second, second.texture(art), spot + along * 0.4f, parent);
            return;
        }
    }

    // A floor spot against a wall, with open space in front of it.
    static bool FindWallSpot(RoomModule room, Bounds b, float radius, out Vector3 spot, out Vector3 normal)
    {
        spot = normal = default;
        if (!FindFloor(room, b, out RaycastHit floor)) return false;

        // Toward the nearest wall: the closest NavMesh edge, then a short ray past it.
        if (!NavMesh.SamplePosition(floor.point, out NavMeshHit onMesh, 0.4f, NavMesh.AllAreas)) return false;
        if (!NavMesh.FindClosestEdge(onMesh.position, out NavMeshHit edge, NavMesh.AllAreas) || edge.distance > 1.5f) return false;
        Vector3 toWall = edge.position - onMesh.position;
        toWall.y = 0f;
        if (toWall.sqrMagnitude < 0.0001f) toWall = -edge.normal;
        toWall.Normalize();
        Vector3 eye = edge.position + Vector3.up * 0.5f;
        if (!Physics.Raycast(eye, toWall, out RaycastHit wall, 1.2f, Solid, QueryTriggerInteraction.Ignore)) return false; // a drop, not a wall
        if (Mathf.Abs(wall.normal.y) > 0.2f || wall.collider.GetComponentInParent<RoomModule>() == null) return false;

        normal = new Vector3(wall.normal.x, 0f, wall.normal.z).normalized;
        spot = new Vector3(wall.point.x, edge.position.y, wall.point.z) + normal * (radius + 0.08f);
        // The floor right under it must be flat floor of this level (not a step or ramp).
        if (!Physics.Raycast(spot + Vector3.up * 0.6f, Vector3.down, out RaycastHit under, 1.2f, Solid, QueryTriggerInteraction.Ignore)) return false;
        if (under.normal.y < 0.95f || Mathf.Abs(under.point.y - edge.position.y) > 0.15f || IsStairs(under.collider)) return false;
        spot.y = under.point.y;

        // Not in a narrow passage: keep at least 2.5 m clear in front.
        if (Physics.Raycast(spot + Vector3.up * 0.5f, normal, 2.5f, Solid, QueryTriggerInteraction.Ignore)) return false;
        if (NearAny(spot, avoid, 2.5f) || NearAny(spot, taken, 1.4f) || NearPitOrLadder(spot)) return false;
        foreach (var barrel in ExplosiveBarrel.All) if (Vector3.Distance(barrel.transform.position, spot) < 1.5f) return false;
        return true;
    }

    static bool FindFloor(RoomModule room, Bounds b, out RaycastHit floor)
    {
        Vector3 p = new Vector3(Random.Range(b.min.x, b.max.x), b.max.y - 0.6f, Random.Range(b.min.z, b.max.z));
        // Start under the ceiling and look down for the first walkable surface.
        if (!Physics.Raycast(p, Vector3.down, out floor, b.size.y + 1f, Solid, QueryTriggerInteraction.Ignore)) return false;
        return floor.normal.y > 0.95f && floor.collider.GetComponentInParent<RoomModule>() == room;
    }

    static bool IsStairs(Collider c)
    {
        string n = c.name;
        return n.Contains("Step") || n.Contains("Stair") || n.Contains("Ramp") || n.Contains("Ladder");
    }

    static bool NearPitOrLadder(Vector3 p)
    {
        foreach (var hit in Physics.OverlapSphere(p + Vector3.up * 0.3f, 1.5f, 1 << IgnoreRaycast, QueryTriggerInteraction.Collide))
            if (hit.GetComponent<RoomPit>() != null || hit.GetComponentInParent<RoomLadder>() != null) return true;
        foreach (var pit in RoomPit.All)
        {
            Bounds pb = pit.Bounds;
            pb.Expand(new Vector3(2f, 4f, 2f));
            if (pb.Contains(p)) return true;
        }
        return false;
    }

    static bool NearAny(Vector3 p, List<Vector3> points, float distance)
    {
        foreach (var q in points)
            if ((q - p).sqrMagnitude < distance * distance) return true;
        return false;
    }

    static void Spawn(PropKind kind, Texture2D tex, Vector3 position, Transform parent)
    {
        var pivot = PropSprites.Billboard(tex.name, parent, position, tex, kind.height, out _);
        taken.Add(position);
        if (!kind.solid) return;
        var capsule = pivot.gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = kind.radius;
        capsule.height = kind.height * 0.8f;
        capsule.center = Vector3.up * capsule.height / 2f;
        var obstacle = pivot.gameObject.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.radius = kind.radius;
        obstacle.height = capsule.height;
        obstacle.center = capsule.center;
        obstacle.carving = true;
    }

    // --- Decals ---------------------------------------------------------------------------------------------

    // A decal flat on a wall: needs a flat stretch of wall the size of the decal, away from doorways,
    // torches and other decals. centreHeight = height of its middle above the floor.
    static void TryWallDecal(RoomModule room, Bounds b, Texture2D tex, float width, float height, float centreHeight, Transform parent, bool spin = false)
    {
        if (tex == null) return;
        for (int attempt = 0; attempt < 15; attempt++)
        {
            if (!FindFloor(room, b, out RaycastHit floor)) continue;
            Vector3 from = floor.point + Vector3.up * centreHeight;
            float angle = Random.Range(0, 4) * 90f + Random.Range(-20f, 20f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            if (!Physics.Raycast(from, dir, out RaycastHit wall, 30f, Solid, QueryTriggerInteraction.Ignore)) continue;
            if (Mathf.Abs(wall.normal.y) > 0.05f || wall.collider.GetComponentInParent<RoomModule>() != room) continue;
            Vector3 normal = wall.normal;
            Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
            if (!FlatPatch(wall.point, normal, right, Vector3.up, width, height)) continue;
            if (NearAny(wall.point, decals, 2f) || NearAny(wall.point, torches, 1.4f) || NearAny(wall.point, avoid, 1.5f)) continue;

            PropSprites.Decal(tex.name, parent, wall.point, normal, tex, width, height, spin ? Random.Range(0f, 360f) : 0f);
            decals.Add(wall.point);
            return;
        }
    }

    static void TryFloorCrack(RoomModule room, Bounds b, Texture2D tex, Transform parent)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            if (!FindFloor(room, b, out RaycastHit floor) || IsStairs(floor.collider)) continue;
            Vector3 right = Vector3.right, forward = Vector3.forward;
            if (!FlatPatch(floor.point, Vector3.up, right, forward, 1.6f, 1.6f) || NearAny(floor.point, decals, 2f)) continue;
            PropSprites.Decal(tex.name, parent, floor.point, Vector3.up, tex, 1.2f, 1.2f, Random.Range(0f, 360f));
            decals.Add(floor.point);
            return;
        }
    }

    // True if the surface is flat (same distance) under all four corners and the centre of the patch.
    static bool FlatPatch(Vector3 centre, Vector3 normal, Vector3 right, Vector3 up, float width, float height)
    {
        float w = width / 2f, h = height / 2f;
        foreach (var offset in new[] { Vector3.zero, right * w + up * h, right * w - up * h, -right * w + up * h, -right * w - up * h })
        {
            Vector3 start = centre + offset + normal * 0.3f;
            if (!Physics.Raycast(start, -normal, out RaycastHit hit, 0.45f, Solid, QueryTriggerInteraction.Ignore)) return false;
            if (Mathf.Abs(hit.distance - 0.3f) > 0.03f || Vector3.Dot(hit.normal, normal) < 0.98f) return false;
        }
        // Nothing sticking out in front of it either (a pillar, a ledge).
        return !Physics.CheckBox(centre + normal * 0.35f, new Vector3(w, h, 0.25f), Quaternion.LookRotation(normal, up), Solid, QueryTriggerInteraction.Ignore);
    }
}
