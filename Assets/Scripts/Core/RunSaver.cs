using System;
using UnityEngine;

// Copies the run in progress into the save slot, and back into the player after loading.
public static class RunSaver
{
    public static RunSave Capture(int seed, float runSeconds)
    {
        var player = Game.Player;
        var level = player.GetComponent<PlayerLevel>();
        var run = new RunSave
        {
            floor = Game.Floor,
            seed = seed,
            level = level.Level,
            xp = level.Xp,
            pendingLevelUps = level.PendingLevelUps,
            health = Game.PlayerHealth.Current,
            maxHealth = Game.PlayerHealth.Max,
            carriedGold = RunManager.Instance.CarriedGold,
            buzz = Game.Buzz.Value,
            kills = RunStats.Kills,
            runSeconds = runSeconds,
        };
        foreach (var u in player.GetComponent<PlayerUpgrades>().TakenInOrder) run.upgrades.Add(u.ToString());
        // Every spell the player can cast right now (Arcane Explosion isn't learnable yet).
        run.spells.Add(Spell.ArcaneMissile.ToString());
        run.spells.Add(Spell.ArcaneVortex.ToString());
        return run;
    }

    public static void Restore(RunSave run)
    {
        var player = Game.Player;
        var upgrades = player.GetComponent<PlayerUpgrades>();
        foreach (var name in run.upgrades)
            if (Enum.TryParse(name, out UpgradeType u)) upgrades.Apply(u);
            else Debug.LogWarning("Unknown upgrade in save: " + name);
        player.GetComponent<PlayerLevel>().Restore(run.level, run.xp, run.pendingLevelUps);
        if (run.maxHealth > 0f) Game.PlayerHealth.Restore(run.health, run.maxHealth);
        RunManager.Instance.AddGold(run.carriedGold);
        Game.Buzz.SetValue(run.buzz);
        RunStats.Kills = run.kills;
    }
}
