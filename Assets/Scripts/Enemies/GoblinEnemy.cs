using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Goblin intern brain: chase, wind up (telegraph), strike, recover.
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinEnemy : MonoBehaviour
{
    public static readonly List<GoblinEnemy> Active = new List<GoblinEnemy>();
    // (where it died). Used to drop loot.
    public static event System.Action<Vector3> Killed;
    static AudioClip behindCueClip;

    enum State { Chase, WindUp, Recover }

    public bool IsDead { get; private set; }

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
        health = t.goblinHealth;
        IsDead = false;
        state = State.Chase;
        repathTimer = 0f;
        hitFlashTimer = 0f;
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
                        Game.PlayerHealth.TakeDamage(t.goblinDamage, transform.position);
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

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= dt;
            material.SetColor("_BaseColor", hitFlashTimer > 0f ? Palette.EnemyHitFlash : CurrentColor());
        }
    }

    void StartWindUp()
    {
        var t = Game.Tuning;
        state = State.WindUp;
        stateTimer = t.goblinWindUpTime;
        if (t.behindCueEnabled && !IsOnScreen()) PlayBehindCue();
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
        if (state != State.WindUp) return Palette.Goblin;
        float k = 1f - stateTimer / Game.Tuning.goblinWindUpTime;
        return Color.Lerp(Palette.Goblin, Palette.GoblinWindUp, k);
    }

    void ResetLook()
    {
        body.localScale = bodyScale;
        material.SetColor("_BaseColor", Palette.Goblin);
        Mats.SetEmission(material, BaseGlow());
    }

    static Color BaseGlow() => Palette.Goblin * Game.Tuning.goblinBaseGlow;

    void Face(Vector3 flatDirection)
    {
        if (flatDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatDirection);
    }

    bool IsOnScreen()
    {
        Vector3 vp = Game.Camera.WorldToViewportPoint(transform.position + Vector3.up * (Game.Tuning.goblinHeight / 2f));
        return vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
    }

    void PlayBehindCue()
    {
        var t = Game.Tuning;
        if (behindCueClip == null) behindCueClip = ToneGenerator.Beep("BehindCue", t.behindCuePitchHz, t.behindCueDuration);
        audioSource.maxDistance = t.behindCueMaxDistance;
        audioSource.PlayOneShot(behindCueClip, t.behindCueVolume);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        health -= amount;
        hitFlashTimer = Game.Tuning.enemyHitFlashTime;
        material.SetColor("_BaseColor", Palette.EnemyHitFlash);
        if (health <= 0f) Die();
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
