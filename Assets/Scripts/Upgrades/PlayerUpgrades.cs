using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum UpgradeType { SplitShot, QuickHands, ArcaneWeight, Piercing, FireWisp, PoisonTrail, SecondWind }

// Which upgrades the player has, how many of each, and the resulting missile stats.
public class PlayerUpgrades : MonoBehaviour
{
    public static readonly UpgradeType[] All = (UpgradeType[])System.Enum.GetValues(typeof(UpgradeType));

    readonly int[] stacks = new int[All.Length];
    public readonly List<UpgradeType> TakenInOrder = new List<UpgradeType>();

    public int Stacks(UpgradeType u) => stacks[(int)u];

    // Missile stats after upgrades
    public int MissileCount => 1 + Stacks(UpgradeType.SplitShot);
    public float FireRate => Game.Tuning.fireRate * (1f + Game.Tuning.quickHandsBonus * Stacks(UpgradeType.QuickHands));
    public float Damage => Game.Tuning.missileDamage * (1f + Game.Tuning.arcaneWeightBonus * Stacks(UpgradeType.ArcaneWeight));
    public int Pierce => Game.Tuning.missilePierce + Game.Tuning.piercingPerStack * Stacks(UpgradeType.Piercing);

    public void Apply(UpgradeType u)
    {
        stacks[(int)u]++;
        TakenInOrder.Add(u);
        if (u == UpgradeType.SecondWind)
            Game.PlayerHealth.AddMaxHealthAndHeal(Game.Tuning.secondWindHealth);
        // Fire Wisp and Poison Trail read their stack count every frame, nothing to do here.
    }

    public string Summary()
    {
        var sb = new StringBuilder();
        foreach (var u in All)
            if (Stacks(u) > 0) sb.Append(Name(u)).Append(" x").Append(Stacks(u)).Append('\n');
        return sb.ToString();
    }

    public static string Name(UpgradeType u)
    {
        switch (u)
        {
            case UpgradeType.SplitShot: return "Split Shot";
            case UpgradeType.QuickHands: return "Quick Hands";
            case UpgradeType.ArcaneWeight: return "Arcane Weight";
            case UpgradeType.Piercing: return "Piercing";
            case UpgradeType.FireWisp: return "Fire Wisp";
            case UpgradeType.PoisonTrail: return "Poison Trail";
            default: return "Second Wind";
        }
    }

    public static string Description(UpgradeType u)
    {
        var t = Game.Tuning;
        switch (u)
        {
            case UpgradeType.SplitShot: return "+1 missile per shot, fanned out";
            case UpgradeType.QuickHands: return $"+{t.quickHandsBonus * 100f:0}% fire rate";
            case UpgradeType.ArcaneWeight: return $"+{t.arcaneWeightBonus * 100f:0}% missile damage";
            case UpgradeType.Piercing: return $"Missiles pass through +{t.piercingPerStack} enemy";
            case UpgradeType.FireWisp: return "An orbiting fireball burns goblins it touches";
            case UpgradeType.PoisonTrail: return "Leave a damaging cloud behind while moving";
            default: return $"+{t.secondWindHealth:0} max health and heal to full";
        }
    }
}
