using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Goblin intern brain: chase, wind up (telegraph), strike, recover.
// A vortex can hold it: pulled to the centre, unable to move or attack (and immune to knockback, once there is any).
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinEnemy : MonoBehaviour
{
    public static readonly List<GoblinEnemy> Active = new List<GoblinEnemy>();
    // (where it died). Used to drop loot.
    public static event System.Action<Vector3> Killed;

    enum State { Chase, WindUp, Recover }

    public bool IsDead { get; private set; }
    public bool IsWindingUp => state == State.WindUp;
    // 0 at the start of the wind-up, 1 when the hit lands.
    public bool IsHitFlashing => hitFlashTimer > 0f;
    // Renewed every frame by a vortex; wears off a moment after the vortex ends.
    public bool IsHeld => Time.time < heldUntil;
    public float WindUp01 => IsWindingUp ? 1f - stateTimer / Game.Tuning.goblinWindUpTime : 0f;

    NavMeshAgent agent;
    AudioSource audioSource;
    Transform body;
    Material material;
    Vector3 bodyScale;
    SimplePool<GoblinEnemy> pool;

    State state;
    float stateTimer;
    float repathTimer;
    float hitFlashTimer;
    float health;
    float heldUntil;
    bool wasHeld;
    Vector3 holdCentre;
    float holdPullSpeed, holdRadius;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        body = transform.Find("Body");
        material = body.GetComponent<Renderer>().sharedMaterial;
        bodyScale = body.localScale;
    }

    public void Spawn(SimplePool<GoblinEnemy> owner)
    {
        var t = Game.Tuning;
        pool = owner;
        health = t.goblinHealth * FloorScaling.EnemyHealth;
        IsDead = false;
        state = State.Chase;
        repathTimer = 0f;
        hitFlashTimer = 0f;
        heldUntil = 0f;
        agent.Warp(transform.position);
        ResetLook();
        Active.Add(this);
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        agent.speed = t.goblinMoveSpeed;
        agent.acceleration = t.goblinAcceleration;
        agent.stoppingDistance = t.goblinAttackRange * 0.8f;

        Vector3 toPlayer = Game.Player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (IsHeld)
        {
            wasHeld = true;
            UpdateHeld(dt);
            UpdateHitFlash(dt);
            return;
        }
        if (wasHeld) { wasHeld = false; ResetLook(); } // drop the magenta tint

        switch (state)
        {
            case State.Chase:
                agent.isStopped = false;
                repathTimer -= dt;
                if (repathTimer <= 0f)
                {
                    agent.SetDestination(Game.Player.position);
                    repathTimer = t.goblinRepathInterval;
                }
                if (distance <= t.goblinAttackRange) StartWindUp();
                break;

            case State.WindUp:
                agent.isStopped = true;
                Face(toPlayer);
                stateTimer -= dt;
                ShowWindUp(1f - stateTimer / t.goblinWindUpTime);
                if (stateTimer <= 0f)
                {
                    if (distance <= t.goblinHitRange)
                        Game.PlayerHealth.TakeDamage(t.goblinDamage * FloorScaling.EnemyDamage, transform.position);
                    ResetLook();
                    state = State.Recover;
                    stateTimer = t.goblinAttackCooldown;
                }
                break;

            case State.Recover:
                agent.isStopped = true;
                stateTimer -= dt;
                if (stateTimer <= 0f) state = State.Chase;
                break;
        }

        UpdateHitFlash(dt);
    }

    void UpdateHitFlash(float dt)
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= dt;
            material.SetColor("_BaseColor", hitFlashTimer > 0f ? Palette.EnemyHitFlash : CurrentColor());
        }
    }

    // Called every frame by a vortex while this goblin is inside it.
    public void Hold(Vector3 centre, float pullSpeed, float stopRadius)
    {
        if (IsDead) return;
        heldUntil = Time.time + 0.1f;
        holdCentre = centre;
        holdPullSpeed = pullSpeed;
        holdRadius = stopRadius;
    }

    // Held: any attack is cancelled, and the goblin slides toward the vortex centre.
    void UpdateHeld(float dt)
    {
        if (state == State.WindUp) ResetLook();
        state = State.Chase; // resume chasing once released
        repathTimer = 0f;
        agent.isStopped = true;

        Vector3 toCentre = holdCentre - transform.position;
        toCentre.y = 0f;
        float gap = toCentre.magnitude - holdRadius;
        if (gap > 0f) agent.Move(toCentre.normalized * Mathf.Min(gap, holdPullSpeed * dt));
        if (hitFlashTimer <= 0f) material.SetColor("_BaseColor", Palette.HeldTint);
        Mats.SetEmission(material, Palette.Arcane * Game.Tuning.heldGlow);
    }

    void StartWindUp()
    {
        var t = Game.Tuning;
        state = State.WindUp;
        stateTimer = t.goblinWindUpTime;
        if (t.behindCueEnabled && !IsOnScreen()) BehindYouCue.TryPlay(audioSource, transform.position);
    }

    // k goes 0 -> 1 over the wind-up: swell up and glow red.
    void ShowWindUp(float k)
    {
        var t = Game.Tuning;
        body.localScale = bodyScale * (1f + t.goblinWindUpSwell * k);
        if (hitFlashTimer <= 0f) material.SetColor("_BaseColor", CurrentColor());
        Mats.SetEmission(material, BaseGlow() + Palette.GoblinWindUp * (t.goblinWindUpGlow * k));
    }

    Color CurrentColor()
    {
        if (IsHeld) return Palette.HeldTint;
        if (state != State.WindUp) return Palette.Goblin;
        float k = 1f - stateTimer / Game.Tuning.goblinWindUpTime;
        return Color.Lerp(Palette.Goblin, Palette.GoblinWindUp, k);
    }

    void ResetLook()
    {
        body.localScale = bodyScale;
        material.SetColor("_BaseColor", CurrentColor());
        Mats.SetEmission(material, BaseGlow());
    }

    static Color BaseGlow() => Palette.Goblin * Game.Tuning.goblinBaseGlow;

    void Face(Vector3 flatDirection)
    {
        if (flatDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatDirection);
    }

    public bool IsOnScreen()
    {
        Vector3 vp = Game.Camera.WorldToViewportPoint(transform.position + Vector3.up * (Game.Tuning.goblinHeight / 2f));
        return vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        health -= amount;
        hitFlashTimer = Game.Tuning.enemyHitFlashTime;
        material.SetColor("_BaseColor", Palette.EnemyHitFlash);
        if (health <= 0f) Die();
    }

    // Gone without counting as a kill (used when leaving a floor).
    public void Despawn()
    {
        if (IsDead) return;
        IsDead = true;
        Active.Remove(this);
        pool.Release(this);
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Active.Remove(this);
        RunStats.Kills++;
        Killed?.Invoke(transform.position);
        pool.Release(this);
    }
}
