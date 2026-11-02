using System.Collections.Generic;
using UnityEngine;

// On-screen info: crosshair, bars, hurt flash, hit-direction arrows, pop-up messages.
public class Hud : MonoBehaviour
{
    static Hud instance;

    class HitMarker { public Vector3 source; public float timeLeft; }
    readonly List<HitMarker> hitMarkers = new List<HitMarker>();

    Texture2D arrow;
    GUIStyle bigText, smallText, leftText, rightText, topRightText;
    float flashTimeLeft;
    string message;
    float messageTimeLeft;
    PlayerMotor motor;
    PlayerLevel level;
    PlayerUpgrades upgrades;
    WaveSpawner waves;
    SpellSwap spellSwap;
    CollapseTimer collapse;

    const float MessageDuration = 2.5f;

    public static void ShowMessage(string text)
    {
        if (instance == null) return;
        instance.message = text;
        instance.messageTimeLeft = MessageDuration;
    }

    void Awake()
    {
        instance = this;
        arrow = MakeArrowTexture(64);
    }

    void Start()
    {
        Game.PlayerHealth.Damaged += OnPlayerDamaged;
        motor = Game.Player.GetComponent<PlayerMotor>();
        level = Game.Player.GetComponent<PlayerLevel>();
        upgrades = Game.Player.GetComponent<PlayerUpgrades>();
        waves = GetComponent<WaveSpawner>();
        spellSwap = Game.Player.GetComponent<SpellSwap>();
        collapse = GetComponent<CollapseTimer>();
    }

    void OnPlayerDamaged(Vector3 source, float amount)
    {
        flashTimeLeft = Game.Tuning.damageFlashDuration;
        hitMarkers.Add(new HitMarker { source = source, timeLeft = Game.Tuning.hitIndicatorDuration });
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        flashTimeLeft -= dt;
        messageTimeLeft -= dt;
        for (int i = hitMarkers.Count - 1; i >= 0; i--)
        {
            hitMarkers[i].timeLeft -= dt;
            if (hitMarkers[i].timeLeft <= 0f) hitMarkers.RemoveAt(i);
        }
    }

    void OnGUI()
    {
        if (RunManager.Instance.Current != RunManager.State.Playing || PauseMenu.IsOpen) return; // RunScreens draws instead
        if (bigText == null) MakeStyles();
        var t = Game.Tuning;
        float w = Screen.width, h = Screen.height;

        // Red hurt flash
        if (flashTimeLeft > 0f)
        {
            float a = t.damageFlashMaxAlpha * (flashTimeLeft / t.damageFlashDuration);
            Fill(new Rect(0, 0, w, h), new Color(1f, 0f, 0f, a));
        }

        DrawCollapse(w, h);
        DrawOffscreenWarnings(w, h);
        DrawHitMarkers(w, h);

        // Crosshair
        Fill(new Rect(w / 2 - 1, h / 2 - 8, 2, 16), Color.white);
        Fill(new Rect(w / 2 - 8, h / 2 - 1, 16, 2), Color.white);

        // Health and dash bars, bottom left
        var hp = Game.PlayerHealth;
        Bar(new Rect(20, h - 50, 300, 22), hp.Current / hp.Max, Palette.HealthBar, $"HP {Mathf.CeilToInt(hp.Current)} / {Mathf.CeilToInt(hp.Max)}");
        if (motor != null) Bar(new Rect(20, h - 22, 300, 8), motor.DashReady01, Palette.DashBar, null);
        DrawBuzzBar(new Rect(20, h - 82, 300, 22));
        DrawSpellSwapWarning(w, h);
        DrawShrinePrompt(w, h);

        // XP bar across the top
        Bar(new Rect(0, 0, w, 10), level.Xp / level.XpToNext, Palette.XpBar, null);
        GUI.Label(new Rect(20, 14, 200, 24), $"Level {level.Level}", leftText);

        // Top right info
        GUI.Label(new Rect(w - 260, 14, 240, 24), $"Floor {Game.Floor}", rightText);
        GUI.Label(new Rect(w - 260, 38, 240, 24), $"Wave {Mathf.Max(1, waves.WaveNumber)} / {waves.WaveCount}", rightText);
        GUI.Label(new Rect(w - 260, 62, 240, 24), $"Enemies left: {waves.EnemiesRemaining}", rightText);
        GUI.color = Palette.Gold;
        GUI.Label(new Rect(w - 260, 86, 240, 24), $"Gold: {RunManager.Instance.CarriedGold}", rightText);
        GUI.color = Color.white;
        if (Game.GodMode) GUI.Label(new Rect(w - 260, 110, 240, 24), "GOD MODE", rightText);

        DrawPortalLabels();

        // Upgrades owned, top right under the run info (the staff hand covers the bottom-right corner)
        GUI.Label(new Rect(w - 260, 140, 240, 260), upgrades.Summary(), topRightText);

        if (messageTimeLeft > 0f)
            GUI.Label(new Rect(0, h * 0.2f, w, 50), message, bigText);
    }

