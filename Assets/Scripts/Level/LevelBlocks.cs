using UnityEngine;

// Gray-box building blocks made in code (corridors, sealed doorways, the fallback room).
public static class LevelBlocks
{
    public static GameObject Block(GameObject parent, string name, Vector3 localPosition, Vector3 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = Mats.Solid(color);
        return go;
    }
}
