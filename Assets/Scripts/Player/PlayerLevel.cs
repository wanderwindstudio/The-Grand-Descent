using UnityEngine;

// XP and level. Each level gained adds a pending level-up for the level-up screen.
public class PlayerLevel : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public float Xp { get; private set; }
    public int PendingLevelUps { get; private set; }

    public float XpToNext => Game.Tuning.xpFirstLevel + Game.Tuning.xpPerLevelIncrease * (Level - 1);

    public void AddXp(float amount)
    {
        Xp += amount;
        while (Xp >= XpToNext)
        {
            Xp -= XpToNext;
            GainLevel();
        }
    }

    public void GainLevel()
    {
        Level++;
        PendingLevelUps++;
    }

    public void ConsumeLevelUp() => PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);
}
