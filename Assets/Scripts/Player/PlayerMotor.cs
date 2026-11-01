using UnityEngine;
using UnityEngine.InputSystem;

// WASD walking, gravity and the Space dash.
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    CharacterController cc;
    PlayerHealth health;
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
    }

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
        if (canControl && kb.spaceKey.wasPressedThisFrame && dashCooldownLeft <= 0f)
        {
            dashDirection = move.sqrMagnitude > 0.01f ? move.normalized : transform.forward;
            dashTimeLeft = t.dashDuration;
            dashCooldownLeft = t.dashCooldown;
            health.GrantInvulnerability(t.dashInvulnerability);
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
}
