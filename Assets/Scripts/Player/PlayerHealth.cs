using System;
using UnityEngine;

// Player hit points, invulnerability and death.
public class PlayerHealth : MonoBehaviour
{
    public float Current { get; private set; }
    public float Max { get; private set; }
    public bool IsDead { get; private set; }

    // (position the damage came from, amount)
    public event Action<Vector3, float> Damaged;
    public event Action Died;

    float invulnerableUntil;

    void Awake()
    {
        Max = Game.Tuning.playerMaxHealth;
        Current = Max;
    }

    public void GrantInvulnerability(float seconds)
    {
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }

    public bool TakeDamage(float amount, Vector3 sourcePosition)
    {
        if (IsDead || Game.GodMode || Time.time < invulnerableUntil) return false;

        Current = Mathf.Max(0f, Current - amount);
        Damaged?.Invoke(sourcePosition, amount);
        if (Current <= 0f) Die();
        return true;
    }

    void Die()
    {
        IsDead = true;
        Time.timeScale = 0f;
        PlayerLook.UnlockCursor();
        Died?.Invoke();
    }
}
