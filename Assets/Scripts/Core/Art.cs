using UnityEngine;

// Art assets handed over by GameBootstrap. Null when missing; visuals then fall back to gray-box.
public static class Art
{
    public static Material BillboardMaterial;
    public static Texture2D GoblinIdle;
    public static Texture2D GoblinWindUp;
    public static Texture2D GoblinHit;
    public static Texture2D GoblinDead;

    public static Material FxMaterial;
    public static Texture2D HandLeft, HandRight;
    public static Texture2D HandLeftCast;  // null if missing: the left hand then keeps its idle pose
    public static Texture2D HandRightSlam; // null if missing: the staff stays in its idle pose at max charges
    public static Texture2D IconMissile, IconVortex, IconExplosion, IconCharge;
    public static Texture2D IconExplosionGrey, IconChargeGrey; // greyscale copies for locked / empty
    public static Texture2D ArcaneMissile, ArcaneImpact, ArcaneVortex;

    public static bool HasGoblinSprite => BillboardMaterial != null && GoblinIdle != null;

    public static Texture2D SpellIcon(Spell spell)
    {
        switch (spell)
        {
            case Spell.ArcaneMissile: return IconMissile;
            case Spell.ArcaneVortex: return IconVortex;
            case Spell.ArcaneExplosion: return IconExplosion;
            default: return null;
        }
    }

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

    // A glowing effect quad (missile, impact, vortex). Faces the camera fully; the caller sets its rotation.
    // Brightness above 1 makes it bloom; alpha fades it out.
    public static Renderer CreateFxQuad(string name, Texture2D texture)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        var renderer = quad.GetComponent<Renderer>();
        renderer.sharedMaterial = new Material(FxMaterialOrFallback());
        renderer.sharedMaterial.SetTexture("_BaseMap", texture);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
    }

    public static void SetFxColor(Renderer fx, Color color, float brightness, float alpha)
    {
        fx.sharedMaterial.SetColor("_BaseColor", new Color(color.r * brightness, color.g * brightness, color.b * brightness, alpha));
    }

    // Face the camera fully, then spin by an angle in screen space (degrees, counter-clockwise).
    public static void FaceCameraFully(Transform t, float screenAngle)
    {
        t.rotation = Game.Camera.transform.rotation * Quaternion.Euler(0f, 0f, screenAngle);
    }

    static Material fxFallback;
    static Material FxMaterialOrFallback()
    {
        if (FxMaterial != null) return FxMaterial;
        if (fxFallback == null)
        {
            // Same settings the setup script gives mat_fxTransparent (works in the editor).
            fxFallback = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            fxFallback.SetFloat("_Surface", 1f);
            fxFallback.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fxFallback.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fxFallback.SetFloat("_ZWrite", 0f);
            fxFallback.SetFloat("_Cull", 0f);
            fxFallback.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            fxFallback.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        return fxFallback;
    }

    // A coloured stand-in for a missing art file: a soft disc, or a solid rounded square.
    public static Texture2D Placeholder(string name, Color color, bool square)
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Point };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f, dy = (y + 0.5f) / size - 0.5f;
                float a = square ? (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) < 0.45f ? 1f : 0f)
                                 : Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 2.2f);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, a));
            }
        tex.Apply();
        Debug.LogWarning($"Art missing - using a coloured placeholder for {name}.");
        return tex;
    }

    // Copy a texture's pixels at the given size. Works on textures that aren't marked readable,
    // by drawing them into a temporary render target and reading that back.
    public static Color32[] ReadPixels(Texture texture, int width, int height)
    {
        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(texture, rt);
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        var pixels = copy.GetPixels32(); // row 0 is the bottom of the image
        Object.Destroy(copy);
        return pixels;
    }

    // A grey version of a texture (same alpha), for empty or locked icons.
    public static Texture2D Greyscale(Texture2D texture)
    {
        int w = Mathf.Min(texture.width, 256), h = Mathf.Min(texture.height, 256);
        var pixels = ReadPixels(texture, w, h);
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            byte grey = (byte)(0.3f * p.r + 0.59f * p.g + 0.11f * p.b);
            pixels[i] = new Color32(grey, grey, grey, p.a);
        }
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = texture.name + "_grey", filterMode = texture.filterMode, wrapMode = TextureWrapMode.Clamp };
        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    // Turn a billboard around the vertical axis only, so it faces the camera.
    public static void FaceCamera(Transform pivot)
    {
        Vector3 away = pivot.position - Game.Camera.transform.position;
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f) pivot.rotation = Quaternion.LookRotation(away);
    }
}
