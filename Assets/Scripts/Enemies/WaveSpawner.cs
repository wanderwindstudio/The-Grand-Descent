using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Runs the waves of a floor: countdown, trickle goblins in out of sight, wait until all are dead.
public class WaveSpawner : MonoBehaviour
{
    enum State { Idle, Countdown, Spawning, Fighting }

    public event Action FloorCleared;

    public int WaveNumber { get; private set; }    // 1-based; 0 before the first wave
    public int WaveCount => Game.Tuning.waveEnemyCounts.Length;
    public int EnemiesRemaining => GoblinEnemy.Active.Count + toSpawn;

    List<Vector3> spawnPoints;
    SimplePool<GoblinEnemy> pool;
    State state = State.Idle;
    float timer;
    int toSpawn;

    void Awake() => pool = new SimplePool<GoblinEnemy>(GoblinBuilder.Create);

    public void BeginFloor(List<Vector3> points)
    {
        spawnPoints = points;
        WaveNumber = 0;
        toSpawn = 0;
        state = State.Countdown;
        timer = Game.Tuning.firstWaveDelay;
    }

    // Removes every goblin without counting kills or dropping loot (used when leaving a floor).
    public void ClearEnemies()
    {
        foreach (var g in GoblinEnemy.Active.ToArray()) g.Despawn();
        toSpawn = 0;
        state = State.Idle;
    }

    void Update()
    {
        var t = Game.Tuning;
        timer -= Time.deltaTime;

        switch (state)
        {
            case State.Countdown:
                if (timer <= 0f) StartWave();
                break;

            case State.Spawning:
                if (timer <= 0f)
                {
                    SpawnOne();
                    toSpawn--;
                    timer = t.spawnInterval;
                    if (toSpawn <= 0) state = State.Fighting;
                }
                break;

            case State.Fighting:
                if (GoblinEnemy.Active.Count > 0) break;
                if (WaveNumber >= WaveCount)
                {
                    state = State.Idle;
                    FloorCleared?.Invoke();
                }
                else
                {
                    Hud.ShowMessage($"Wave {WaveNumber} cleared");
                    state = State.Countdown;
                    timer = t.timeBetweenWaves;
                }
                break;
        }
    }

    void StartWave()
    {
        WaveNumber++;
        int baseCount = Game.Tuning.waveEnemyCounts[WaveNumber - 1];
        toSpawn = Mathf.Max(1, Mathf.RoundToInt(baseCount * FloorScaling.EnemyCount));
        state = State.Spawning;
        timer = 0f;
        Hud.ShowMessage($"Wave {WaveNumber}");
    }

    void SpawnOne()
    {
        var t = Game.Tuning;
        Vector3 point = PickHiddenSpawnPoint();
        Vector2 scatter = UnityEngine.Random.insideUnitCircle * t.spawnScatter;
        point += new Vector3(scatter.x, 0f, scatter.y);
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, t.spawnScatter * 2f, NavMesh.AllAreas))
            point = hit.position;
        pool.Get(point).Spawn(pool);
    }

    // A random spawn point that is off-screen and not too close. If none qualifies, the farthest one.
    Vector3 PickHiddenSpawnPoint()
    {
        var hidden = new List<Vector3>();
        Vector3 farthest = spawnPoints[0];
        float farthestDist = 0f;
        foreach (var p in spawnPoints)
        {
            float dist = Vector3.Distance(p, Game.Player.position);
            if (dist > farthestDist) { farthestDist = dist; farthest = p; }
            if (dist >= Game.Tuning.minSpawnDistance && !IsOnScreen(p)) hidden.Add(p);
        }
        return hidden.Count > 0 ? hidden[UnityEngine.Random.Range(0, hidden.Count)] : farthest;
    }

    static bool IsOnScreen(Vector3 point)
    {
        Vector3 vp = Game.Camera.WorldToViewportPoint(point + Vector3.up);
        return vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
    }
}
