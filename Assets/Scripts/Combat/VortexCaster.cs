using UnityEngine;

// Arcane Vortex: press CastVortex to throw a slow orb toward the crosshair. It becomes a vortex
// where it hits a wall, the floor or an enemy, or after a maximum distance.
public class VortexCaster : MonoBehaviour
{
    public float CooldownLeft { get; private set; }
    public float Cooldown01 => Mathf.Clamp01(CooldownLeft / Game.Tuning.vortexCooldown);

    PlayerUpgrades upgrades;

    void Awake() => upgrades = GetComponent<PlayerUpgrades>();

    public void ResetCooldown() => CooldownLeft = 0f;

    void Update()
    {
        CooldownLeft = Mathf.Max(0f, CooldownLeft - Time.deltaTime);
        if (Game.InputBlocked || Game.PlayerHealth.IsDead || Time.timeScale == 0f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (!Controls.CastVortex.WasPressedThisFrame() || CooldownLeft > 0f) return;

        CooldownLeft = Game.Tuning.vortexCooldown;
        RunStats.VortexCasts++;
        Vector3 origin = Aim.HandOrigin();
        Vector3 direction = (Aim.CrosshairTarget() - origin).normalized;
        VortexOrb.Throw(origin, direction, upgrades);
        FirstPersonHands.Instance?.OnVortexThrow();
    }
}
