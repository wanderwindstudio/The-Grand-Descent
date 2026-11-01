using System.Collections.Generic;
using UnityEngine;

// A flying magic missile. Sweeps a sphere ahead each frame to find what it hits.
public class Missile : MonoBehaviour
{
    static readonly RaycastHit[] hits = new RaycastHit[16];
    static readonly HitDistanceComparer byDistance = new HitDistanceComparer();

    SimplePool<Missile> pool;
    Vector3 direction;
    float damage;
    float lifeLeft;
    int hitsLeft;
    readonly HashSet<GoblinEnemy> alreadyHit = new HashSet<GoblinEnemy>();

    public void SetPool(SimplePool<Missile> owner) => pool = owner;

    public void Launch(Vector3 dir, float dmg, int pierce)
    {
        direction = dir.normalized;
        damage = dmg;
        hitsLeft = 1 + pierce;
        lifeLeft = Game.Tuning.missileLifetime;
        alreadyHit.Clear();
    }

    void Update()
    {
        var t = Game.Tuning;
        float step = t.missileSpeed * Time.deltaTime;
        Vector3 pos = transform.position;

        int count = Physics.SphereCastNonAlloc(pos, t.missileRadius, direction, hits, step, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, 0, count, byDistance);

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i].collider;
            if (col.GetComponentInParent<PlayerHealth>() != null) continue;

            var goblin = col.GetComponentInParent<GoblinEnemy>();
            if (goblin == null) { Despawn(); return; } // hit a wall or floor

            if (goblin.IsDead || alreadyHit.Contains(goblin)) continue;
            alreadyHit.Add(goblin);
            goblin.TakeDamage(damage);
            hitsLeft--;
            if (hitsLeft <= 0) { Despawn(); return; }
        }

        transform.position = pos + direction * step;
        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f) Despawn();
    }

    void Despawn() => pool.Release(this);

    class HitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
