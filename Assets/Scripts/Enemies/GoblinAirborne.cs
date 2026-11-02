using UnityEngine;
using UnityEngine.AI;

// A goblin off the NavMesh: knocked flying (barrels, later Arcane Explosion) or lifted over a pit by a vortex.
// Flies under gravity until it lands. Falling far hurts; landing in a pit after starting outside it is a RING OUT.
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinAirborne : MonoBehaviour
{
    static readonly RaycastHit[] hits = new RaycastHit[8];
    const int SolidMask = ~(1 << 2);
    const float Radius = 0.3f;

    public bool Active { get; private set; }
    public bool HeldOverPit => Active && Time.time < heldUntil;

    NavMeshAgent agent;
    GoblinEnemy goblin;
    GoblinTraversal traversal;
    Vector3 velocity;
    float peakY, heldUntil, holdPull;
    Vector3 holdCentre;
    bool startedInPit;
    GoblinEnemy.Cause cause;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        goblin = GetComponent<GoblinEnemy>();
        traversal = GetComponent<GoblinTraversal>();
    }

    public void ResetState()
    {
        Active = false;
        heldUntil = 0f;
    }

    public void Launch(Vector3 v, GoblinEnemy.Cause source)
    {
        Begin(source);
        velocity = v;
    }

    // Called every frame by a vortex hanging over a pit: pulled up and in, off the floor.
    public void HoldOverPit(Vector3 centre, float pullSpeed)
    {
        Begin(GoblinEnemy.Cause.RingOut);
        startedInPit = false; // lifted out by the vortex: dropping back in counts
        heldUntil = Time.time + 0.1f;
        holdCentre = centre;
        holdPull = pullSpeed;
        velocity = Vector3.zero;
    }

    void Begin(GoblinEnemy.Cause source)
    {
        cause = source;
        if (Active) return;
        traversal.Cancel();
        Active = true;
        agent.enabled = false;
        peakY = transform.position.y;
        startedInPit = RoomPit.At(transform.position) != null;
    }

    // Returns true while airborne (the goblin's brain sits out).
    public bool Tick(float dt)
    {
        if (!Active) return false;
        var t = Game.Tuning;

        if (HeldOverPit)
        {
            Vector3 target = holdCentre + Vector3.down * 0.9f;
            transform.position = Vector3.MoveTowards(transform.position, target, holdPull * dt);
            peakY = Mathf.Max(peakY, transform.position.y);
            return true;
        }

        velocity.y = Mathf.Max(velocity.y + t.goblinGravity * dt, -40f);
        Vector3 step = velocity * dt;
        float dist = step.magnitude;
        Vector3 origin = transform.position + Vector3.up * (Radius + 0.05f);

        if (dist > 0.0001f && FirstSolidHit(origin, step / dist, dist, out RaycastHit hit))
        {
            if (hit.normal.y > 0.5f && velocity.y <= 0f)
            {
                Land(origin + step / dist * hit.distance + Vector3.down * (Radius + 0.05f));
                return true;
            }
            // Hit a wall or ceiling: stop there and fall straight down.
            transform.position = origin + step / dist * Mathf.Max(0f, hit.distance - 0.02f) - Vector3.up * (Radius + 0.05f);
            velocity = new Vector3(0f, Mathf.Min(0f, velocity.y), 0f);
            return true;
        }

        transform.position += step;
        peakY = Mathf.Max(peakY, transform.position.y);
        if (transform.position.y < peakY - 60f) { Active = false; goblin.RingOut(); } // fell out of the world
        return true;
    }

    bool FirstSolidHit(Vector3 origin, Vector3 dir, float dist, out RaycastHit best)
    {
        best = default;
        int n = Physics.SphereCastNonAlloc(origin, Radius, dir, hits, dist, SolidMask, QueryTriggerInteraction.Ignore);
        float bestDist = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            var c = hits[i].collider;
            if (hits[i].distance <= 0f) continue; // already touching at the start
            if (c.GetComponentInParent<GoblinEnemy>() != null || c.GetComponentInParent<PlayerHealth>() != null) continue;
            if (hits[i].distance < bestDist) { bestDist = hits[i].distance; best = hits[i]; }
        }
        return bestDist < float.MaxValue;
    }

    void Land(Vector3 point)
    {
        var t = Game.Tuning;
        Active = false;
        transform.position = point;
        float fall = peakY - point.y;

        if (!startedInPit && RoomPit.At(point) != null) { goblin.RingOut(); return; }
        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)) { goblin.RingOut(); return; } // somewhere it can never leave

        agent.enabled = true;
        agent.Warp(hit.position);
        float damage = Mathf.Max(0f, fall - t.goblinFallDamageHeight) * t.goblinFallDamagePerMetre;
        if (damage > 0f) goblin.TakeDamage(damage, cause == GoblinEnemy.Cause.RingOut ? GoblinEnemy.Cause.Spell : cause);
        if (!goblin.IsDead) goblin.Stun(t.goblinLandingStun);
    }
}
