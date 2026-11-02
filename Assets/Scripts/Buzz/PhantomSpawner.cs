using UnityEngine;

// Hangover mode, high Buzz: hallucinated goblins appear from the normal spawn points during a wave.
// The higher the Buzz above the threshold, the faster they come. They all vanish when Buzz drops
// below the threshold or the wave is cleared.
public class PhantomSpawner : MonoBehaviour
{
    WaveSpawner waves;
    float timer;

    void Awake() => waves = GetComponent<WaveSpawner>();

    void Update()
    {
        var t = Game.Tuning;
        if (Game.Player == null || Game.PlayerHealth.IsDead) return;
        bool active = BuzzMeter.Hangover && Game.Buzz.Current != BuzzMeter.Phase.Crash && Game.Buzz.Normalized >= t.phantomBuzzThreshold;
        if (!active)
        {
            if (GoblinEnemy.PhantomCount > 0) DispelAll();
            timer = 0f;
            return;
        }
        if (!waves.WaveInProgress || RunManager.Instance.PortalsOpen) return;

        // 0 at the threshold, 1 at full Buzz.
        float k = Mathf.InverseLerp(t.phantomBuzzThreshold, 1f, Game.Buzz.Normalized);
        float interval = Mathf.Lerp(t.phantomIntervalAtThreshold, t.phantomIntervalAtMax, k);
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;
        if (GoblinEnemy.PhantomCount < t.phantomMaxAlive) waves.SpawnPhantom();
    }

    // Every phantom vanishes at once (not counted as killed).
    public static void DispelAll()
    {
        foreach (var g in GoblinEnemy.Active.ToArray())
            if (g.IsPhantom) g.Vanish(killed: false);
    }
}
