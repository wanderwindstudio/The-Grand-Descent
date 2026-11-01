using System.Collections.Generic;
using UnityEngine;

// On-screen info: crosshair, bars, hurt flash, hit-direction arrows, pop-up messages.
public class Hud : MonoBehaviour
{
    static Hud instance;

    class HitMarker { public Vector3 source; public float timeLeft; }
    readonly List<HitMarker> hitMarkers = new List<HitMarker>();

    Texture2D arrow;
    GUIStyle bigText, smallText;
    float flashTimeLeft;
    string message;
    float messageTimeLeft;
    PlayerMotor motor;

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
        if (bigText == null) MakeStyles();
        var t = Game.Tuning;
        float w = Screen.width, h = Screen.height;

        // Red hurt flash
        if (flashTimeLeft > 0f)
        {
            float a = t.damageFlashMaxAlpha * (flashTimeLeft / t.damageFlashDuration);
            Fill(new Rect(0, 0, w, h), new Color(1f, 0f, 0f, a));
        }

        DrawHitMarkers(w, h);

        // Crosshair
        Fill(new Rect(w / 2 - 1, h / 2 - 8, 2, 16), Color.white);
        Fill(new Rect(w / 2 - 8, h / 2 - 1, 16, 2), Color.white);

        // Health and dash bars, bottom left
        var hp = Game.PlayerHealth;
        Bar(new Rect(20, h - 50, 300, 22), hp.Current / hp.Max, Palette.HealthBar, $"HP {Mathf.CeilToInt(hp.Current)} / {Mathf.CeilToInt(hp.Max)}");
        if (motor != null) Bar(new Rect(20, h - 22, 300, 8), motor.DashReady01, Palette.DashBar, null);

        // Top right info
        GUI.Label(new Rect(w - 260, 15, 240, 24), $"Enemies: {GoblinEnemy.Active.Count}", smallText);
        if (Game.GodMode) GUI.Label(new Rect(w - 260, 40, 240, 24), "GOD MODE", smallText);

        if (messageTimeLeft > 0f)
            GUI.Label(new Rect(0, h * 0.2f, w, 50), message, bigText);

        if (hp.IsDead)
        {
            Fill(new Rect(0, 0, w, h), new Color(0f, 0f, 0f, 0.6f));
            GUI.Label(new Rect(0, h / 2 - 40, w, 50), "YOU DIED", bigText);
            GUI.Label(new Rect(0, h / 2 + 10, w, 30), "Press R to restart", smallText);
        }
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
