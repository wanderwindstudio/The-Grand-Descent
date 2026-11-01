using UnityEngine;

public enum SpellVariant { Normal, HugeOrb, Sparks }

// At high Buzz, every few seconds the next shot turns into a random variant.
// It is announced first (Warning), then Armed, and the next shot fired uses it.
public class SpellSwap : MonoBehaviour
{
    public SpellVariant Warning { get; private set; }  // announced, not active yet
    public float WarningTimeLeft { get; private set; }
    public SpellVariant Armed { get; private set; }    // the next shot will be this

    BuzzMeter buzz;
    float nextSwapIn;

    void Awake()
    {
        buzz = GetComponent<BuzzMeter>();
        nextSwapIn = RandomInterval();
    }

    void Update()
    {
        if (!buzz.IsHigh)
        {
            Warning = SpellVariant.Normal;
            Armed = SpellVariant.Normal;
            return;
        }
        if (Armed != SpellVariant.Normal) return; // waiting for you to shoot

        if (Warning != SpellVariant.Normal)
        {
            WarningTimeLeft -= Time.deltaTime;
            if (WarningTimeLeft <= 0f)
            {
                Armed = Warning;
                Warning = SpellVariant.Normal;
            }
            return;
        }

        nextSwapIn -= Time.deltaTime;
        if (nextSwapIn <= 0f)
        {
            Warning = Random.value < 0.5f ? SpellVariant.HugeOrb : SpellVariant.Sparks;
            WarningTimeLeft = Game.Tuning.spellSwapWarning;
            nextSwapIn = RandomInterval();
        }
    }

    // Called by the shooter for every shot. Returns the variant to fire and clears it.
    public SpellVariant ConsumeArmed()
    {
        var v = Armed;
        Armed = SpellVariant.Normal;
        return v;
    }

    static float RandomInterval() => Random.Range(Game.Tuning.spellSwapMinInterval, Game.Tuning.spellSwapMaxInterval);

    public static string Name(SpellVariant v) => v == SpellVariant.HugeOrb ? "HUGE ORB" : v == SpellVariant.Sparks ? "SPARK SPRAY" : "";
}
