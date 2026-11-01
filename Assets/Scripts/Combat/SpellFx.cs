using UnityEngine;

// Short-lived spell effects: the Arcane impact burst (missile hits, Blink puffs, Implosion).
public class SpellFx : MonoBehaviour
{
    static SpellFx instance;
    SimplePool<ImpactFx> pool;

    void Awake()
    {
        instance = this;
        pool = new SimplePool<ImpactFx>(CreateImpact);
    }

    // A burst that grows from startSize to endSize metres and fades out, with a light flash.
    public static void Impact(Vector3 position, float startSize, float endSize, float duration, float lightIntensity, float lightRange)
    {
        if (instance == null) return;
        instance.pool.Get(position).Begin(instance.pool, startSize, endSize, duration, lightIntensity, lightRange);
    }

    public static void MissileImpact(Vector3 position)
    {
        var t = Game.Tuning;
        Impact(position, t.impactStartSize, t.impactEndSize, t.impactDuration, t.impactLightIntensity, t.impactLightRange);
    }

    static ImpactFx CreateImpact()
    {
        var root = new GameObject("Arcane Impact");
        root.SetActive(false);
        var fx = root.AddComponent<ImpactFx>();
        fx.sprite = Art.CreateFxQuad("Sprite", Art.ArcaneImpact);
        fx.sprite.transform.SetParent(root.transform, false);
        fx.flash = new GameObject("Flash").AddComponent<Light>();
        fx.flash.transform.SetParent(root.transform, false);
        fx.flash.type = LightType.Point;
        fx.flash.color = Palette.ArcaneLight;
        return fx;
    }

    public class ImpactFx : MonoBehaviour
    {
        public Renderer sprite;
        public Light flash;
        SimplePool<ImpactFx> pool;
        float age, duration, startSize, endSize, lightIntensity, roll;

        public void Begin(SimplePool<ImpactFx> owner, float fromSize, float toSize, float seconds, float intensity, float range)
        {
            pool = owner;
            age = 0f;
            duration = Mathf.Max(0.01f, seconds);
            startSize = fromSize;
            endSize = toSize;
            lightIntensity = intensity;
            flash.range = range;
            roll = Random.Range(0f, 360f);
            Update();
        }

        void Update()
        {
            age += Time.deltaTime;
            float k = age / duration;
            if (k >= 1f) { pool.Release(this); return; }

            float ease = 1f - (1f - k) * (1f - k); // fast start, soft end
            sprite.transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, ease);
            Art.FaceCameraFully(sprite.transform, roll);
            Art.SetFxColor(sprite, Color.white, Game.Tuning.spellSpriteBrightness, 1f - k * k);
            flash.intensity = lightIntensity * (1f - k);
        }
    }
}
