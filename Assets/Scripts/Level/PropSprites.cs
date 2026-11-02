using UnityEngine;

// Sprite props and decals: lit, alpha-cutout quads (the same material as the goblins), so torches light
// them and fog fades them. Props stand on a bottom-centre pivot; decals lie flat on a surface.
public static class PropSprites
{
    public static bool Ready => Art.BillboardMaterial != null;

    // A prop that turns to face the camera. height = height of the whole 256 x 256 canvas in metres.
    public static Transform Billboard(string name, Transform parent, Vector3 position, Texture2D texture, float height, out Renderer renderer)
    {
        var pivot = Art.CreateBillboard(name, parent, texture, out renderer);
        pivot.position = position;
        Art.SizeBillboard(pivot, texture, height, 1f);
        pivot.gameObject.AddComponent<BillboardFacer>();
        return pivot;
    }

    // A flat quad lying on a surface: centre on the surface, facing out along the normal.
    // spin turns it around the normal (degrees). Lifted a little off the surface so it never flickers.
    public static Renderer Decal(string name, Transform parent, Vector3 point, Vector3 normal, Texture2D texture, float width, float height, float spin = 0f)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);
        Vector3 up = Mathf.Abs(normal.y) > 0.9f ? Vector3.forward : Vector3.up;
        quad.transform.SetPositionAndRotation(point + normal * 0.015f, Quaternion.LookRotation(-normal, up) * Quaternion.Euler(0f, 0f, spin));
        quad.transform.localScale = new Vector3(width, height, 1f);

        var renderer = quad.GetComponent<Renderer>();
        renderer.sharedMaterial = DecalMaterial(texture);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return renderer;
    }

    static readonly System.Collections.Generic.Dictionary<Texture2D, Material> decalMaterials = new System.Collections.Generic.Dictionary<Texture2D, Material>();

    static Material DecalMaterial(Texture2D texture)
    {
        if (decalMaterials.TryGetValue(texture, out var m) && m != null) return m;
        m = new Material(Art.BillboardMaterial) { name = "Decal " + texture.name };
        m.SetTexture("_BaseMap", texture);
        m.SetColor("_EmissionColor", Color.black);
        decalMaterials[texture] = m;
        return m;
    }
}
