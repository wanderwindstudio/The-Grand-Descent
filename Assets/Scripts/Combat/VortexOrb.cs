using UnityEngine;

// The thrown orb: a small spinning copy of the vortex sprite. Turns into a vortex on contact or at max range.
public class VortexOrb : MonoBehaviour
{
    const float HitRadius = 0.3f;

    Renderer sprite;
    Vector3 direction;
    float travelled, spin;
    PlayerUpgrades upgrades;

    public static void Throw(Vector3 origin, Vector3 direction, PlayerUpgrades upgrades)
    {
        var go = new GameObject("Vortex Orb");
        go.transform.position = origin;
        var orb = go.AddComponent<VortexOrb>();
        orb.direction = direction;
        orb.upgrades = upgrades;

        orb.sprite = Art.CreateFxQuad("Sprite", Art.ArcaneVortex);
        orb.sprite.transform.SetParent(go.transform, false);
        orb.sprite.transform.localScale = Vector3.one * Game.Tuning.vortexOrbSize;
        Art.SetFxColor(orb.sprite, Color.white, Game.Tuning.spellSpriteBrightness, 1f);

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;
        light.color = Palette.ArcaneLight;
        light.range = 4f;
        light.intensity = 2f;
    }

    void Update()
    {
        var t = Game.Tuning;
        float step = t.vortexOrbSpeed * Time.deltaTime;
        Vector3 from = transform.position;

        if (Physics.SphereCast(from, HitRadius, direction, out RaycastHit hit, step, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<PlayerHealth>() == null)
        {
            var goblin = hit.collider.GetComponentInParent<GoblinEnemy>();
            Burst(goblin != null ? goblin.transform.position : from + direction * hit.distance);
            return;
        }

        transform.position = from + direction * step;
        travelled += step;
        spin += t.vortexSpinSpeed * 2f * Time.deltaTime;
        Art.FaceCameraFully(sprite.transform, spin);
        if (travelled >= t.vortexMaxThrowDistance) Burst(transform.position);
    }

    void Burst(Vector3 where)
    {
        Vortex.Create(where, upgrades);
        Destroy(gameObject);
    }
}
