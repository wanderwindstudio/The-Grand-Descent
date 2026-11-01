using UnityEngine;

// The Buzz value, its slow drain, and the Transcendence -> Crash cycle at 100%.
public class BuzzMeter : MonoBehaviour
{
    public enum Phase { Normal, Transcendent, Crash }

    public float Value { get; private set; }
    public Phase Current { get; private set; }
    public float PhaseTimeLeft { get; private set; }

    public float Normalized => Value / Game.Tuning.buzzMax;
    public bool IsHigh => Normalized >= Game.Tuning.buzzHighThreshold;

    public float DamageMultiplier => Current == Phase.Transcendent
        ? Game.Tuning.transcendenceDamageMultiplier
        : 1f + Normalized * Game.Tuning.buzzMaxDamageBonus;
    public float FireRateMultiplier => Current == Phase.Transcendent ? Game.Tuning.transcendenceFireRateMultiplier : 1f;
    public float MoveSpeedMultiplier => Current == Phase.Crash ? Game.Tuning.crashMoveSpeedMultiplier : 1f;

    // Shrines and pickups. Ignored while transcendent or crashing.
    public void Add(float amount)
    {
        if (Current != Phase.Normal) return;
        Value = Mathf.Min(Game.Tuning.buzzMax, Value + amount);
        if (Value >= Game.Tuning.buzzMax) StartTranscendence();
    }

    // Debug: F2 and the Buzz slider.
    public void Fill()
    {
        Current = Phase.Normal;
        Add(Game.Tuning.buzzMax);
    }

    public void SetValue(float value)
    {
        if (Current != Phase.Normal) return;
        Value = Mathf.Clamp(value, 0f, Game.Tuning.buzzMax);
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        if (IsHigh) RunStats.SecondsAboveBuzzThreshold += dt;

        switch (Current)
        {
            case Phase.Normal:
                Value = Mathf.Max(0f, Value - t.buzzDrainPerSecond * dt);
                break;

            case Phase.Transcendent:
                Value = t.buzzMax;
                PhaseTimeLeft -= dt;
                if (PhaseTimeLeft <= 0f)
                {
                    Current = Phase.Crash;
                    PhaseTimeLeft = t.crashDuration;
                    Value = 0f;
                    Hud.ShowMessage("CRASH");
                }
                break;

            case Phase.Crash:
                Value = 0f;
                PhaseTimeLeft -= dt;
                if (PhaseTimeLeft <= 0f) Current = Phase.Normal;
                break;
        }
    }

    void StartTranscendence()
    {
        Current = Phase.Transcendent;
        PhaseTimeLeft = Game.Tuning.transcendenceDuration;
        RunStats.Transcendences++;
        Hud.ShowMessage("TRANSCENDENCE");
    }
}
