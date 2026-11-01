using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum UpgradeType
{
    // Shared
    QuickHands, ArcaneWeight, FireWisp, PoisonTrail, SecondWind,
    // Arcane kit
    SplittingMissiles, Implosion, DeepFocus, EventHorizon, Overload, Blink,
}

// Which upgrades the player has, how many of each, and the resulting spell stats.
public class PlayerUpgrades : MonoBehaviour
{
    public static readonly UpgradeType[] All = (UpgradeType[])System.Enum.GetValues(typeof(UpgradeType));

    readonly int[] stacks = new int[All.Length];
    public readonly List<UpgradeType> TakenInOrder = new List<UpgradeType>();

    public int Stacks(UpgradeType u) => stacks[(int)u];
    public bool Has(UpgradeType u) => stacks[(int)u] > 0;

    // Missile stats after upgrades (Arcane Charges and Buzz are applied on top by the caster)
    public float FireRate => Game.Tuning.fireRate * (1f + Game.Tuning.quickHandsBonus * Stacks(UpgradeType.QuickHands));
    public float Damage => Game.Tuning.missileDamage * (1f + Game.Tuning.arcaneWeightBonus * Stacks(UpgradeType.ArcaneWeight));
    public int MaxCharges => Game.Tuning.maxCharges + Stacks(UpgradeType.DeepFocus);

    // Vortex stats after upgrades
    public float VortexRadius => Game.Tuning.vortexRadius * (1f + Game.Tuning.eventHorizonRadiusBonus * Stacks(UpgradeType.EventHorizon));
    public float VortexDuration => Game.Tuning.vortexDuration + Game.Tuning.eventHorizonDurationBonus * Stacks(UpgradeType.EventHorizon);

    // Upgrades that do nothing more when taken twice are only offered once.
    public static bool IsUnique(UpgradeType u) =>
        u == UpgradeType.SplittingMissiles || u == UpgradeType.Overload || u == UpgradeType.Blink;

    public static bool IsArcane(UpgradeType u) => u >= UpgradeType.SplittingMissiles;

    public bool CanOffer(UpgradeType u) => !(IsUnique(u) && Has(u));

    public void Apply(UpgradeType u)
    {
        stacks[(int)u]++;
        TakenInOrder.Add(u);
        if (u == UpgradeType.SecondWind)
            Game.PlayerHealth.AddMaxHealthAndHeal(Game.Tuning.secondWindHealth);
        // Everything else is read by the systems that use it, nothing to do here.
    }

    public string Summary()
    {
        var sb = new StringBuilder();
        foreach (var u in All)
            if (Stacks(u) > 0) sb.Append(Name(u)).Append(IsUnique(u) ? "" : " x" + Stacks(u)).Append('\n');
        return sb.ToString();
    }

    public static string Name(UpgradeType u)
    {
        switch (u)
        {
            case UpgradeType.QuickHands: return "Quick Hands";
            case UpgradeType.ArcaneWeight: return "Arcane Weight";
            case UpgradeType.FireWisp: return "Fire Wisp";
            case UpgradeType.PoisonTrail: return "Poison Trail";
            case UpgradeType.SecondWind: return "Second Wind";
            case UpgradeType.SplittingMissiles: return "Splitting Missiles";
            case UpgradeType.Implosion: return "Implosion";
            case UpgradeType.DeepFocus: return "Deep Focus";
            case UpgradeType.EventHorizon: return "Event Horizon";
            case UpgradeType.Overload: return "Overload";
            default: return "Blink";
        }
    }

    public static string Description(UpgradeType u)
    {
        var t = Game.Tuning;
        switch (u)
        {
            case UpgradeType.QuickHands: return $"+{t.quickHandsBonus * 100f:0}% fire rate";
            case UpgradeType.ArcaneWeight: return $"+{t.arcaneWeightBonus * 100f:0}% missile damage";
            case UpgradeType.FireWisp: return "An orbiting fireball burns goblins it touches";
            case UpgradeType.PoisonTrail: return "Leave a damaging cloud behind while moving";
            case UpgradeType.SecondWind: return $"+{t.secondWindHealth:0} max health and heal to full";
            case UpgradeType.SplittingMissiles: return "At max Arcane Charges, missiles split into three";
            case UpgradeType.Implosion: return "The vortex explodes when it ends - harder the more enemies it holds";
            case UpgradeType.DeepFocus: return "+1 max Arcane Charge";
            case UpgradeType.EventHorizon: return $"Vortex radius +{t.eventHorizonRadiusBonus * 100f:0}% and +{t.eventHorizonDurationBonus:0} s";
            case UpgradeType.Overload: return $"Missing no longer resets charges, but each miss costs {t.overloadMissCost:0} health";
            default: return $"Dash becomes a {t.blinkDistance:0} m teleport";
        }
    }
}
