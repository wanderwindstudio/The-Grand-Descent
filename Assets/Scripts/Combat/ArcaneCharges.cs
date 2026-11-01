using UnityEngine;

// Arcane Charges: hitting again within the charge window adds a charge (up to the max).
// Each charge adds missile damage, fire rate and a little Buzz.
// A miss, or a full charge window without a hit, resets them (Overload: misses cost health instead).
public class ArcaneCharges : MonoBehaviour
{
    public int Charges { get; private set; }
    public int Max => upgrades.MaxCharges;
    public bool AtMax => Charges >= Max;
    // HUD flashes the pips until this (unscaled) time after a reset.
    public float ResetFlashUntil { get; private set; }

    public float DamageMultiplier => 1f + Game.Tuning.chargeDamageBonus * Charges;
    public float FireRateMultiplier => 1f + Game.Tuning.chargeFireRateBonus * Charges;

    PlayerUpgrades upgrades;
    float lastHitTime = -999f;

    void Awake() => upgrades = GetComponent<PlayerUpgrades>();

    public void RegisterHit()
    {
        var t = Game.Tuning;
        if (Time.time - lastHitTime <= t.chargeWindow && Charges < Max)
        {
            Charges++;
            Game.Buzz.Add(t.buzzPerCharge);
            if (Charges == Max) RunStats.MaxChargesReached++;
        }
        lastHitTime = Time.time;
    }

    public void RegisterMiss()
    {
        if (upgrades.Has(UpgradeType.Overload))
        {
            Game.PlayerHealth.LoseHealth(Game.Tuning.overloadMissCost);
            return;
        }
        ResetCharges();
    }

    void Update()
    {
        if (Charges > 0 && Time.time - lastHitTime > Game.Tuning.chargeWindow) ResetCharges();
    }

    void ResetCharges()
    {
        if (Charges == 0) return;
        Charges = 0;
        ResetFlashUntil = Time.unscaledTime + 0.5f;
    }
}
