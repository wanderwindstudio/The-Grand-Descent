using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// Turns a FloorTemplate into geometry: floor, ceiling, walls, torches. Bakes the NavMesh at the end.
public static class FloorBuilder
{
    static readonly Vector2Int[] FourDirections = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

    public static LevelLayout Build(TuningConfig t, FloorTemplate template)
    {
        var layout = new LevelLayout();
        var root = new GameObject("Floor - " + template.name);
        layout.root = root;

        string[] rows = template.rows;
        int h = rows.Length, w = 0;
        foreach (var r in rows) w = Mathf.Max(w, r.Length);
        float cs = t.cellSize, wallH = t.wallHeight;

        char At(int x, int y) => y >= 0 && y < h && x >= 0 && x < rows[y].Length ? rows[y][x] : '#';
        bool IsFloor(int x, int y) => ".PSHO".IndexOf(At(x, y)) >= 0;
        Vector3 CellToWorld(int x, int y) => new Vector3((x - w / 2f + 0.5f) * cs, 0f, (h / 2f - y - 0.5f) * cs);

        bool NeedsWall(int x, int y)
        {
            if (IsFloor(x, y)) return false;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (IsFloor(x + dx, y + dy)) return true;
            return false;
        }

        Block(root, "Floor", new Vector3(0f, -0.25f, 0f), new Vector3(w * cs, 0.5f, h * cs), Palette.Floor);
        Block(root, "Ceiling", new Vector3(0f, wallH + 0.25f, 0f), new Vector3(w * cs, 0.5f, h * cs), Palette.Wall);

        // Walls: join each horizontal run of wall cells into one long block.
        for (int y = 0; y < h; y++)
        {
            int x = 0;
            while (x < w)
            {
                if (!NeedsWall(x, y)) { x++; continue; }
                int start = x;
                while (x < w && NeedsWall(x, y)) x++;
                Vector3 centre = (CellToWorld(start, y) + CellToWorld(x - 1, y)) / 2f + Vector3.up * (wallH / 2f);
                Block(root, "Wall", centre, new Vector3((x - start) * cs, wallH, cs), Palette.Wall);
            }
        }

        // Markers
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Vector3 pos = CellToWorld(x, y);
                switch (At(x, y))
                {
                    case 'P': layout.playerStart = pos + Vector3.up * 0.05f; break;
                    case 'S': layout.spawnPoints.Add(pos); break;
                    case 'H': layout.shrineSpots.Add(pos); break;
                    case 'O': layout.portalSpot = pos; break;
                    case 'T':
                        foreach (var d in FourDirections)
                        {
                            // Grid y grows southward, world z grows northward.
                            if (!IsFloor(x + d.x, y + d.y)) continue;
                            Vector3 toFloor = new Vector3(d.x, 0f, -d.y);
                            LevelLighting.AddTorch(root, pos + toFloor * (cs / 2f + 0.15f) + Vector3.up * t.torchHeight, t);
                            break;
                        }
                        break;
                }
            }
        }
        layout.playerStartYaw = 0f; // face north

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
