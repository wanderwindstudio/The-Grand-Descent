using System.Collections.Generic;
using UnityEngine;

// A flying magic missile. Sweeps a sphere ahead each frame to find what it hits.
// At high Buzz it drifts off its line in a sine wave (wobble).
public class Missile : MonoBehaviour
{
    static readonly RaycastHit[] hits = new RaycastHit[16];
    static readonly HitDistanceComparer byDistance = new HitDistanceComparer();

    SimplePool<Missile> pool;
    Renderer rend;
    Light glow;
    readonly HashSet<GoblinEnemy> alreadyHit = new HashSet<GoblinEnemy>();

    Vector3 direction, side, lift;
    Vector3 straightPosition; // where it would be without wobble
    float speed, damage, hitRadius, wobble, phase, age, lifeLeft;
    int hitsLeft;

    public void SetPool(SimplePool<Missile> owner) => pool = owner;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        glow = GetComponentInChildren<Light>();
    }

    public void Launch(Vector3 dir, float dmg, int pierce, float missileSpeed, float radius, float size, float wobbleAmount, Color color)
    {
        var t = Game.Tuning;
        direction = dir.normalized;
        side = Vector3.Cross(Vector3.up, direction).normalized;
        if (side.sqrMagnitude < 0.01f) side = Vector3.right;
        lift = Vector3.Cross(direction, side);

        damage = dmg;
        hitsLeft = 1 + pierce;
        speed = missileSpeed;
        hitRadius = radius;
        wobble = wobbleAmount;
        phase = Random.Range(0f, Mathf.PI * 2f);
        age = 0f;
        lifeLeft = t.missileLifetime / Mathf.Max(0.1f, missileSpeed / t.missileSpeed); // slow orbs fly as far as normal ones
        straightPosition = transform.position;
        alreadyHit.Clear();

        transform.localScale = Vector3.one * size;
        rend.sharedMaterial = Mats.Glow(color, t.missileGlowIntensity);
        glow.color = color;
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        age += dt;
        straightPosition += direction * speed * dt;

        float ramp = Mathf.Clamp01(age / t.wobbleRampTime);
        float w = 2f * Mathf.PI * t.wobbleFrequency * age + phase;
        Vector3 offset = (side * Mathf.Sin(w) + lift * (0.5f * Mathf.Cos(w * 0.7f))) * (wobble * ramp);
        Vector3 from = transform.position;
        Vector3 to = straightPosition + offset;
        Vector3 move = to - from;
        float dist = move.magnitude;

        if (dist > 0.0001f && SweepHits(from, move / dist, dist)) return;

        transform.position = to;
        lifeLeft -= dt;
        if (lifeLeft <= 0f) Despawn();
    }

    // Returns true if the missile was used up.
    bool SweepHits(Vector3 from, Vector3 dir, float dist)
    {
        int count = Physics.SphereCastNonAlloc(from, hitRadius, dir, hits, dist, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, 0, count, byDistance);

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i].collider;
            if (col.GetComponentInParent<PlayerHealth>() != null) continue;

            var goblin = col.GetComponentInParent<GoblinEnemy>();
            if (goblin == null) { Despawn(); return true; } // hit a wall or floor

            if (goblin.IsDead || alreadyHit.Contains(goblin)) continue;
            alreadyHit.Add(goblin);
            goblin.TakeDamage(damage);
            hitsLeft--;
            if (hitsLeft <= 0) { Despawn(); return true; }
        }
        return false;
    }

    void Despawn() => pool.Release(this);

    class HitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
