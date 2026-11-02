using System.Collections.Generic;
using UnityEngine;

// Meshes made for one floor (textured copies, decals). Deleted together with the floor.
public class FloorMeshes : MonoBehaviour
{
    public readonly List<Mesh> meshes = new List<Mesh>();

    void OnDestroy()
    {
        foreach (var m in meshes) if (m != null) Destroy(m);
    }
}
