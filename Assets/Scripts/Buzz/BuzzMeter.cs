using UnityEngine;

// The Buzz value, its slow drain, and the Transcendence -> Crash cycle at 100%.
// Classic and Hangover modes differ in the damage curve, Transcendence and the Crash slowdown;
// the Tab (Hangover only) lives in HangoverTab.
public class BuzzMeter : MonoBehaviour
{
    public enum Phase { Normal, Transcendent, Crash }

    public float Value { get; private set; }
    public Phase Current { get; private set; }
    public float PhaseTimeLeft { get; private set; }

    public float Normalized => Value / Game.Tuning.buzzMax;
    public bool IsHigh => Normalized >= Game.Tuning.buzzHighThreshold;

    public static bool Hangover => Game.Tuning.buzzMode == BuzzMode.Hangover;

    public float DamageMultiplier
    {
        get
        {
            var t = Game.Tuning;
            if (!Hangover)
                return Current == Phase.Transcendent ? t.transcendenceDamageMultiplier : 1f + Normalized * t.buzzMaxDamageBonus;
            if (Current == Phase.Transcendent) return t.hangoverTranscendenceDamageMultiplier;
            return 1f + (t.hangoverMaxDamageMultiplier - 1f) * Mathf.Pow(Mathf.Clamp01(Normalized), t.hangoverDamageExponent);
        }
    }
    public float FireRateMultiplier => Current == Phase.Transcendent ? Game.Tuning.transcendenceFireRateMultiplier : 1f;
    // Hangover mode never slows you down.
    public float MoveSpeedMultiplier => Current == Phase.Crash && !Hangover ? Game.Tuning.crashMoveSpeedMultiplier : 1f;
    public float TranscendenceDuration => Hangover ? Game.Tuning.hangoverTranscendenceDuration : Game.Tuning.transcendenceDuration;

    // The Crash just started (the Tab is paid then).
    public event System.Action CrashStarted;

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
                    CrashStarted?.Invoke();
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
        PhaseTimeLeft = TranscendenceDuration;
        RunStats.Transcendences++;
        Hud.ShowMessage("TRANSCENDENCE");
    }
}
