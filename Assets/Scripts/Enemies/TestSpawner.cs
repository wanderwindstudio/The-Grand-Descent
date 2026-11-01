using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Milestone 1 only: keeps a handful of goblins alive. Replaced by real waves in milestone 2.
public class TestSpawner : MonoBehaviour
{
    List<Vector3> spawnPoints;
    SimplePool<GoblinEnemy> pool;
    float respawnTimer = -1f;

    public void Init(List<Vector3> points)
    {
        spawnPoints = points;
        pool = new SimplePool<GoblinEnemy>(GoblinBuilder.Create);
        SpawnBatch();
    }

    void Update()
    {
        if (GoblinEnemy.Active.Count > 0) return;

        if (respawnTimer < 0f)
        {
            respawnTimer = Game.Tuning.testRespawnDelay;
            Hud.ShowMessage("All clear - more goblins incoming");
        }
        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0f)
        {
            respawnTimer = -1f;
            SpawnBatch();
        }
    }

    void SpawnBatch()
    {
        var t = Game.Tuning;
        for (int i = 0; i < t.testGoblinCount; i++)
        {
            Vector3 point = spawnPoints[i % spawnPoints.Count];
            Vector2 scatter = Random.insideUnitCircle * t.spawnScatter;
            point += new Vector3(scatter.x, 0f, scatter.y);
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, t.spawnScatter * 2f, NavMesh.AllAreas))
                point = hit.position;
            pool.Get(point).Spawn(pool);
        }
    }
}
