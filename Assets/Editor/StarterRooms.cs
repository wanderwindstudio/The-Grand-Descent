using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Menu: Grand Descent > Rebuild Starter Rooms.
// Builds the three gray-box starter rooms out of cubes and saves them as prefabs in Assets/Rooms:
// The Balcony, The Pillar Hall and The Pit. Running it again overwrites those three prefabs (not your own rooms).
// Every room: floor at height 0, walls 0.5 m thick outside the interior, doorways 3 m wide and 3.5 m high.
// Balconies, stairs, the ramp and bridges carry a RoomSurface (Wood); the rest is textured by face direction.
public static class StarterRooms
{
    const string Folder = "Assets/Rooms";
    const string MaterialFolder = "Assets/Rooms/Materials";
    const float Wall = 0.5f, DoorWidth = 3f, DoorHeight = 3.5f;

    static Material floorMat, wallMat, pillarMat, stairMat, ladderMat, pitMat;

    [MenuItem("Grand Descent/Rebuild Starter Rooms")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "Rooms");
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder(Folder, "Materials");
        floorMat = RoomMaterial("Room_Floor", Palette.Floor);
        wallMat = RoomMaterial("Room_Wall", Palette.Wall);
        pillarMat = RoomMaterial("Room_Pillar", Palette.Pillar);
        stairMat = RoomMaterial("Room_Stairs", Palette.Stairs);
        ladderMat = RoomMaterial("Room_Ladder", Palette.Ladder);
        pitMat = RoomMaterial("Room_PitFloor", Palette.PitFloor);

