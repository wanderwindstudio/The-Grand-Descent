using UnityEngine;
using UnityEngine.AI;

// Punishes camping where goblins can't get to you: stand still for a while somewhere no goblin can reach
// (no NavMesh path from a goblin to you), and 1-2 ranged "sprayers" are sent in.
// Also warns in the Console about such spots, so room builders can fix them.
public class CampingWatcher : MonoBehaviour
{
    WaveSpawner waves;
    Vector3 anchor;
    float campTimer, checkTimer, cooldownLeft;
    bool unreachable;
    Vector3 lastWarned = Vector3.one * 9999f;
    NavMeshPath path;

    void Awake()
    {
        waves = GetComponent<WaveSpawner>();
        path = new NavMeshPath();
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        cooldownLeft -= dt;
        if (RunManager.Instance.Current != RunManager.State.Playing || GoblinEnemy.Active.Count == 0 || dt <= 0f)
        {
            campTimer = 0f;
            return;
        }

        Vector3 p = Game.Player.position;
        if (Vector3.Distance(p, anchor) > t.campRadius)
        {
            anchor = p;
            campTimer = 0f;
        }

        checkTimer -= dt;
        if (checkTimer <= 0f)
        {
            checkTimer = 0.5f;
            unreachable = !GoblinsCanReach(p);
            if (unreachable && Vector3.Distance(p, lastWarned) > 4f && Game.Player.GetComponent<PlayerMotor>().IsGrounded)
            {
                lastWarned = p;
                Debug.LogWarning($"Player is standing where goblins can't reach: {p}. If this is in a room, add a link (ledge, ladder, low wall) or block it off.");
            }
        }

        campTimer = unreachable ? campTimer + dt : 0f;
        if (campTimer >= t.campTime && cooldownLeft <= 0f)
        {
            campTimer = 0f;
            cooldownLeft = t.sprayerCooldown;
            int count = Random.Range(t.sprayersMin, t.sprayersMax + 1);
            waves.SpawnSprayers(count);
            Hud.ShowMessage(count > 1 ? "Sprayers incoming!" : "A sprayer is coming for you!");
        }
    }

    // True if any goblin on the NavMesh has a full path to (near) this spot.
    bool GoblinsCanReach(Vector3 spot)
    {
        if (!NavMesh.SamplePosition(spot, out NavMeshHit target, 1.2f, NavMesh.AllAreas)) return false;
        int tried = 0;
        foreach (var g in GoblinEnemy.Active)
        {
            if (g.IsBusyMoving || g.IsRanged) continue;
            if (!NavMesh.SamplePosition(g.transform.position, out NavMeshHit from, 1f, NavMesh.AllAreas)) continue;
            if (NavMesh.CalculatePath(from.position, target.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                return true;
            if (++tried >= 3) break; // a few goblins is enough
        }
        return tried == 0; // nobody to ask: assume reachable
    }
}