    // Big countdown at the top; the screen edges pulse red in the last seconds.
    void DrawCollapse(float w, float h)
    {
        if (!collapse.Active) return;
        var t = Game.Tuning;
        if (collapse.TimeLeft <= t.collapseWarningTime)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 10f);
            float a = 0.25f * pulse * (1f - collapse.TimeLeft / t.collapseWarningTime);
            Color edge = new Color(1f, 0f, 0f, a);
            Fill(new Rect(0, 0, w, 40), edge);
            Fill(new Rect(0, h - 40, w, 40), edge);
            Fill(new Rect(0, 0, 40, h), edge);
            Fill(new Rect(w - 40, 0, 40, h), edge);
        }
        GUI.color = Palette.HitIndicator;
        GUI.Label(new Rect(0, 30, w, 50), $"COLLAPSE  {collapse.TimeLeft:0.0}", bigText);
        GUI.color = Color.white;
    }

    // Amber arrows for goblins you can't see that are winding up an attack. They grow as the hit nears.
    void DrawOffscreenWarnings(float w, float h)
    {
        var t = Game.Tuning;
        if (!t.offscreenWarningArrows) return;
        Vector3 forward = Game.Player.forward;
        forward.y = 0f;
        Vector2 centre = new Vector2(w / 2f, h / 2f);

        foreach (var g in GoblinEnemy.Active)
        {
            if (!g.IsWindingUp || g.IsOnScreen()) continue;
            Vector3 toEnemy = g.transform.position - Game.Player.position;
            toEnemy.y = 0f;
            if (toEnemy.magnitude > t.behindCueMaxDistance) continue;

            float k = g.WindUp01;
            float size = t.hitIndicatorSize * Mathf.Lerp(0.6f, 1.1f, k);
            float angle = Vector3.SignedAngle(forward, toEnemy, Vector3.up);
            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, centre);
            GUI.color = new Color(Palette.OffscreenWarning.r, Palette.OffscreenWarning.g, Palette.OffscreenWarning.b, Mathf.Lerp(0.4f, 1f, k));
            GUI.DrawTexture(new Rect(centre.x - size / 2f, centre.y - t.hitIndicatorRadius - size - 20f, size, size), arrow);
            GUI.matrix = old;
        }
        GUI.color = Color.white;
    }

    // Floating names over the portals.
    void DrawPortalLabels()
    {
        foreach (var portal in Portal.All)
        {
            Vector3 sp = Game.Camera.WorldToScreenPoint(portal.transform.position + Vector3.up * 2f);
            if (sp.z <= 0f) continue;
            GUI.color = portal.Kind == PortalKind.Extract ? Palette.ExtractPortal : Palette.DescendPortal;
            GUI.Label(new Rect(sp.x - 120, Screen.height - sp.y - 25, 240, 50), Portal.Label(portal.Kind), smallText);
        }
        GUI.color = Color.white;
    }

    void DrawBuzzBar(Rect r)
    {
        var t = Game.Tuning;
        var buzz = Game.Buzz;
        switch (buzz.Current)
        {
            case BuzzMeter.Phase.Transcendent:
                Bar(r, buzz.PhaseTimeLeft / t.transcendenceDuration, Palette.TranscendenceBar, $"TRANSCENDENCE {buzz.PhaseTimeLeft:0.0}s   DMG x{buzz.DamageMultiplier:0.0}");
                break;
            case BuzzMeter.Phase.Crash:
                Bar(r, buzz.PhaseTimeLeft / t.crashDuration, Palette.CrashBar, $"CRASH {buzz.PhaseTimeLeft:0.0}s");
                break;
            default:
                Bar(r, buzz.Normalized, Palette.BuzzBar, $"BUZZ {buzz.Normalized * 100f:0}%   DMG x{buzz.DamageMultiplier:0.00}");
                // Tick mark where spell swaps start.
                Fill(new Rect(r.x + r.width * t.buzzHighThreshold - 1, r.y - 3, 2, r.height + 6), Color.white);
                break;
        }
    }

    // Under the crosshair: what the next shot will turn into, with a countdown bar.
    void DrawSpellSwapWarning(float w, float h)
    {
        var t = Game.Tuning;
        SpellVariant shown = spellSwap.Armed != SpellVariant.Normal ? spellSwap.Armed : spellSwap.Warning;
        if (shown == SpellVariant.Normal) return;

        Color color = shown == SpellVariant.HugeOrb ? Palette.HugeOrb : Palette.Spark;
        bool armed = spellSwap.Armed != SpellVariant.Normal;
        string text = armed ? $"{SpellSwap.Name(shown)} READY" : $"NEXT SHOT: {SpellSwap.Name(shown)}";
        float y = h / 2f + 40f;

        GUI.color = armed && Mathf.Repeat(Time.unscaledTime, 0.3f) < 0.15f ? Color.white : color;
        GUI.Label(new Rect(0, y, w, 30), text, smallText);
        GUI.color = Color.white;
        float fill = armed ? 1f : 1f - spellSwap.WarningTimeLeft / t.spellSwapWarning;
        Bar(new Rect(w / 2f - 80f, y + 30f, 160f, 6f), fill, color, null);
    }

    void DrawShrinePrompt(float w, float h)
    {
        var shrine = PipeShrine.InRange();
        if (shrine == null) return;
        string text;
        if (!shrine.Ready) text = "Pipe is recharging...";
        else if (Game.Buzz.Current != BuzzMeter.Phase.Normal) text = "Can't hit the pipe right now";
        else text = $"Press {Controls.KeyName(Controls.Interact)} to hit the pipe  (+{Game.Tuning.shrineBuzzAmount:0} Buzz)";
        GUI.Label(new Rect(0, h * 0.65f, w, 30), text, smallText);
    }

    // Arrows around the crosshair pointing toward whoever hurt you. They keep tracking as you turn.
    void DrawHitMarkers(float w, float h)
    {
        var t = Game.Tuning;
        Vector3 forward = Game.Player.forward;
        forward.y = 0f;
        Vector2 centre = new Vector2(w / 2f, h / 2f);
        float size = t.hitIndicatorSize;

        foreach (var marker in hitMarkers)
        {
            Vector3 toSource = marker.source - Game.Player.position;
            toSource.y = 0f;
            float angle = Vector3.SignedAngle(forward, toSource, Vector3.up);

            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, centre);
            GUI.color = new Color(Palette.HitIndicator.r, Palette.HitIndicator.g, Palette.HitIndicator.b, marker.timeLeft / t.hitIndicatorDuration);
            GUI.DrawTexture(new Rect(centre.x - size / 2f, centre.y - t.hitIndicatorRadius - size, size, size), arrow);
            GUI.matrix = old;
        }
        GUI.color = Color.white;
    }

    static void Fill(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void Bar(Rect r, float fill01, Color color, string label)
    {
        Fill(r, Palette.BarBackground);
        Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(fill01), r.height), color);
        if (label != null) GUI.Label(new Rect(r.x + 6, r.y, r.width, r.height), label);
    }

    void MakeStyles()
    {
        bigText = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        smallText = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        leftText = new GUIStyle(smallText) { alignment = TextAnchor.MiddleLeft };
        rightText = new GUIStyle(smallText) { alignment = TextAnchor.MiddleRight };
        topRightText = new GUIStyle(smallText) { alignment = TextAnchor.UpperRight, fontSize = 15 };
    }

    // A white triangle pointing up.
    static Texture2D MakeArrowTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            float halfWidth = (1f - y / (float)size) * size / 2f; // texture y=0 is the bottom (wide end)
            for (int x = 0; x < size; x++)
            {
                bool inside = Mathf.Abs(x - size / 2f) <= halfWidth;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return tex;
    }
}
