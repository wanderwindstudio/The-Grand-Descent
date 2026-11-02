using UnityEngine;
using UnityEngine.UI;

// Soft purple smoke puffs drifting slowly across the menu background. UI images, so they sit
// exactly between the background and the buttons.
public class MenuSmoke : MonoBehaviour
{
    const int Count = 16;
    static readonly Color Purple = new Color(0.62f, 0.3f, 0.85f);

    class Puff
    {
        public RectTransform rect;
        public RawImage image;
        public Vector2 velocity;
        public float age, life, size, maxAlpha, spin;
    }

    readonly Puff[] puffs = new Puff[Count];
    RectTransform area;
    Texture2D texture;

    public static MenuSmoke Create(Transform parent)
    {
        var area = UiKit.Stretch(UiKit.Rect("Smoke", parent));
        var smoke = area.gameObject.AddComponent<MenuSmoke>();
        smoke.area = area;
        smoke.texture = MakePuffTexture(64);
        for (int i = 0; i < Count; i++)
        {
            var rect = UiKit.Rect("Puff", area);
            var img = rect.gameObject.AddComponent<RawImage>();
            img.texture = smoke.texture;
            img.raycastTarget = false;
            smoke.puffs[i] = new Puff { rect = rect, image = img };
            smoke.Respawn(smoke.puffs[i], Random.value); // start mid-life, so the screen isn't empty at first
        }
        return smoke;
    }

    void Respawn(Puff p, float startAge01)
    {
        Vector2 size = area.rect.size;
        if (size.x < 1f) size = new Vector2(1920f, 1080f);
        p.life = Random.Range(12f, 22f);
        p.age = p.life * startAge01;
        p.size = Random.Range(260f, 620f);
        p.maxAlpha = Random.Range(0.06f, 0.16f);
        p.spin = Random.Range(-6f, 6f);
        p.velocity = new Vector2(Random.Range(8f, 28f), Random.Range(4f, 16f));
        p.rect.anchorMin = p.rect.anchorMax = Vector2.zero;
        // Mostly low and to the left, drifting up and right.
        p.rect.anchoredPosition = new Vector2(Random.Range(-0.1f, 0.8f) * size.x, Random.Range(-0.15f, 0.6f) * size.y) - p.velocity * p.age;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        foreach (var p in puffs)
        {
            p.age += dt;
            if (p.age >= p.life) { Respawn(p, 0f); continue; }
            float k = p.age / p.life;
            float alpha = p.maxAlpha * Mathf.Sin(k * Mathf.PI); // fade in, then out
            p.rect.anchoredPosition += p.velocity * dt;
            float s = p.size * Mathf.Lerp(0.7f, 1.3f, k);
            p.rect.sizeDelta = new Vector2(s, s * 0.7f);
            p.rect.localRotation = Quaternion.Euler(0f, 0f, p.spin * p.age);
            p.image.color = new Color(Purple.r, Purple.g, Purple.b, alpha);
        }
    }

    // A soft, lumpy blob: a radial falloff broken up by a little noise.
    static Texture2D MakePuffTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        float seed = Random.value * 100f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f, dy = (y + 0.5f) / size - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float noise = Mathf.PerlinNoise(seed + x * 0.09f, seed + y * 0.09f);
                float a = Mathf.Clamp01(1f - r) * Mathf.Lerp(0.5f, 1f, noise);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        tex.Apply();
        return tex;
    }
}
