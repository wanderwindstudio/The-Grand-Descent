using UnityEngine;
using UnityEngine.InputSystem;

// WASD walking, jumping, ladders, gravity, knockback and the Dash action (Blink upgrade: a short teleport instead).
// Jumping: coyote time, a jump buffer, a shorter hop when Jump is released early, heavier gravity on the way down,
// and less steering in the air.
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    static readonly RaycastHit[] blinkHits = new RaycastHit[16];

    // (metres fallen from the highest point, true if it was a fall over the damage height)
    public event System.Action<float, bool> Landed;

    public bool IsGrounded { get; private set; } = true;
    public bool IsClimbing => ladder != null;

    CharacterController cc;
    PlayerHealth health;
    PlayerUpgrades upgrades;
    float verticalSpeed;
    Vector3 airVelocity;      // horizontal speed carried through the air
    Vector3 knockback;        // horizontal push from explosions, fades out
    float dashTimeLeft;
    float dashCooldownLeft;
    Vector3 dashDirection;
    float lastGroundedTime = -1f, jumpPressedTime = -1f;
    bool jumping, jumpCut;
    float highestY;
    RoomLadder ladder;
    Vector3 lastSafePosition;

    // 0 = just dashed, 1 = ready. Used by the HUD.
    public float DashReady01 => 1f - Mathf.Clamp01(dashCooldownLeft / Game.Tuning.dashCooldown);

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth>();
        upgrades = GetComponent<PlayerUpgrades>();
    }

    void Start() => ResetMotion();

    public void ResetCooldown() => dashCooldownLeft = 0f;

    // A shove, e.g. from a barrel explosion. The upward part launches you.
    public void AddImpulse(Vector3 velocity)
    {
        knockback += new Vector3(velocity.x, 0f, velocity.z);
        if (velocity.y > 0f)
        {
            verticalSpeed = Mathf.Max(verticalSpeed, velocity.y);
            IsGrounded = false;
        }
    }

    // After a teleport (new floor): forget the old fall height and momentum.
    public void ResetMotion()
    {
        verticalSpeed = 0f;
        airVelocity = knockback = Vector3.zero;
        highestY = transform.position.y;
        lastSafePosition = transform.position;
        ladder = null;
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        var kb = Keyboard.current;

        Vector2 input = Vector2.zero;
        bool canControl = kb != null && !Game.InputBlocked && !health.IsDead;
        if (canControl)
        {
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
        }
        input = Vector2.ClampMagnitude(input, 1f); // diagonals aren't faster

        Vector3 wish = (transform.right * input.x + transform.forward * input.y) * t.moveSpeed * Game.Buzz.MoveSpeedMultiplier;
        if (canControl && Controls.Jump.WasPressedThisFrame()) jumpPressedTime = Time.time;

        if (UpdateLadder(input, canControl, dt)) return;

        // Horizontal: instant on the ground, steered with reduced control in the air.
        Vector3 horizontal;
        if (IsGrounded) horizontal = airVelocity = wish;
        else
        {
            airVelocity = Vector3.MoveTowards(airVelocity, wish, t.airAcceleration * t.airControl * dt);
            horizontal = airVelocity;
        }

        dashCooldownLeft -= dt;
        if (canControl && Controls.Dash.WasPressedThisFrame() && dashCooldownLeft <= 0f)
        {
            dashDirection = wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
            dashCooldownLeft = t.dashCooldown;
            health.GrantInvulnerability(t.dashInvulnerability);
            FirstPersonHands.Instance?.OnDash();
            if (upgrades.Has(UpgradeType.Blink)) Blink(dashDirection);
            else dashTimeLeft = t.dashDuration;
        }
        if (dashTimeLeft > 0f)
        {
            dashTimeLeft -= dt;
            horizontal = dashDirection * t.dashSpeed;
            if (dashTimeLeft <= 0f && !IsGrounded) airVelocity = dashDirection * t.moveSpeed; // keep going after an air dash
        }

        // Jump: buffered press + coyote time.
        bool canJump = IsGrounded || (!jumping && Time.time - lastGroundedTime <= t.coyoteTime);
        if (canControl && canJump && Time.time - jumpPressedTime <= t.jumpBufferTime)
        {
            verticalSpeed = Mathf.Sqrt(2f * -t.gravity * t.jumpHeight);
            jumping = true;
            jumpCut = false;
            IsGrounded = false;
            jumpPressedTime = -1f;
            RunStats.Jumps++;
        }
        // Let go early: a shorter hop.
        if (jumping && !jumpCut && verticalSpeed > 0f && !Controls.Jump.IsPressed())
        {
            verticalSpeed *= t.jumpCutMultiplier;
            jumpCut = true;
        }

        if (IsGrounded && verticalSpeed < 0f) verticalSpeed = t.groundStickSpeed;
        // Half the gravity before the move and half after, so the jump peaks at jumpHeight at any frame rate.
        float gravity = verticalSpeed > 0f ? t.gravity : t.gravity * t.fallGravityMultiplier;
        verticalSpeed = Mathf.Max(verticalSpeed + gravity * dt * 0.5f, -t.maxFallSpeed);

        knockback = Vector3.Lerp(knockback, Vector3.zero, 1f - Mathf.Exp(-(IsGrounded ? t.knockbackDecay : 1f) * dt));
        Vector3 move = horizontal + knockback;
        move.y = verticalSpeed;
        var flags = cc.Move(move * dt);
        verticalSpeed = Mathf.Max(verticalSpeed + gravity * dt * 0.5f, -t.maxFallSpeed);
        if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f; // bumped the ceiling

        UpdateGrounded();
        CatchFallingOutOfTheWorld();
    }

    void UpdateGrounded()
    {
        var t = Game.Tuning;
        bool grounded = cc.isGrounded;
        if (!grounded)
        {
            highestY = Mathf.Max(highestY, transform.position.y);
            IsGrounded = false;
            return;
        }

        if (!IsGrounded)
        {
            float fall = highestY - transform.position.y;
            bool big = fall > t.playerFallDamageHeight;
            if (big) health.TakeDamage((fall - t.playerFallDamageHeight) * t.playerFallDamagePerMetre, transform.position + Vector3.down);
            Landed?.Invoke(fall, big);
        }
        IsGrounded = true;
        jumping = false;
        lastGroundedTime = Time.time;
        highestY = transform.position.y;
        lastSafePosition = transform.position;
    }

    // Ladders: walk into one while pressing forward to grab it. W/S climb, Jump lets go.
    // Returns true while climbing (normal movement is skipped).
    bool UpdateLadder(Vector2 input, bool canControl, float dt)
    {
        var t = Game.Tuning;
        RoomLadder here = RoomLadder.Find(transform.position + Vector3.up * 0.5f);
        if (ladder == null)
        {
            if (here == null || !canControl || input.y <= 0.1f) return false;
            if (Vector3.Dot(transform.forward, here.ClimbDirection) < 0.2f) return false; // must face it
            ladder = here;
        }
        if (here != ladder || (canControl && Controls.Jump.WasPressedThisFrame()))
        {
            // Let go (or climbed off the top)
            Vector3 push = here == ladder ? -ladder.ClimbDirection * 3f : Vector3.zero;
            ladder = null;
            airVelocity = push;
            verticalSpeed = push.sqrMagnitude > 0f ? 3f : 0f;
            jumpPressedTime = -1f;
            return false;
        }

        float climb = canControl ? input.y : 0f;
        Vector3 move = Vector3.up * (climb * t.ladderClimbSpeed);
        // At the top: step over onto the platform.
        if (transform.position.y >= ladder.TopY - 0.15f && climb > 0f)
            move += ladder.ClimbDirection * (t.ladderClimbSpeed * 0.8f);
        // Down at the bottom: step off.
        if (cc.isGrounded && climb < 0f) { ladder = null; return false; }

        // Stay lined up with the ladder.
        Vector3 toLine = ladder.Column(transform.position) - transform.position;
        toLine.y = 0f;
        move += toLine * 6f;

        cc.Move(move * dt);
        verticalSpeed = 0f;
        airVelocity = knockback = Vector3.zero;
        jumping = false;
        IsGrounded = false;
        highestY = transform.position.y; // no fall damage for climbing down
        lastGroundedTime = Time.time;
        return true;
    }

    // Safety net: fell through a gap in a room. Put the player back on the last solid ground.
    void CatchFallingOutOfTheWorld()
    {
        if (transform.position.y > lastSafePosition.y - 40f) return;
        cc.enabled = false;
        transform.position = lastSafePosition + Vector3.up * 0.5f;
        cc.enabled = true;
        ResetMotion();
        health.TakeDamage(10f, transform.position);
        Debug.LogWarning("Player fell out of the level and was put back. Check the room for a hole near " + lastSafePosition);
    }

    // Teleport up to blinkDistance, stopping just short of the first wall (goblins don't block it).
    void Blink(Vector3 direction)
    {
        var t = Game.Tuning;
        Vector3 start = transform.position;
        Vector3 bottom = start + Vector3.up * (cc.radius + 0.1f);
        Vector3 top = start + Vector3.up * (cc.height - cc.radius);
        float distance = t.blinkDistance;

        int count = Physics.CapsuleCastNonAlloc(bottom, top, cc.radius * 0.95f, direction, blinkHits, t.blinkDistance, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Collider c = blinkHits[i].collider;
            if (c.GetComponentInParent<PlayerHealth>() != null || c.GetComponentInParent<GoblinEnemy>() != null) continue;
            if (blinkHits[i].distance <= 0f) continue; // already touching (e.g. the floor)
            distance = Mathf.Min(distance, Mathf.Max(0f, blinkHits[i].distance - 0.1f));
        }

        // Afterimage where you were, a flash where you land.
        SpellFx.Impact(start + Vector3.up * t.eyeHeight * 0.6f, 1.6f, 0.4f, 0.35f, t.impactLightIntensity, t.impactLightRange);
        cc.enabled = false;
        transform.position = start + direction * distance;
        cc.enabled = true;
        SpellFx.Impact(transform.position + Vector3.up * t.eyeHeight * 0.6f, 0.3f, 1.4f, 0.25f, t.impactLightIntensity, t.impactLightRange);
    }
}
