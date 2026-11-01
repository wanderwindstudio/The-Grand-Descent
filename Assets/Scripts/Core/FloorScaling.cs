using UnityEngine;

// How much tougher (and richer) the current floor is than floor 1.
public static class FloorScaling
{
    static float Grow(float perFloor) => Mathf.Pow(perFloor, Game.Floor - 1);

    public static float EnemyHealth => Grow(Game.Tuning.floorEnemyHealthMultiplier);
    public static float EnemyCount => Grow(Game.Tuning.floorEnemyCountMultiplier);
    public static float EnemyDamage => Grow(Game.Tuning.floorEnemyDamageMultiplier);
    public static float Gold => Grow(Game.Tuning.floorGoldMultiplier);
}
