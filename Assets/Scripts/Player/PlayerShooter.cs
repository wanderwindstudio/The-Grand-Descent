using UnityEngine;
using UnityEngine.InputSystem;

// Left mouse fires magic missiles from the camera. Hold to auto-fire.
public class PlayerShooter : MonoBehaviour
{
    SimplePool<Missile> pool;
    PlayerUpgrades upgrades;
    SpellSwap spellSwap;
    float nextFireTime;

    void Awake()
    {
        pool = new SimplePool<Missile>(CreateMissile);
        upgrades = GetComponent<PlayerUpgrades>();
        spellSwap = GetComponent<SpellSwap>();
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || Game.InputBlocked || Game.PlayerHealth.IsDead || Time.timeScale == 0f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return; // the click that grabs the mouse shouldn't shoot

        if (mouse.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / (upgrades.FireRate * Game.Buzz.FireRateMultiplier);
            Fire();
        }
    }

    void Fire()
    {
        var t = Game.Tuning;
        Transform cam = Game.Camera.transform;
        Vector3 origin = cam.position + cam.forward * t.missileSpawnOffset;
        float damage = upgrades.Damage * Game.Buzz.DamageMultiplier;
        float wobble = Game.Buzz.Normalized * t.wobbleMaxAmplitude;

        switch (spellSwap.ConsumeArmed())
        {
            case SpellVariant.HugeOrb:
                pool.Get(origin).Launch(cam.forward, damage * t.hugeOrbDamageMultiplier, upgrades.Pierce + t.hugeOrbExtraPierce,
                    t.missileSpeed * t.hugeOrbSpeedMultiplier, t.hugeOrbHitRadius, t.hugeOrbSize, wobble, Palette.HugeOrb);
                break;

            case SpellVariant.Sparks:
                for (int i = 0; i < t.sparkCount; i++)
                {
                    float s = t.sparkSpreadDegrees;
                    Vector3 dir = cam.rotation * Quaternion.Euler(Random.Range(-s, s), Random.Range(-s, s), 0f) * Vector3.forward;
                    pool.Get(origin).Launch(dir, damage * t.sparkDamageMultiplier, upgrades.Pierce,
                        t.missileSpeed * t.sparkSpeedMultiplier, t.missileRadius * 0.5f, t.sparkSize, wobble, Palette.Spark);
                }
                break;

            default:
                int count = upgrades.MissileCount;
                for (int i = 0; i < count; i++)
                {
                    // Spread extra missiles evenly left and right of the crosshair.
                    float angle = (i - (count - 1) / 2f) * t.splitShotSpreadDegrees;
                    Vector3 dir = Quaternion.AngleAxis(angle, cam.up) * cam.forward;
                    pool.Get(origin).Launch(dir, damage, upgrades.Pierce,
                        t.missileSpeed, t.missileRadius, t.missileVisualSize, wobble, Palette.Missile);
                }
                break;
        }
    }

    Missile CreateMissile()
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Missile";
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>()); // must be gone before its first sphere cast
        var rend = go.GetComponent<Renderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;
        light.color = Palette.Missile;
        light.range = t.missileLightRange;
        light.intensity = t.missileLightIntensity;

        var missile = go.AddComponent<Missile>();
        missile.SetPool(pool);
        return missile;
    }
}
