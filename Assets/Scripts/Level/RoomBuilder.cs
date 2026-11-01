using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// Milestone 1: one square room with pillars and torches. Bakes the NavMesh at the end.
public static class RoomBuilder
{
    // Pillar positions as fractions of half the room size.
    static readonly Vector2[] PillarSpots =
    {
        new Vector2(-0.45f, -0.3f), new Vector2(0.45f, -0.3f),
        new Vector2(-0.45f, 0.35f), new Vector2(0.45f, 0.35f),
        new Vector2(0f, 0.6f), new Vector2(0f, 0.05f),
    };

    public static LevelLayout Build(TuningConfig t)
    {
        var layout = new LevelLayout();
        var root = new GameObject("Level");
        layout.root = root;

        float size = t.roomSize, half = size / 2f, h = t.wallHeight, w = t.wallThickness;

        Block(root, "Floor", new Vector3(0f, -0.25f, 0f), new Vector3(size, 0.5f, size), Palette.Floor);
        Block(root, "Ceiling", new Vector3(0f, h + 0.25f, 0f), new Vector3(size, 0.5f, size), Palette.Wall);
        Block(root, "Wall N", new Vector3(0f, h / 2f, half), new Vector3(size + w, h, w), Palette.Wall);
        Block(root, "Wall S", new Vector3(0f, h / 2f, -half), new Vector3(size + w, h, w), Palette.Wall);
        Block(root, "Wall E", new Vector3(half, h / 2f, 0f), new Vector3(w, h, size + w), Palette.Wall);
        Block(root, "Wall W", new Vector3(-half, h / 2f, 0f), new Vector3(w, h, size + w), Palette.Wall);

        foreach (var spot in PillarSpots)
            Block(root, "Pillar", new Vector3(spot.x * half, h / 2f, spot.y * half), new Vector3(t.pillarSize, h, t.pillarSize), Palette.Pillar);

        float inner = half - w;
        LevelLighting.AddTorch(root, new Vector3(0f, t.torchHeight, inner), t);
        LevelLighting.AddTorch(root, new Vector3(0f, t.torchHeight, -inner), t);
        LevelLighting.AddTorch(root, new Vector3(inner, t.torchHeight, 0f), t);
        LevelLighting.AddTorch(root, new Vector3(-inner, t.torchHeight, 0f), t);

        // Spawn points around the edge. The wave spawner picks ones you can't see.
        float edge = half - 2.5f;
        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
                if (x != 0 || z != 0)
                    layout.spawnPoints.Add(new Vector3(x * edge, 0f, z * edge));

        // Shrines go in the far corners and on the side walls, close to where goblins come in.
        float shrine = half - 4f;
        layout.shrineSpots.Add(new Vector3(-shrine, 0f, shrine));
        layout.shrineSpots.Add(new Vector3(shrine, 0f, shrine));
        layout.shrineSpots.Add(new Vector3(-shrine, 0f, 0f));
        layout.shrineSpots.Add(new Vector3(shrine, 0f, 0f));

        layout.playerStart = new Vector3(0f, 0.05f, -half + 3f);
        layout.playerStartYaw = 0f;

        var surface = root.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        return layout;
    }

    public static GameObject Block(GameObject parent, string name, Vector3 position, Vector3 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = Mats.Solid(color);
        return go;
    }
}
