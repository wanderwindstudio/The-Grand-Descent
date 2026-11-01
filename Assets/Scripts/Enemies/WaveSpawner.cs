using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Runs the waves of a floor: countdown, trickle goblins in out of sight, wait until all are dead.
public class WaveSpawner : MonoBehaviour
{
    enum State { Countdown, Spawning, Fighting, FloorCleared }

    public int WaveNumber { get; private set; }    // 1-based; 0 before the first wave
    public int WaveCount => Game.Tuning.waveEnemyCounts.Length;
    public int EnemiesRemaining => GoblinEnemy.Active.Count + toSpawn;

    List<Vector3> spawnPoints;
    SimplePool<GoblinEnemy> pool;
    State state;
    float timer;
    int toSpawn;

    public void Init(List<Vector3> points)
    {
        spawnPoints = points;
        pool = new SimplePool<GoblinEnemy>(GoblinBuilder.Create);
        StartCountdown(Game.Tuning.firstWaveDelay);
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
                    state = State.FloorCleared;
                    timer = t.floorClearedRestartDelay;
                    Hud.ShowMessage("FLOOR CLEARED");
                }
                else
                {
                    Hud.ShowMessage($"Wave {WaveNumber} cleared");
                    StartCountdown(t.timeBetweenWaves);
                }
                break;

            case State.FloorCleared:
                // Milestone 2 only: replay the waves. Portals replace this in milestone 4.
                if (timer <= 0f)
                {
                    WaveNumber = 0;
                    StartCountdown(0f);
                }
                break;
        }
    }

    void StartCountdown(float seconds)
    {
        state = State.Countdown;
        timer = seconds;
    }

    void StartWave()
    {
        WaveNumber++;
        toSpawn = Game.Tuning.waveEnemyCounts[WaveNumber - 1];
        state = State.Spawning;
        timer = 0f;
        Hud.ShowMessage($"Wave {WaveNumber}");
    }

    void SpawnOne()
    {
        var t = Game.Tuning;
        Vector3 point = PickHiddenSpawnPoint();
        Vector2 scatter = Random.insideUnitCircle * t.spawnScatter;
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
        return hidden.Count > 0 ? hidden[Random.Range(0, hidden.Count)] : farthest;
    }

    static bool IsOnScreen(Vector3 point)
    {
        Vector3 vp = Game.Camera.WorldToViewportPoint(point + Vector3.up);
        return vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
    }
}
