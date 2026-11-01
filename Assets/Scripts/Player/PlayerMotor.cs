using UnityEngine;
using UnityEngine.InputSystem;

// WASD walking, gravity and the Dash action (Blink upgrade: a short teleport instead).
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    static readonly RaycastHit[] blinkHits = new RaycastHit[16];

    CharacterController cc;
    PlayerHealth health;
    PlayerUpgrades upgrades;
    float verticalSpeed;
    float dashTimeLeft;
    float dashCooldownLeft;
    Vector3 dashDirection;

    // 0 = just dashed, 1 = ready. Used by the HUD.
    public float DashReady01 => 1f - Mathf.Clamp01(dashCooldownLeft / Game.Tuning.dashCooldown);

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth>();
        upgrades = GetComponent<PlayerUpgrades>();
    }

    public void ResetCooldown() => dashCooldownLeft = 0f;

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
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

        Vector3 move = (transform.right * input.x + transform.forward * input.y) * t.moveSpeed * Game.Buzz.MoveSpeedMultiplier;

        dashCooldownLeft -= dt;
        if (canControl && Controls.Dash.WasPressedThisFrame() && dashCooldownLeft <= 0f)
        {
            dashDirection = move.sqrMagnitude > 0.01f ? move.normalized : transform.forward;
            dashCooldownLeft = t.dashCooldown;
            health.GrantInvulnerability(t.dashInvulnerability);
            FirstPersonHands.Instance?.OnDash();
            if (upgrades.Has(UpgradeType.Blink)) Blink(dashDirection);
            else dashTimeLeft = t.dashDuration;
        }
        if (dashTimeLeft > 0f)
        {
            dashTimeLeft -= dt;
            move = dashDirection * t.dashSpeed;
        }

        if (cc.isGrounded && verticalSpeed < 0f) verticalSpeed = t.groundStickSpeed;
        verticalSpeed += t.gravity * dt;
        move.y = verticalSpeed;

        cc.Move(move * dt);
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