        Save(Balcony(), "Room_TheBalcony");
        Save(PillarHall(), "Room_ThePillarHall");
        Save(ThePit(), "Room_ThePit");
        RoomLibraryUpdater.Refresh();
    }

    public static bool Exist() => System.IO.File.Exists(Folder + "/Room_TheBalcony.prefab");

    // ---------------------------------------------------------------- The Balcony
    // Hall 24 x 18, 8 m high. A balcony 3.5 m up along the north wall, reached by one narrow staircase
    // (west, a chokepoint) and one ladder (middle). Below the east part of the balcony edge: a 3 m pit with steps out.
    static GameObject Balcony()
    {
        const float X = 12f, Z = 9f, H = 8f;
        var room = NewRoom("The Balcony");

        // Floor with a hole for the pit (x 4..12, z 0..5)
        Box(room, "Floor", new Vector3(-X - Wall, -0.5f, -Z - Wall), new Vector3(4f, 0f, Z + Wall), floorMat);
        Box(room, "Floor", new Vector3(4f, -0.5f, -Z - Wall), new Vector3(X + Wall, 0f, 0f), floorMat);
        Box(room, "Floor", new Vector3(4f, -0.5f, 5f), new Vector3(X + Wall, 0f, Z + Wall), floorMat);
        Box(room, "Ceiling", new Vector3(-X - Wall, H, -Z - Wall), new Vector3(X + Wall, H + 0.5f, Z + Wall), wallMat);
        Walls(room, X, Z, H, north: null, south: new[] { 0f }, east: new[] { -4.5f }, west: new[] { -4.5f });

        // The pit: 3 m deep, steps up on its west side
        Box(room, "Pit Floor", new Vector3(4f, -3.5f, 0f), new Vector3(X, -3f, 5f), pitMat);
        Box(room, "Pit Wall", new Vector3(3.5f, -3.5f, 0f), new Vector3(4f, -0.5f, 5f), wallMat);
        Box(room, "Pit Wall", new Vector3(4f, -3.5f, -0.5f), new Vector3(X, -0.5f, 0f), wallMat);
        Box(room, "Pit Wall", new Vector3(4f, -3.5f, 5f), new Vector3(X, -0.5f, 5.5f), wallMat);
        Box(room, "Pit Wall", new Vector3(X, -3.5f, 0f), new Vector3(X + Wall, 0f, 5f), wallMat);
        for (int i = 0; i < 12; i++) // 12 steps of 0.25 m, down northward from the south rim
            Box(room, "Pit Step", new Vector3(4f, -3f, 0.35f * i), new Vector3(5.5f, -0.25f * (i + 1), 0.35f * (i + 1)), stairMat);
        Pit(room, new Vector3(4f, -3f, 0f), new Vector3(X, -0.3f, 5f));
        Ledge(room, new Vector3(8.75f, 0f, 0f), 0f, 6.5f);   // south rim, drop north into the pit
        Ledge(room, new Vector3(8f, 0f, 5f), 180f, 7f);      // under the balcony, drop south into the pit

        // Balcony and its supports
        Box(room, "Balcony", new Vector3(-X, 3f, 5f), new Vector3(X, 3.5f, Z), floorMat);
        foreach (float x in new[] { -6f, 0f, 3f })
            Box(room, "Balcony Support", new Vector3(x - 0.6f, 0f, 5f), new Vector3(x + 0.6f, 3f, 5.6f), pillarMat);
        Ledge(room, new Vector3(1f, 3.5f, 5f), 180f, 22f); // goblins jump down (not over the pit: too high)

        // Staircase along the west wall: 14 steps up to the balcony, 1.6 m wide
        for (int i = 0; i < 14; i++)
            Box(room, "Step", new Vector3(-X, 0f, 0.1f + 0.35f * i), new Vector3(-X + 1.6f, 0.25f * (i + 1), 0.1f + 0.35f * (i + 1)), stairMat);

        Ladder(room, new Vector3(0f, 0f, 5f), 0f, 3.5f);

        Spot(room, RoomSpot.Kind.PlayerStart, new Vector3(0f, 0f, -6.5f));
        Spot(room, RoomSpot.Kind.Portal, new Vector3(-4f, 0f, -2.5f));
        foreach (var p in new[] { new Vector3(-9f, 0f, -7f), new Vector3(9f, 0f, -7f), new Vector3(-6f, 0f, 2f), new Vector3(-8f, 3.5f, 7.5f), new Vector3(8f, 3.5f, 7.5f) })
            Spot(room, RoomSpot.Kind.EnemySpawn, p);
        Spot(room, RoomSpot.Kind.Shrine, new Vector3(-3.5f, 3.5f, 7.6f));
        foreach (var p in new[] { new Vector3(6.5f, 0f, -0.9f), new Vector3(7f, 3.5f, 5.7f), new Vector3(-9.5f, 0f, -0.8f) })
            Spot(room, RoomSpot.Kind.Barrel, p);
        foreach (var p in new[] { new Vector3(-X + 0.15f, 2.6f, -1f), new Vector3(X - 0.15f, 2.6f, -7.5f), new Vector3(-6f, 2.6f, -Z + 0.15f),
                                  new Vector3(6f, 2.6f, -Z + 0.15f), new Vector3(-X + 0.15f, 2.6f, -7.5f), new Vector3(-6f, 6f, Z - 0.15f),
                                  new Vector3(6f, 6f, Z - 0.15f), new Vector3(X - 0.15f, 1.2f, 2.5f), new Vector3(0f, 2.2f, 5.75f) })
            Spot(room, RoomSpot.Kind.Torch, p);
        return room;
    }

    // ---------------------------------------------------------------- The Pillar Hall
    // Wide room 26 x 22, 6 m high. Six thick pillars to break line of sight and run loops around,
    // and four 1 m low walls to jump over (goblins hop them after a short pause).
    static GameObject PillarHall()
    {
        const float X = 13f, Z = 11f, H = 6f;
        var room = NewRoom("The Pillar Hall");
        Box(room, "Floor", new Vector3(-X - Wall, -0.5f, -Z - Wall), new Vector3(X + Wall, 0f, Z + Wall), floorMat);
        Box(room, "Ceiling", new Vector3(-X - Wall, H, -Z - Wall), new Vector3(X + Wall, H + 0.5f, Z + Wall), wallMat);
        Walls(room, X, Z, H, north: new[] { 0f }, south: new[] { 0f }, east: new[] { 0f }, west: new[] { 0f });

        foreach (float x in new[] { -8f, 0f, 8f })
            foreach (float z in new[] { -5f, 5f })
                Box(room, "Pillar", new Vector3(x - 1f, 0f, z - 1f), new Vector3(x + 1f, H, z + 1f), pillarMat);

        LowWall(room, new Vector3(-11f, 0f, -0.25f), new Vector3(-7f, 1f, 0.25f));
        LowWall(room, new Vector3(7f, 0f, -0.25f), new Vector3(11f, 1f, 0.25f));
        LowWall(room, new Vector3(-4.25f, 0f, -9.5f), new Vector3(-3.75f, 1f, -6.5f));
        LowWall(room, new Vector3(3.75f, 0f, 6.5f), new Vector3(4.25f, 1f, 9.5f));

        Spot(room, RoomSpot.Kind.PlayerStart, new Vector3(0f, 0f, -8.5f));
        Spot(room, RoomSpot.Kind.Portal, new Vector3(0f, 0f, 0f));
        foreach (var p in new[] { new Vector3(-11f, 0f, -9f), new Vector3(11f, 0f, -9f), new Vector3(-11f, 0f, 9f), new Vector3(11f, 0f, 9f), new Vector3(-11f, 0f, 4f), new Vector3(11f, 0f, -4f) })
            Spot(room, RoomSpot.Kind.EnemySpawn, p);
        Spot(room, RoomSpot.Kind.Shrine, new Vector3(-4.5f, 0f, 9f));
        foreach (var p in new[] { new Vector3(4f, 0f, -2.5f), new Vector3(-5f, 0f, 2.5f) })
            Spot(room, RoomSpot.Kind.Barrel, p);
        foreach (var p in new[] { new Vector3(-X + 0.15f, 2.6f, -6f), new Vector3(X - 0.15f, 2.6f, 6f), new Vector3(-6f, 2.6f, Z - 0.15f), new Vector3(6f, 2.6f, -Z + 0.15f) })
            Spot(room, RoomSpot.Kind.Torch, p);
        foreach (float x in new[] { -8f, 0f, 8f }) // one torch on the inner face of each pillar
            foreach (float z in new[] { -5f, 5f })
                Spot(room, RoomSpot.Kind.Torch, new Vector3(x, 2.6f, z - Mathf.Sign(z) * 1.15f));
        return room;
    }

    // ---------------------------------------------------------------- The Pit
    // 28 x 28, 7 m high. A ring walkway around a 12 x 12 pit, 4 m deep, with a ramp back up along its south side
    // and two narrow bridges (1.6 m) across it.
    static GameObject ThePit()
    {
        const float X = 14f, Z = 14f, H = 7f, P = 6f, Depth = 4f;
        var room = NewRoom("The Pit");

        Box(room, "Ring Floor", new Vector3(-X - Wall, -Depth - 0.5f, P), new Vector3(X + Wall, 0f, Z + Wall), floorMat);
        Box(room, "Ring Floor", new Vector3(-X - Wall, -Depth - 0.5f, -Z - Wall), new Vector3(X + Wall, 0f, -P), floorMat);
        Box(room, "Ring Floor", new Vector3(-X - Wall, -Depth - 0.5f, -P), new Vector3(-P, 0f, P), floorMat);
        Box(room, "Ring Floor", new Vector3(P, -Depth - 0.5f, -P), new Vector3(X + Wall, 0f, P), floorMat);
        Box(room, "Pit Floor", new Vector3(-P, -Depth - 0.5f, -P), new Vector3(P, -Depth, P), pitMat);
        Box(room, "Ceiling", new Vector3(-X - Wall, H, -Z - Wall), new Vector3(X + Wall, H + 0.5f, Z + Wall), wallMat);
        Walls(room, X, Z, H, north: new[] { 0f }, south: new[] { 0f }, east: new[] { 0f }, west: new[] { 0f });

        // Ramp from the pit floor (west end) up to the rim (east end), along the south side
        Vector3 low = new Vector3(-P + 0.4f, -Depth, -5.25f), high = new Vector3(P, 0f, -5.25f);
        float length = Vector3.Distance(low, high), angle = Mathf.Atan2(Depth, high.x - low.x) * Mathf.Rad2Deg;
        var ramp = Box(room, "Ramp", Vector3.zero, Vector3.one, stairMat);
        ramp.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        ramp.transform.localScale = new Vector3(length + 0.3f, 0.5f, 1.5f);
        ramp.transform.localPosition = (low + high) / 2f - ramp.transform.up * 0.25f;

        foreach (float z in new[] { -2.5f, 2.5f })
        {
            Box(room, "Bridge", new Vector3(-P, -0.4f, z - 0.8f), new Vector3(P, 0f, z + 0.8f), floorMat);
            Ledge(room, new Vector3(0f, 0f, z + 0.8f), 0f, 11f);
            Ledge(room, new Vector3(0f, 0f, z - 0.8f), 180f, 11f);
        }

        Pit(room, new Vector3(-P, -Depth, -P), new Vector3(P, -0.6f, P));
        Ledge(room, new Vector3(0f, 0f, P), 180f, 12f);
        Ledge(room, new Vector3(0f, 0f, -P), 0f, 12f);
        Ledge(room, new Vector3(-P, 0f, 0f), 90f, 12f);
        Ledge(room, new Vector3(P, 0f, 0f), 270f, 12f);

        Spot(room, RoomSpot.Kind.PlayerStart, new Vector3(0f, 0f, -11f));
        Spot(room, RoomSpot.Kind.Portal, new Vector3(0f, 0f, 10f));
        foreach (var p in new[] { new Vector3(-11f, 0f, -11f), new Vector3(11f, 0f, -11f), new Vector3(-11f, 0f, 11f), new Vector3(11f, 0f, 11f), new Vector3(-10f, 0f, 0f), new Vector3(3f, -Depth, 0f) })
            Spot(room, RoomSpot.Kind.EnemySpawn, p);
        Spot(room, RoomSpot.Kind.Shrine, new Vector3(-3f, -Depth, 0f));
        foreach (var p in new[] { new Vector3(-7.2f, 0f, 4f), new Vector3(7.2f, 0f, 4f), new Vector3(-3f, 0f, 7f) })
            Spot(room, RoomSpot.Kind.Barrel, p);
        foreach (var p in new[] { new Vector3(-X + 0.15f, 2.6f, -6f), new Vector3(X - 0.15f, 2.6f, 6f), new Vector3(6f, 2.6f, Z - 0.15f), new Vector3(-6f, 2.6f, -Z + 0.15f), new Vector3(P - 0.15f, -1.6f, 4f),
                                  new Vector3(-X + 0.15f, 2.6f, 6f), new Vector3(X - 0.15f, 2.6f, -6f), new Vector3(-6f, 2.6f, Z - 0.15f), new Vector3(6f, 2.6f, -Z + 0.15f),
                                  new Vector3(-P + 0.15f, -1.6f, -1f) })
            Spot(room, RoomSpot.Kind.Torch, p);
        return room;
    }

    // ---------------------------------------------------------------- helpers

    static GameObject NewRoom(string name)
    {
        var root = new GameObject(name);
        root.AddComponent<RoomModule>().displayName = name;
        return root;
    }

    static void Save(GameObject room, string fileName)
    {
        PrefabUtility.SaveAsPrefabAsset(room, $"{Folder}/{fileName}.prefab");
        Object.DestroyImmediate(room);
    }

    static Material RoomMaterial(string name, Color color)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            var template = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/GrayboxLit.mat");
            m = template != null ? new Material(template) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(m);
        return m;
    }

    // Pieces that get wooden planks in game (everything else is textured by face direction).
    static readonly string[] WoodPieces = { "Balcony", "Step", "Pit Step", "Ramp", "Bridge" };

    // An axis-aligned block from min corner to max corner.
    static GameObject Box(GameObject parent, string name, Vector3 min, Vector3 max, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = (min + max) / 2f;
        go.transform.localScale = max - min;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (System.Array.IndexOf(WoodPieces, name) >= 0) go.AddComponent<RoomSurface>().kind = RoomSurface.Kind.Wood;
        return go;
    }

    // Four walls around the interior x -X..X, z -Z..Z, each with doorways (positions along the wall) and a marker per doorway.
    static void Walls(GameObject room, float X, float Z, float H, float[] north, float[] south, float[] east, float[] west)
    {
        WallRun(room, "North Wall", true, Z, Z + Wall, -X - Wall, X + Wall, H, north);
        WallRun(room, "South Wall", true, -Z - Wall, -Z, -X - Wall, X + Wall, H, south);
        WallRun(room, "East Wall", false, X, X + Wall, -Z, Z, H, east);
        WallRun(room, "West Wall", false, -X - Wall, -X, -Z, Z, H, west);

        if (north != null) foreach (float d in north) Door(room, new Vector3(d, 0f, Z + Wall), 0f);
        if (south != null) foreach (float d in south) Door(room, new Vector3(d, 0f, -Z - Wall), 180f);
        if (east != null) foreach (float d in east) Door(room, new Vector3(X + Wall, 0f, d), 90f);
        if (west != null) foreach (float d in west) Door(room, new Vector3(-X - Wall, 0f, d), 270f);
    }

    // A straight wall from a to b along X (alongX) or Z, between t0 and t1 across it, with openings.
    static void WallRun(GameObject room, string name, bool alongX, float t0, float t1, float a, float b, float H, float[] doors)
    {
        Vector3 P(float along, float y, float across) => alongX ? new Vector3(along, y, across) : new Vector3(across, y, along);
        var openings = new List<float>(doors ?? new float[0]);
        openings.Sort();
        float start = a;
        foreach (float d in openings)
        {
            float left = d - DoorWidth / 2f, right = d + DoorWidth / 2f;
            if (left > start) Box(room, name, P(start, 0f, t0), P(left, H, t1), wallMat);
            Box(room, name + " Lintel", P(left, DoorHeight, t0), P(right, H, t1), wallMat);
            start = right;
        }
        if (b > start) Box(room, name, P(start, 0f, t0), P(b, H, t1), wallMat);
    }

    static void Door(GameObject room, Vector3 position, float yaw)
    {
        var go = Marker(room, "Doorway", position, yaw);
        var d = go.AddComponent<RoomDoorway>();
        d.width = DoorWidth;
        d.height = DoorHeight;
    }

    static void Spot(GameObject room, RoomSpot.Kind kind, Vector3 position, float yaw = 0f)
    {
        Marker(room, kind.ToString(), position, yaw).AddComponent<RoomSpot>().kind = kind;
    }

    static void Ledge(GameObject room, Vector3 position, float yaw, float length)
    {
        Marker(room, "Ledge", position, yaw).AddComponent<RoomLedge>().length = length;
    }

    static void LowWall(GameObject room, Vector3 min, Vector3 max)
    {
        Box(room, "Low Wall", min, max, wallMat).AddComponent<RoomLowWall>();
    }

    static void Pit(GameObject room, Vector3 min, Vector3 max)
    {
        var go = Marker(room, "Pit", (min + max) / 2f, 0f);
        var box = go.AddComponent<BoxCollider>();
        box.size = max - min;
        go.AddComponent<RoomPit>();
    }

    // Ladder: marker at its foot against the wall, facing into the wall, plus rails and rungs (no colliders).
    static void Ladder(GameObject room, Vector3 foot, float yaw, float height)
    {
        var go = Marker(room, "Ladder", foot, yaw);
        go.AddComponent<BoxCollider>();
        var ladder = go.AddComponent<RoomLadder>();
        ladder.height = height;
        ladder.Setup();

        var visual = new GameObject("Ladder Visual");
        visual.transform.SetParent(go.transform, false);
        foreach (float x in new[] { -0.3f, 0.3f })
            Visual(visual, new Vector3(x, (height + 0.6f) / 2f, -0.08f), new Vector3(0.08f, height + 0.6f, 0.08f));
        for (float y = 0.3f; y < height; y += 0.35f)
            Visual(visual, new Vector3(0f, y, -0.08f), new Vector3(0.6f, 0.05f, 0.05f));
    }

    static void Visual(GameObject parent, Vector3 pos, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Ladder Part";
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = ladderMat;
    }

    static GameObject Marker(GameObject room, string name, Vector3 position, float yaw)
    {
        var go = new GameObject(name);
        go.transform.SetParent(room.transform, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }
}
