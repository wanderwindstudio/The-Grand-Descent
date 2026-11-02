using UnityEngine;

// Arcane Missile: hold Fire to auto-cast. Missiles leave the left hand and fly to the crosshair point.
// Damage and fire rate grow with Arcane Charges; at max charges Splitting Missiles fires three.
// At high Buzz the next shot may be swapped for a Huge Orb or a Spark Spray (see SpellSwap).
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
        if (Game.InputBlocked || Game.PlayerHealth.IsDead || Time.timeScale == 0f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return; // the click that grabs the mouse shouldn't shoot

        if (Controls.Fire.IsPressed() && Time.time >= nextFireTime)
        {
            float rate = upgrades.FireRate * Game.Charges.FireRateMultiplier * Game.Buzz.FireRateMultiplier;
            nextFireTime = Time.time + 1f / rate;
            Fire();
        }
    }

    void Fire()
    {
        var t = Game.Tuning;
        Vector3 origin = Aim.HandOrigin();
        Vector3 aim = (Aim.CrosshairTarget() - origin).normalized;
        Transform cam = Game.Camera.transform;

        var shot = new MissileShot
        {
            direction = aim,
            damage = upgrades.Damage * Game.Charges.DamageMultiplier * Game.Buzz.DamageMultiplier,
            pierce = t.missilePierce,
            speed = t.missileSpeed,
            hitRadius = t.missileRadius,
            length = t.arcaneMissileLength,
            wobble = Game.Buzz.Normalized * (BuzzMeter.Hangover ? t.hangoverWobble : t.wobbleMaxAmplitude),
            tint = Color.white,
            countsHits = true,
            countsMisses = true,
        };

        switch (spellSwap.ConsumeArmed())
        {
            case SpellVariant.HugeOrb:
                // Buzz variants neither build nor reset charges.
                shot.damage *= t.hugeOrbDamageMultiplier;
                shot.pierce += t.hugeOrbExtraPierce;
                shot.speed *= t.hugeOrbSpeedMultiplier;
                shot.hitRadius = t.hugeOrbHitRadius;
                shot.length = t.hugeOrbSize * 1.5f;
                shot.tint = Palette.HugeOrb;
                shot.countsHits = shot.countsMisses = false;
                pool.Get(origin).Launch(shot);
                break;

            case SpellVariant.Sparks:
                shot.damage *= t.sparkDamageMultiplier;
                shot.speed *= t.sparkSpeedMultiplier;
                shot.hitRadius *= 0.5f;
                shot.length = t.sparkSize * 3f;
                shot.tint = Palette.Spark;
                shot.countsHits = shot.countsMisses = false;
                for (int i = 0; i < t.sparkCount; i++)
                {
                    float s = t.sparkSpreadDegrees;
                    shot.direction = Quaternion.AngleAxis(Random.Range(-s, s), cam.up) * Quaternion.AngleAxis(Random.Range(-s, s), cam.right) * aim;
                    pool.Get(origin).Launch(shot);
                }
                break;

            default:
                bool split = upgrades.Has(UpgradeType.SplittingMissiles) && Game.Charges.AtMax;
                pool.Get(origin).Launch(shot);
                if (split)
                {
                    // Side missiles can build charges but never reset them; only the centre one counts as a miss.
                    shot.countsMisses = false;
                    foreach (float sign in new[] { -1f, 1f })
                    {
                        shot.direction = Quaternion.AngleAxis(sign * t.splitShotSpreadDegrees, cam.up) * aim;
                        pool.Get(origin).Launch(shot);
                    }
                }
                break;
        }

        FirstPersonHands.Instance?.OnMissileCast();
    }

    Missile CreateMissile()
    {
        var go = new GameObject("Arcane Missile");
        go.SetActive(false);

        var sprite = Art.CreateFxQuad("Sprite", Art.ArcaneMissile);
        sprite.transform.SetParent(go.transform, false);

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;

        var missile = go.AddComponent<Missile>();
        missile.SetPool(pool);
        return missile;
    }
}
