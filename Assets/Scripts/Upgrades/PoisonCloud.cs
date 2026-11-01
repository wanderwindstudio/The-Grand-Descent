using UnityEngine;

// One puddle of poison. Damages goblins standing in it, then shrinks away.
public class PoisonCloud : MonoBehaviour
{
    static readonly Collider[] overlaps = new Collider[32];

    SimplePool<PoisonCloud> pool;
    int stacks;
    float lifeLeft;
    float tickTimer;

    public void Begin(int stackCount, SimplePool<PoisonCloud> owner)
    {
        stacks = stackCount;
        pool = owner;
        lifeLeft = Game.Tuning.poisonCloudDuration;
        tickTimer = 0f;
    }

    void Update()
    {
        var t = Game.Tuning;
        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f) { pool.Release(this); return; }

        float shrink = Mathf.Clamp01(lifeLeft / t.poisonCloudDuration);
        float diameter = t.poisonCloudRadius * 2f * Mathf.Lerp(0.4f, 1f, shrink);
        transform.localScale = new Vector3(diameter, 0.02f, diameter);

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;
        tickTimer = t.poisonTickInterval;

        float damage = t.poisonDamagePerSecond * stacks * t.poisonTickInterval;
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.5f, t.poisonCloudRadius, overlaps, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var goblin = overlaps[i].GetComponentInParent<GoblinEnemy>();
            if (goblin != null && !goblin.IsDead) goblin.TakeDamage(damage);
        }
    }
}
