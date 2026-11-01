using UnityEngine;
using UnityEngine.InputSystem;

// F1 = tuning sliders. F2 = fill Buzz, F3 = level up, F4 = kill all, F5 = god mode.
public class DebugPanel : MonoBehaviour
{
    bool open;
    Vector2 scroll;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f1Key.wasPressedThisFrame) Toggle();
        if (kb.f2Key.wasPressedThisFrame) Hud.ShowMessage("Buzz arrives in milestone 3");
        if (kb.f3Key.wasPressedThisFrame) Hud.ShowMessage("Level-ups arrive in milestone 2");
        if (kb.f4Key.wasPressedThisFrame) KillAll();
        if (kb.f5Key.wasPressedThisFrame)
        {
            Game.GodMode = !Game.GodMode;
            Hud.ShowMessage(Game.GodMode ? "God mode ON" : "God mode OFF");
        }
    }

    void Toggle()
    {
        open = !open;
        Game.InputBlocked = open;
        if (open) PlayerLook.UnlockCursor();
        else if (!Game.PlayerHealth.IsDead) PlayerLook.LockCursor();
    }

    static void KillAll()
    {
        var all = GoblinEnemy.Active.ToArray();
        foreach (var g in all) g.Die();
        Hud.ShowMessage($"Killed {all.Length} enemies");
    }

    void OnGUI()
    {
        if (!open) return;
        GUILayout.BeginArea(new Rect(10, 10, 440, Screen.height - 20), GUI.skin.box);
        GUILayout.Label("DEBUG PANEL (F1 to close)  -  F4 kill all  -  F5 god mode");
        scroll = GUILayout.BeginScrollView(scroll);

        var t = Game.Tuning;
        GUILayout.Label("Player");
        t.moveSpeed = Slider("Move speed", t.moveSpeed, 1f, 15f);
        t.mouseSensitivity = Slider("Mouse sensitivity", t.mouseSensitivity, 0.02f, 0.5f);
        t.dashSpeed = Slider("Dash speed", t.dashSpeed, 5f, 40f);
        t.dashCooldown = Slider("Dash cooldown", t.dashCooldown, 0.2f, 5f);
        t.dashInvulnerability = Slider("Dash invulnerability", t.dashInvulnerability, 0f, 1f);

        GUILayout.Label("Missile");
        t.missileDamage = Slider("Damage", t.missileDamage, 1f, 50f);
        t.fireRate = Slider("Fire rate (/s)", t.fireRate, 0.5f, 20f);
        t.missileSpeed = Slider("Speed", t.missileSpeed, 5f, 80f);
        t.missileRadius = Slider("Hit radius", t.missileRadius, 0.05f, 1f);
        t.missilePierce = IntSlider("Pierce", t.missilePierce, 0, 5);

        GUILayout.Label("Goblin");
        t.goblinHealth = Slider("Health (new spawns)", t.goblinHealth, 5f, 200f);
        t.goblinMoveSpeed = Slider("Move speed", t.goblinMoveSpeed, 0.5f, 10f);
        t.goblinDamage = Slider("Damage", t.goblinDamage, 1f, 50f);
        t.goblinWindUpTime = Slider("Wind-up time", t.goblinWindUpTime, 0.1f, 2f);
        t.goblinAttackCooldown = Slider("Attack cooldown", t.goblinAttackCooldown, 0.1f, 4f);
        t.goblinAttackRange = Slider("Attack range", t.goblinAttackRange, 0.8f, 4f);
        t.goblinHitRange = Slider("Hit range", t.goblinHitRange, 0.8f, 5f);
        t.testGoblinCount = IntSlider("Goblins per batch", t.testGoblinCount, 1, 30);

        GUILayout.Label("Feedback");
        t.hitIndicatorDuration = Slider("Hit arrow duration", t.hitIndicatorDuration, 0.2f, 4f);
        t.hitIndicatorRadius = Slider("Hit arrow distance", t.hitIndicatorRadius, 40f, 400f);
        t.behindCueVolume = Slider("Behind-you beep volume", t.behindCueVolume, 0f, 1f);
        t.behindCueEnabled = GUILayout.Toggle(t.behindCueEnabled, "Behind-you beep on");

        GUILayout.Label("Atmosphere");
        t.ambientIntensity = Slider("Ambient brightness", t.ambientIntensity, 0f, 1f);
        t.fogDensity = Slider("Fog density", t.fogDensity, 0f, 0.15f);
        t.torchIntensity = Slider("Torch brightness", t.torchIntensity, 0f, 10f);
        t.torchRange = Slider("Torch reach", t.torchRange, 2f, 40f);
        t.goblinBaseGlow = Slider("Goblin glow (new spawns)", t.goblinBaseGlow, 0f, 2f);
        LevelLighting.Refresh(t);

        GUILayout.Space(10);
        if (GUILayout.Button("Reset to asset values")) ResetToAsset();
#if UNITY_EDITOR
        if (GUILayout.Button("Save current values into TuningConfig asset")) SaveToAsset();
#endif
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    static float Slider(string label, float value, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {value:0.###}", GUILayout.Width(210));
        value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(190));
        GUILayout.EndHorizontal();
        return value;
    }

    static int IntSlider(string label, int value, int min, int max) =>
        Mathf.RoundToInt(Slider(label, value, min, max));

    static void ResetToAsset()
    {
        if (Game.TuningAsset == null) return;
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Game.TuningAsset), Game.Tuning);
        Hud.ShowMessage("Tuning reset to asset");
    }

#if UNITY_EDITOR
    static void SaveToAsset()
    {
        if (Game.TuningAsset == null) return;
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Game.Tuning), Game.TuningAsset);
        UnityEditor.EditorUtility.SetDirty(Game.TuningAsset);
        UnityEditor.AssetDatabase.SaveAssets();
        Hud.ShowMessage("Saved to TuningConfig asset");
    }
#endif
}
