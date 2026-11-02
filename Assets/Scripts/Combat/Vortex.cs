using System.Collections.Generic;
using UnityEngine;

// A spinning Arcane vortex. For its lifetime it pulls every goblin within its radius to the centre and holds
// them there (no moving, no attacking), ticking a little damage. With Implosion it explodes when it ends.
// Over a pit it hangs at rim height and lifts what it holds; when it ends, they drop in (ring out).
public class Vortex : MonoBehaviour
{
    Renderer sprite;
    Light glow;
    Vector3 ground;     // centre on the floor; goblins are pulled here
    float radius, duration, age, spin, tickTimer;
    int implosionStacks;
    RoomPit pit;        // set when the vortex hangs over a pit
    readonly List<GoblinEnemy> held = new List<GoblinEnemy>();

    public static void Create(Vector3 point, PlayerUpgrades upgrades)
    {
        var t = Game.Tuning;
        // Find the floor under the point, so a vortex thrown at a wall or ceiling still works on the ground.
        Vector3 ground = point;
        RoomPit pit = RoomPit.Above(point);
        if (pit != null) ground = new Vector3(point.x, pit.RimY, point.z); // hang over the pit at rim height
        else if (Physics.Raycast(point + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 20f, ~(1 << 2), QueryTriggerInteraction.Ignore))
            ground = hit.point;

        var go = new GameObject("Arcane Vortex");
        go.transform.position = ground + Vector3.up * t.vortexHeight;
        var v = go.AddComponent<Vortex>();
        v.ground = ground;
        v.pit = pit;
        v.radius = upgrades.VortexRadius;
        v.duration = upgrades.VortexDuration;
        v.implosionStacks = upgrades.Stacks(UpgradeType.Implosion);

        v.sprite = Art.CreateFxQuad("Sprite", Art.ArcaneVortex);
        v.sprite.transform.SetParent(go.transform, false);
        v.sprite.transform.localScale = Vector3.zero;

        v.glow = new GameObject("Glow").AddComponent<Light>();
        v.glow.transform.SetParent(go.transform, false);
        v.glow.type = LightType.Point;
        v.glow.color = Palette.ArcaneLight;
        v.glow.range = t.vortexLightRange;
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        age += dt;
        if (age >= duration) { End(); return; }

        // Look: grow in fast, spin, pulse, shrink to nothing at the end.
        float grow = Mathf.Clamp01(age / t.vortexGrowTime);
        float shrink = Mathf.Clamp01((duration - age) / t.vortexShrinkTime);
        float pulse = 1f + t.vortexPulse * Mathf.Sin(age * 12f);
        float size = t.vortexSize * Mathf.Min(grow, shrink) * pulse;
        sprite.transform.localScale = Vector3.one * size;
        spin += t.vortexSpinSpeed * dt;
        Art.FaceCameraFully(sprite.transform, -spin); // clockwise, matching the art's swirl
        Art.SetFxColor(sprite, Color.white, t.spellSpriteBrightness, t.vortexOpacity * Mathf.Min(1f, shrink * 1.5f));
        glow.intensity = t.vortexLightIntensity * Mathf.Min(grow, shrink) * pulse;

        // Pull and hold everything inside the radius.
        held.Clear();
        foreach (var g in GoblinEnemy.Active)
        {
            Vector3 d = g.transform.position - ground;
            float below = -d.y;
            d.y = 0f;
            if (d.magnitude > radius) continue;
            if (pit != null)
            {
                // Over a pit: also reaches goblins down in it, and lifts them all off the floor.
                if (below > ground.y - pit.Bounds.min.y + 0.5f || below < -2.5f) continue;
                g.HoldOverPit(transform.position, t.vortexPullSpeed);
            }
            else
            {
                if (Mathf.Abs(below) > 2.5f) continue; // not someone on a balcony above, or a pit below
                g.Hold(ground, t.vortexPullSpeed, t.vortexHoldRadius);
            }
            held.Add(g);
        }

        tickTimer -= dt;
        if (tickTimer <= 0f)
        {
            tickTimer = t.vortexTickInterval;
            float damage = t.vortexTickDamage * Game.Buzz.DamageMultiplier;
            foreach (var g in held.ToArray()) g.TakeDamage(damage);
        }
    }

    void End()
    {
        if (implosionStacks > 0) Implode();
        Destroy(gameObject);
    }

    // Implosion: one big hit to everything in range, stronger for every goblin the vortex was holding.
    void Implode()
    {
        var t = Game.Tuning;
        float damage = implosionStacks * (t.implosionBaseDamage + t.implosionDamagePerHeld * held.Count) * Game.Buzz.DamageMultiplier;
        foreach (var g in GoblinEnemy.Active.ToArray())
        {
            Vector3 d = g.transform.position - ground;
            d.y = 0f;
            if (d.magnitude <= radius) g.TakeDamage(damage);
        }
        SpellFx.Impact(transform.position, t.vortexSize * 0.5f, radius * 1.2f, 0.45f, t.vortexLightIntensity * 3f, radius * 2.5f);
        Hud.ShowMessage(held.Count > 0 ? $"IMPLOSION x{held.Count}" : "IMPLOSION");
    }
}
