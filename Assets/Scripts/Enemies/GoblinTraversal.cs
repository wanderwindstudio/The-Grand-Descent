using UnityEngine;
using UnityEngine.AI;

// Crosses goblin NavMeshLinks by hand: hop a low wall (short crouch first), drop off a ledge
// (short stun on landing), or climb a ladder slowly. No attacking while any of this is going on.
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinTraversal : MonoBehaviour
{
    public bool Active { get; private set; }
    public bool IsCrouching => Active && kind == TraversalLink.Kind.Hop && elapsed < Game.Tuning.goblinHopDelay;

    NavMeshAgent agent;
    GoblinEnemy goblin;
    TraversalLink.Kind kind;
    Vector3 from, to;
    Vector3[] path;     // ladder: foot -> up the ladder -> top
    float elapsed, duration, launchSpeed;
    int pathIndex;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        goblin = GetComponent<GoblinEnemy>();
        agent.autoTraverseOffMeshLink = false;
    }

    public void Cancel() => Active = false;

    // Returns true while crossing a link (the goblin's brain sits out).
    public bool Tick(float dt)
    {
        if (!Active)
        {
            if (!agent.enabled || !agent.isOnOffMeshLink) return false;
            Begin();
        }

        var t = Game.Tuning;
        elapsed += dt;
        bool done;
        switch (kind)
        {
            case TraversalLink.Kind.Hop: done = Hop(t); break;
            case TraversalLink.Kind.Ladder: done = Climb(t, dt); break;
            default: done = Drop(); break;
        }
        if (done) Finish(t);
        return true;
    }

    void Begin()
    {
        var t = Game.Tuning;
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        from = transform.position;
        to = data.endPos;
        var info = (data.owner as Component)?.GetComponent<TraversalLink>();
        kind = info != null ? info.kind : (to.y < from.y - 1f ? TraversalLink.Kind.Drop : TraversalLink.Kind.Hop);
        elapsed = 0f;
        Active = true;

        if (kind == TraversalLink.Kind.Drop)
        {
            // A small hop out, then a fall under gravity: pick the time so it lands on the far end.
            float drop = Mathf.Max(0.1f, from.y - to.y);
            duration = Mathf.Sqrt(2f * (drop + 0.5f) / -t.goblinGravity);
            launchSpeed = (to.y - from.y - 0.5f * t.goblinGravity * duration * duration) / duration;
        }
        else if (kind == TraversalLink.Kind.Ladder && info != null && info.ladder != null)
        {
            Vector3 column = info.ladder.Column(from);
            path = new[] { new Vector3(column.x, from.y, column.z), new Vector3(column.x, to.y, column.z), to };
            pathIndex = 0;
        }
        else if (kind == TraversalLink.Kind.Ladder)
        {
            path = new[] { new Vector3(from.x, to.y, from.z), to };
            pathIndex = 0;
        }
    }

    bool Hop(TuningConfig t)
    {
        if (elapsed < t.goblinHopDelay) { Face(to - from); return false; } // crouch, then go
        float k = Mathf.Clamp01((elapsed - t.goblinHopDelay) / t.goblinHopTime);
        Vector3 p = Vector3.Lerp(from, to, k);
        p.y += 4f * k * (1f - k) * 1.3f; // arc that clears a 1 m wall
        transform.position = p;
        return k >= 1f;
    }

    bool Drop()
    {
        var t = Game.Tuning;
        float time = Mathf.Min(elapsed, duration);
        float k = time / duration;
        Vector3 p = Vector3.Lerp(from, to, k);
        p.y = from.y + launchSpeed * time + 0.5f * t.goblinGravity * time * time;
        transform.position = p;
        Face(to - from);
        return elapsed >= duration;
    }

    bool Climb(TuningConfig t, float dt)
    {
        float speed = t.goblinMoveSpeed * t.goblinLadderSpeedFraction;
        float step = speed * dt;
        // A few corners per frame at most; a tiny leftover step is dropped (it can't move a float position).
        for (int corner = 0; corner < 4 && step > 0.0001f && pathIndex < path.Length; corner++)
        {
            Vector3 target = path[pathIndex];
            Vector3 p = Vector3.MoveTowards(transform.position, target, step);
            step -= Vector3.Distance(transform.position, p);
            transform.position = p;
            if ((p - target).sqrMagnitude < 0.0001f) pathIndex++;
        }
        return pathIndex >= path.Length;
    }

    void Finish(TuningConfig t)
    {
        transform.position = to;
        agent.CompleteOffMeshLink();
        Active = false;
        if (kind == TraversalLink.Kind.Drop) goblin.Stun(t.goblinLandingStun);
    }

    void Face(Vector3 d)
    {
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
    }
}
