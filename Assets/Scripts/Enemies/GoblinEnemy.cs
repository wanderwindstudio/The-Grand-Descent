using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Goblin intern brain: chase, wind up (telegraph), strike, recover.
// A vortex can hold it: pulled to the centre, unable to move or attack, and immune to knockback.
// Traversal (wall hops, ledge drops, ladders) and flying (knockback, pit drops) live in GoblinTraversal and GoblinAirborne.
// A "sprayer" is the ranged variant: it keeps its distance and lobs slow projectiles.
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinEnemy : MonoBehaviour
{
    public static readonly List<GoblinEnemy> Active = new List<GoblinEnemy>();
    // (where it died). Used to drop loot.
    public static event System.Action<Vector3> Killed;

    // What dealt the damage, for the run log.
    public enum Cause { Spell, Barrel, RingOut }

    enum State { Chase, WindUp, Recover }

    public bool IsDead { get; private set; }
    public bool IsRanged { get; private set; }
    public bool IsWindingUp => state == State.WindUp;
    public bool IsHitFlashing => hitFlashTimer > 0f;
    // Renewed every frame by a vortex; wears off a moment after the vortex ends.
    public bool IsHeld => Time.time < heldUntil || airborne.HeldOverPit;
    public bool IsBusyMoving => airborne.Active || traversal.Active;
    public bool IsCrouching => traversal.IsCrouching;
    // 0 at the start of the wind-up, 1 when the hit lands.
    public float WindUp01 => IsWindingUp ? 1f - stateTimer / WindUpTime : 0f;
    public Color SpriteTint => IsRanged ? Palette.SprayerTint : Color.white;

    float WindUpTime => IsRanged ? Game.Tuning.sprayerWindUpTime : Game.Tuning.goblinWindUpTime;
    Color BodyColor => IsRanged ? Palette.Sprayer : Palette.Goblin;

    NavMeshAgent agent;
    GoblinTraversal traversal;
    GoblinAirborne airborne;
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
    float stunnedUntil;
    bool wasHeld;
    Cause lastCause;
    Vector3 holdCentre;
    float holdPullSpeed, holdRadius;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        traversal = GetComponent<GoblinTraversal>();
        airborne = GetComponent<GoblinAirborne>();
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
        IsRanged = false;
        state = State.Chase;
        repathTimer = 0f;
        hitFlashTimer = 0f;
        heldUntil = 0f;
        stunnedUntil = 0f;
        lastCause = Cause.Spell;
        traversal.Cancel();
        airborne.ResetState();
        agent.enabled = true;
        agent.Warp(transform.position);
        ResetLook();
        Active.Add(this);
    }

    // Turns a freshly spawned goblin into a sprayer.
    public void MakeRanged()
    {
        IsRanged = true;
        health = Game.Tuning.sprayerHealth * FloorScaling.EnemyHealth;
        ResetLook();
    }

    public void Stun(float seconds) => stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;

        if (airborne.Tick(dt) || traversal.Tick(dt))
        {
            if (state == State.WindUp) ResetLook();
            state = State.Chase;
            UpdateHitFlash(dt);
            return;
        }

        agent.speed = t.goblinMoveSpeed;
        agent.acceleration = t.goblinAcceleration;
        agent.stoppingDistance = IsRanged ? t.sprayerRange * 0.7f : t.goblinAttackRange * 0.8f;

        Vector3 toPlayer = Game.Player.position - transform.position;
        float height = Mathf.Abs(toPlayer.y);
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

        if (Time.time < stunnedUntil)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            UpdateHitFlash(dt);
            return;
        }

        switch (state)
        {
            case State.Chase:
                agent.isStopped = false;
                repathTimer -= dt;
                if (repathTimer <= 0f && agent.isOnNavMesh)
                {
                    agent.SetDestination(Game.Player.position);
                    repathTimer = t.goblinRepathInterval;
                }
                if (IsRanged ? distance <= t.sprayerRange && CanSeePlayer()
                             : distance <= t.goblinAttackRange && height <= t.goblinAttackVerticalReach)
                    StartWindUp();
                break;

            case State.WindUp:
                agent.isStopped = true;
                Face(toPlayer);
                stateTimer -= dt;
                ShowWindUp(1f - stateTimer / WindUpTime);
                if (stateTimer <= 0f)
                {
                    if (IsRanged)
                        EnemyProjectile.Fire(transform.position + Vector3.up * (t.goblinHeight * 0.75f), Game.Player.position + Vector3.up * 1.1f);
                    else if (distance <= t.goblinHitRange && height <= t.goblinAttackVerticalReach)
                        Game.PlayerHealth.TakeDamage(t.goblinDamage * FloorScaling.EnemyDamage, transform.position);
                    ResetLook();
                    state = State.Recover;
                    stateTimer = IsRanged ? t.sprayerFireCooldown : t.goblinAttackCooldown;
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

    bool CanSeePlayer()
    {
        Vector3 from = transform.position + Vector3.up * (Game.Tuning.goblinHeight * 0.75f);
        Vector3 to = Game.Player.position + Vector3.up * 1.1f;
        if (!Physics.Linecast(from, to, out RaycastHit hit, ~(1 << 2), QueryTriggerInteraction.Ignore)) return true;
        return hit.collider.GetComponentInParent<PlayerHealth>() != null || hit.collider.GetComponentInParent<GoblinEnemy>() != null;
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
        if (IsDead || IsBusyMoving) return;
        heldUntil = Time.time + 0.1f;
        holdCentre = centre;
        holdPullSpeed = pullSpeed;
        holdRadius = stopRadius;
    }

    // A vortex over a pit: lifted off the floor; it drops when the vortex ends.
    public void HoldOverPit(Vector3 centre, float pullSpeed)
    {
        if (IsDead || traversal.Active) return;
        airborne.HoldOverPit(centre, pullSpeed);
        if (hitFlashTimer <= 0f) material.SetColor("_BaseColor", Palette.HeldTint);
        Mats.SetEmission(material, Palette.Arcane * Game.Tuning.heldGlow);
    }

    // Sent flying (barrel blast, later Arcane Explosion). Goblins held by a vortex don't budge.
    public void Knockback(Vector3 velocity, Cause source)
    {
        if (IsDead || IsHeld) return;
        if (state == State.WindUp) ResetLook();
        state = State.Chase;
        airborne.Launch(velocity, source);
    }

    // Held: any attack is cancelled, and the goblin slides toward the vortex centre.
    void UpdateHeld(float dt)
    {
        if (state == State.WindUp) ResetLook();
        state = State.Chase; // resume chasing once released
        repathTimer = 0f;
        if (!agent.isOnNavMesh) return;
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
        stateTimer = WindUpTime;
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
        if (state != State.WindUp) return BodyColor;
        return Color.Lerp(BodyColor, Palette.GoblinWindUp, WindUp01);
    }

    void ResetLook()
    {
        body.localScale = bodyScale;
        material.SetColor("_BaseColor", CurrentColor());
        Mats.SetEmission(material, BaseGlow());
    }

    Color BaseGlow() => BodyColor * Game.Tuning.goblinBaseGlow;

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

    public void TakeDamage(float amount, Cause cause = Cause.Spell)
    {
        if (IsDead) return;
        health -= amount;
        lastCause = cause;
        hitFlashTimer = Game.Tuning.enemyHitFlashTime;
        material.SetColor("_BaseColor", Palette.EnemyHitFlash);
        if (health <= 0f) Die();
    }

    // Knocked or dropped into a pit: dead on the spot, with a bonus.
    public void RingOut()
    {
        if (IsDead) return;
        lastCause = Cause.RingOut;
        WorldPopup.Show(transform.position + Vector3.up * 1.5f, "RING OUT!", Palette.Gold);
        Game.Player.GetComponent<PlayerLevel>().AddXp(Game.Tuning.ringOutXpBonus);
        Die();
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
        if (lastCause == Cause.RingOut) RunStats.RingOutKills++;
        else if (lastCause == Cause.Barrel) RunStats.BarrelKills++;
        Killed?.Invoke(transform.position);
        pool.Release(this);
    }
}
