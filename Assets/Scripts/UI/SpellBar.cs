using UnityEngine;

// Bottom centre, between the hands: the Arcane Charge crystals and the spell bar below them.
// Spell icons carry their key label; a spell on cooldown gets a dark radial sweep; Arcane Explosion is locked.
// Sizes are for a 1080p screen and scale with the screen height.
public class SpellBar : MonoBehaviour
{
    const int SweepSteps = 60; // the sweep texture is rebuilt only when the fill crosses one of these steps

    VortexCaster vortex;
    Texture2D sweep, padlock;
    int sweepStep = -1;
    GUIStyle keyStyle, timerStyle;

    void Start()
    {
        vortex = Game.Player.GetComponent<VortexCaster>();
        sweep = new Texture2D(96, 96, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        padlock = MakePadlockTexture(32);
    }

    void OnGUI()
    {
        if (RunManager.Instance.Current != RunManager.State.Playing) return;
        if (keyStyle == null)
        {
            keyStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            timerStyle = new GUIStyle(keyStyle);
        }
        var t = Game.Tuning;
        float w = Screen.width, h = Screen.height;
        float s = Mathf.Clamp(h / 1080f, 0.6f, 2.5f);
        keyStyle.fontSize = Mathf.RoundToInt(18f * s);
        timerStyle.fontSize = Mathf.RoundToInt(24f * s);

        float icon = t.spellIconSize * s, gap = icon * 0.25f, labelH = 24f * s;
        float barW = 3f * icon + 2f * gap;
        float iconY = h - t.spellBarBottomMargin * s - labelH - icon;
        float x0 = w / 2f - barW / 2f;

        DrawSpell(new Rect(x0, iconY, icon, icon), Art.IconMissile, Controls.KeyName(Controls.Fire), 0f, 0f, false);
        DrawSpell(new Rect(x0 + icon + gap, iconY, icon, icon), Art.IconVortex, Controls.KeyName(Controls.CastVortex),
                  vortex.Cooldown01, vortex.CooldownLeft, false);
        DrawSpell(new Rect(x0 + 2f * (icon + gap), iconY, icon, icon), Art.IconExplosionGrey, Controls.KeyName(Controls.CastExplosion), 0f, 0f, true);

        DrawCharges(w, iconY - 8f * s, s);
        GUI.color = Color.white;
    }

    void DrawSpell(Rect r, Texture2D icon, string key, float cooldown01, float cooldownLeft, bool locked)
    {
        GUI.color = locked ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
        GUI.DrawTexture(r, icon);

        if (cooldown01 > 0f)
        {
            UpdateSweep(cooldown01);
            GUI.color = Color.white;
            GUI.DrawTexture(r, sweep);
            GUI.Label(r, cooldownLeft.ToString("0"), timerStyle);
        }
        if (locked)
        {
            float p = r.width * 0.42f;
            GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
            GUI.DrawTexture(new Rect(r.center.x - p / 2f, r.center.y - p / 2f, p, p), padlock);
        }

        GUI.color = locked ? new Color(0.6f, 0.6f, 0.6f) : Color.white;
        GUI.Label(new Rect(r.x - r.width * 0.25f, r.yMax, r.width * 1.5f, keyStyle.fontSize * 1.4f), locked ? key + " (locked)" : key, keyStyle);
    }

    // Arcane Charges as crystals: lit ones in full colour with a soft pulse, empty ones grey and faint.
    // When the charges reset, all of them flash.
    void DrawCharges(float w, float bottom, float s)
    {
        var t = Game.Tuning;
        var charges = Game.Charges;
        int max = charges.Max;
        float size = t.chargeIconSize * s;
        float step = size * 0.62f; // the crystal fills only the middle ~60% of its canvas
        float y = bottom - size;
        float x0 = w / 2f - (step * (max - 1) + size) / 2f;
        bool flashing = Time.unscaledTime < charges.ResetFlashUntil;
        bool flashOn = flashing && Mathf.Repeat(Time.unscaledTime, 0.16f) < 0.08f;

        for (int i = 0; i < max; i++)
        {
            var r = new Rect(x0 + i * step, y, size, size);
            if (flashing)
            {
                // Alternate a bright silver crystal with a red-tinted one
                GUI.color = flashOn ? Color.white : Palette.HitIndicator;
                GUI.DrawTexture(r, flashOn ? Art.IconChargeGrey : Art.IconCharge);
            }
            else if (i < charges.Charges)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * t.chargePulseSpeed + i * 0.7f);
                float grow = 1f + t.chargePulseAmount * 0.3f * pulse;
                var pr = new Rect(r.center.x - r.width * grow / 2f, r.center.y - r.height * grow / 2f, r.width * grow, r.height * grow);
                float b = 1f - t.chargePulseAmount + t.chargePulseAmount * pulse;
                GUI.color = new Color(b, b, b, 1f);
                GUI.DrawTexture(pr, Art.IconCharge);
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, t.emptyChargeAlpha);
                GUI.DrawTexture(r, Art.IconChargeGrey);
            }
        }
    }

    // A dark disc with a wedge cut out: the remaining cooldown, shrinking clockwise from 12 o'clock.
    void UpdateSweep(float remaining01)
    {
        int stepNow = Mathf.CeilToInt(remaining01 * SweepSteps);
        if (stepNow == sweepStep) return;
        sweepStep = stepNow;
        float remaining = stepNow / (float)SweepSteps;
        int size = sweep.width;
        var pixels = new Color32[size * size];
        var dark = new Color32(0, 0, 0, (byte)(255 * Game.Tuning.cooldownSweepAlpha));
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f, dy = (y + 0.5f) / size - 0.5f; // texture y=0 is the bottom
                if (dx * dx + dy * dy > 0.47f * 0.47f) continue;
                float around = Mathf.Repeat(Mathf.Atan2(dx, dy) / (2f * Mathf.PI), 1f); // 0 at 12 o'clock, clockwise
                if (around >= 1f - remaining) pixels[y * size + x] = dark;
            }
        sweep.SetPixels32(pixels);
        sweep.Apply();
    }

    // A simple white padlock: a shackle ring on top of a solid body.
    static Texture2D MakePadlockTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size, v = (y + 0.5f) / size; // v=0 is the bottom
                bool body = u > 0.15f && u < 0.85f && v > 0.05f && v < 0.55f;
                float dx = u - 0.5f, dy = v - 0.55f, d = Mathf.Sqrt(dx * dx + dy * dy);
                bool shackle = v >= 0.55f && d > 0.18f && d < 0.3f;
                bool keyhole = Mathf.Abs(u - 0.5f) < 0.05f && v > 0.2f && v < 0.4f;
                tex.SetPixel(x, y, (body && !keyhole) || shackle ? Color.white : Color.clear);
            }
        tex.Apply();
        return tex;
    }
}
