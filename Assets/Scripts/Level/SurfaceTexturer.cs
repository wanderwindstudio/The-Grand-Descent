using System.Collections.Generic;
using UnityEngine;

// Puts the dungeon textures on a built floor. Every solid piece (anything with a non-trigger collider)
// gets new UVs worked out from world positions, so one texture repeat always covers exactly 1 m
// whatever the size of the piece, and neighbouring pieces line up. Which texture a face gets:
// facing up = stone floor, facing down = ceiling, sideways = wall (stone, or brick in some rooms).
// A RoomSurface component on a piece overrides that (wooden platforms, forced brick, untextured).
public static class SurfaceTexturer
{
    enum Tex { Floor, Ceiling, Stone, Brick, Wood }

    static readonly Dictionary<Tex, Material> materials = new Dictionary<Tex, Material>();

    public static void Apply(GameObject floorRoot, TuningConfig t)
    {
        var art = EnvironmentArt.Current;
        if (art == null || art.stoneFloor == null) return; // no art: keep the gray-box colours
        var owner = floorRoot.AddComponent<FloorMeshes>();
        roomWalls.Clear();

        foreach (var filter in floorRoot.GetComponentsInChildren<MeshFilter>())
        {
            var surface = filter.GetComponentInParent<RoomSurface>();
            if (surface != null && surface.kind == RoomSurface.Kind.Untextured) continue;
            if (surface == null && !HasSolidCollider(filter.gameObject)) continue;
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || filter.sharedMesh == null) continue;
            if (!filter.sharedMesh.isReadable)
            {
                Debug.LogWarning($"Can't texture {filter.name}: its mesh isn't readable (enable Read/Write on the model).");
                continue;
            }

            var room = filter.GetComponentInParent<RoomModule>();
            Tex wall = WallFor(room, t);
            if (surface != null && surface.kind == RoomSurface.Kind.Stone) wall = Tex.Stone;
            if (surface != null && surface.kind == RoomSurface.Kind.Brick) wall = Tex.Brick;
            bool wood = surface != null && surface.kind == RoomSurface.Kind.Wood;

            var mesh = Retexture(filter.sharedMesh, filter.transform, wood, wall, out List<Tex> used);
            owner.meshes.Add(mesh);
            filter.sharedMesh = mesh;
            var mats = new Material[used.Count];
            for (int i = 0; i < used.Count; i++) mats[i] = MaterialFor(used[i], art);
            renderer.sharedMaterials = mats;
        }
    }

    static bool HasSolidCollider(GameObject go)
    {
        foreach (var c in go.GetComponents<Collider>()) if (!c.isTrigger) return true;
        return false;
    }

    // Brick in about a third of the rooms, decided per room from the floor's random seed.
    static readonly Dictionary<RoomModule, Tex> roomWalls = new Dictionary<RoomModule, Tex>();
    static Tex WallFor(RoomModule room, TuningConfig t)
    {
        if (room == null) return Tex.Stone; // corridors and sealed doorways
        if (roomWalls.TryGetValue(room, out Tex w)) return w;
        switch (room.walls)
        {
            case RoomModule.WallStyle.Stone: w = Tex.Stone; break;
            case RoomModule.WallStyle.Brick: w = Tex.Brick; break;
            default: w = Random.value < t.brickRoomChance ? Tex.Brick : Tex.Stone; break;
        }
        roomWalls[room] = w;
        return w;
    }

    // A copy of the mesh with one submesh per texture and UVs from world positions (1 unit = 1 m).
    // Every triangle gets its own three vertices, so faces never share UVs.
    static Mesh Retexture(Mesh source, Transform tr, bool wood, Tex wall, out List<Tex> used)
    {
        var verts = source.vertices;
        var normals = source.normals;
        bool hasNormals = normals != null && normals.Length == verts.Length;
        var tris = source.triangles;

        var outVerts = new List<Vector3>(tris.Length);
        var outNormals = new List<Vector3>(tris.Length);
        var outUv = new List<Vector2>(tris.Length);
        var groups = new Dictionary<Tex, List<int>>();
        used = new List<Tex>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = tr.TransformPoint(verts[tris[i]]), b = tr.TransformPoint(verts[tris[i + 1]]), c = tr.TransformPoint(verts[tris[i + 2]]);
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            Tex tex = wood ? Tex.Wood : n.y > 0.7f ? Tex.Floor : n.y < -0.7f ? Tex.Ceiling : wall;
            if (!groups.TryGetValue(tex, out var list)) { list = new List<int>(); groups[tex] = list; used.Add(tex); }

            for (int k = 0; k < 3; k++)
            {
                int vi = tris[i + k];
                Vector3 w = k == 0 ? a : k == 1 ? b : c;
                list.Add(outVerts.Count);
                outVerts.Add(verts[vi]);
                outNormals.Add(hasNormals ? normals[vi] : tr.InverseTransformDirection(n));
                outUv.Add(WorldUv(w, n));
            }
        }

        var mesh = new Mesh { name = source.name + " (textured)" };
        if (outVerts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(outVerts);
        mesh.SetNormals(outNormals);
        mesh.SetUVs(0, outUv);
        mesh.subMeshCount = used.Count;
        for (int s = 0; s < used.Count; s++) mesh.SetTriangles(groups[used[s]], s);
        mesh.RecalculateBounds();
        return mesh;
    }

    // Project onto the plane the face mostly lies in. Walls keep "up" as up so bricks stay level.
    static Vector2 WorldUv(Vector3 p, Vector3 n)
    {
        float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
        if (ay >= ax && ay >= az) return new Vector2(p.x, p.z);
        if (ax >= az) return new Vector2(n.x > 0f ? -p.z : p.z, p.y);
        return new Vector2(n.z > 0f ? p.x : -p.x, p.y);
    }

    // The F1 brightness slider.
    public static void Refresh(TuningConfig t)
    {
        foreach (var m in materials.Values)
            if (m != null) m.SetColor("_BaseColor", Color.white * t.worldTextureBrightness);
    }

    static Material MaterialFor(Tex tex, EnvironmentArt art)
    {
        if (materials.TryGetValue(tex, out var m) && m != null) return m;
        Texture2D texture;
        switch (tex)
        {
            case Tex.Ceiling: texture = art.stoneCeiling; break;
            case Tex.Stone: texture = art.stoneWall; break;
            case Tex.Brick: texture = art.brickWall != null ? art.brickWall : art.stoneWall; break;
            case Tex.Wood: texture = art.woodPlanks; break;
            default: texture = art.stoneFloor; break;
        }
        m = Mats.New(Color.white * Game.Tuning.worldTextureBrightness);
        m.name = "World " + tex;
        m.SetTexture("_BaseMap", texture);
        m.SetFloat("_Smoothness", 0f);
        materials[tex] = m;
        return m;
    }
}
