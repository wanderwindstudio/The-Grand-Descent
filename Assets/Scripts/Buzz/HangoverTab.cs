using UnityEngine;

// Hangover mode: while Buzz is above 0 you're numb. Damage you take goes on the Tab instead of your health.
// The Tab slowly drains (that part is forgiven). When Buzz runs out, or the Crash starts, or the Tab grows
// past 150% of your max health, whatever is left hits at once: THE HANGOVER.
// During Transcendence the Tab is frozen: new damage is ignored.
public class HangoverTab : MonoBehaviour
{
    public float Amount { get; private set; }

    float shakeLeft;

    // Damage goes on the Tab instead of health right now.
    public bool Absorbing
    {
        get
        {
            if (!BuzzMeter.Hangover) return false;
            var buzz = Game.Buzz;
            return buzz.Current == BuzzMeter.Phase.Transcendent || (buzz.Current == BuzzMeter.Phase.Normal && buzz.Value > 0f);
        }
    }

    public bool Frozen => BuzzMeter.Hangover && Game.Buzz.Current == BuzzMeter.Phase.Transcendent;

    void Start() => Game.Buzz.CrashStarted += () => { if (BuzzMeter.Hangover) Pay(); };

    public void Add(float damage)
    {
        if (Frozen || damage <= 0f) return;
        Amount += damage;
        RunStats.MaxTab = Mathf.Max(RunStats.MaxTab, Amount);
        if (Amount > Game.Tuning.tabMaxFractionOfHealth * Game.PlayerHealth.Max) Pay();
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        if (!BuzzMeter.Hangover)
        {
            // Switched to Classic mid-run: the Tab is forgiven.
            if (Amount > 0f) { RunStats.TabForgiven += Amount; Amount = 0f; }
            return;
        }
        if (Amount > 0f && !Frozen && dt > 0f)
        {
            float forgiven = Amount * t.tabDrainPerSecond * dt;
            Amount -= forgiven;
            RunStats.TabForgiven += forgiven;
        }
        if (Amount > 0f && !Absorbing && !Game.PlayerHealth.IsDead) Pay(); // Buzz hit 0
    }

    void LateUpdate()
    {
        var t = Game.Tuning;
        if (shakeLeft <= 0f) { CameraOffsets.Jolt = Vector3.zero; return; }
        shakeLeft -= Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(shakeLeft / t.hangoverShakeTime);
        CameraOffsets.Jolt = Random.insideUnitSphere * (t.hangoverShake * k * k);
    }

    // The bill arrives: the whole Tab as real damage.
    public void Pay()
    {
        float bill = Amount;
        Amount = 0f;
        if (bill <= 0f) return;
        var t = Game.Tuning;
        RunStats.TabPaid += bill;
        Hud.ShowMessage($"THE HANGOVER   -{Mathf.CeilToInt(bill)}");
        Hud.Flash(t.hangoverFlashAlpha, t.hangoverFlashTime);
        shakeLeft = t.hangoverShakeTime;
        Game.PlayerHealth.LoseHealthFromHangover(bill);
    }

    // Extracting with a Tab: it becomes a gold penalty instead. Returns the gold to subtract.
    public int SettleInGold()
    {
        int gold = Mathf.CeilToInt(Amount * Game.Tuning.tabGoldPerPoint);
        Amount = 0f;
        return gold;
    }

    // Loading a save.
    public void Restore(float amount) => Amount = Mathf.Max(0f, amount);
}
