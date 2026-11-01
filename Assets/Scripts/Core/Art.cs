using UnityEngine;

// Art assets handed over by GameBootstrap. Null when missing; visuals then fall back to gray-box.
public static class Art
{
    public static Material BillboardMaterial;
    public static Texture2D GoblinIdle;

    public static bool HasGoblinSprite => BillboardMaterial != null && GoblinIdle != null;

    // A billboard quad with its pivot at the bottom-centre: the returned transform sits at the feet.
    public static Transform CreateBillboard(string name, Transform parent, Texture2D texture, out Renderer renderer)
    {
        var pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Quad";
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(pivot, false);

        renderer = quad.GetComponent<Renderer>();
        var material = new Material(BillboardMaterial);
        material.SetTexture("_BaseMap", texture);
        material.EnableKeyword("_EMISSION"); // for hit flashes and the wind-up glow
        material.SetColor("_EmissionColor", Color.black);
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return pivot;
    }

    // Size the quad so the drawn character is the given height with its feet on the pivot.
    public static void SizeBillboard(Transform pivot, Texture2D texture, float characterHeight, float fillFraction)
    {
        float h = characterHeight / Mathf.Max(0.01f, fillFraction);
        float w = h * texture.width / texture.height;
        var quad = pivot.GetChild(0);
        quad.localScale = new Vector3(w, h, 1f);
        quad.localPosition = Vector3.up * (h / 2f);
    }

    // Turn a billboard around the vertical axis only, so it faces the camera.
    public static void FaceCamera(Transform pivot)
    {
        Vector3 away = pivot.position - Game.Camera.transform.position;
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f) pivot.rotation = Quaternion.LookRotation(away);
    }
}
