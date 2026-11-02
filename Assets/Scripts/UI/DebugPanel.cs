using UnityEngine;
using UnityEngine.InputSystem;

// F1 = tuning sliders. F2 = fill Buzz, F3 = level up, F4 = kill all, F5 = god mode, F7 = reset cooldowns,
// F8 = rebuild the floor as the next single room prefab (room tester).
public class DebugPanel : MonoBehaviour
{
    bool open;
    Vector2 scroll;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (PauseMenu.IsOpen) return;
        if (kb.f1Key.wasPressedThisFrame) Toggle();
        if (kb.f2Key.wasPressedThisFrame) Game.Buzz.Fill();
        if (kb.f3Key.wasPressedThisFrame) Game.Player.GetComponent<PlayerLevel>().GainLevel();
        if (kb.f4Key.wasPressedThisFrame) KillAll();
        if (kb.f7Key.wasPressedThisFrame) ResetCooldowns();
        if (kb.f8Key.wasPressedThisFrame) RunManager.Instance.DebugNextTestRoom();
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

    static void ResetCooldowns()
    {
        Game.Player.GetComponent<PlayerMotor>().ResetCooldown();
        Game.Player.GetComponent<VortexCaster>().ResetCooldown();
        Hud.ShowMessage("Cooldowns reset");
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
        GUILayout.Label("DEBUG PANEL (F1 to close)  -  F2 fill Buzz  -  F3 level up  -  F4 kill all  -  F5 god mode  -  F7 reset cooldowns  -  F8 next test room");
        scroll = GUILayout.BeginScrollView(scroll);

        var t = Game.Tuning;
        GUILayout.Label("Player");
        t.moveSpeed = Slider("Move speed", t.moveSpeed, 1f, 15f);
        t.mouseSensitivity = Slider("Mouse sensitivity", t.mouseSensitivity, 0.02f, 0.5f);
        t.dashSpeed = Slider("Dash speed", t.dashSpeed, 5f, 40f);
        t.dashCooldown = Slider("Dash cooldown", t.dashCooldown, 0.2f, 5f);
        t.dashInvulnerability = Slider("Dash invulnerability", t.dashInvulnerability, 0f, 1f);

        GUILayout.Label("Jumping");
        t.jumpHeight = Slider("Jump height (m)", t.jumpHeight, 0.3f, 4f);
        t.fallGravityMultiplier = Slider("Fall gravity multiplier", t.fallGravityMultiplier, 1f, 4f);
        t.airControl = Slider("Air control", t.airControl, 0f, 1f);
        t.jumpCutMultiplier = Slider("Early release cut", t.jumpCutMultiplier, 0f, 1f);
        t.coyoteTime = Slider("Coyote time (s)", t.coyoteTime, 0f, 0.4f);
        t.jumpBufferTime = Slider("Jump buffer (s)", t.jumpBufferTime, 0f, 0.4f);

        GUILayout.Label("Goblin traversal");
        t.goblinLedgeJumpHeight = Slider("Ledge-jump height (m, next floor/F8)", t.goblinLedgeJumpHeight, 0f, 8f);
        t.goblinLadderSpeedFraction = Slider("Ladder speed fraction", t.goblinLadderSpeedFraction, 0.1f, 1f);
        t.goblinHopDelay = Slider("Wall hop delay (s)", t.goblinHopDelay, 0f, 1.5f);
        t.goblinLandingStun = Slider("Landing stun (s)", t.goblinLandingStun, 0f, 2f);
        t.campTime = Slider("Camping time before sprayers (s)", t.campTime, 2f, 30f);

        GUILayout.Label("Arcane Missile");
        t.missileDamage = Slider("Damage", t.missileDamage, 1f, 50f);
        t.fireRate = Slider("Fire rate (/s)", t.fireRate, 0.5f, 20f);
        t.missileSpeed = Slider("Speed", t.missileSpeed, 5f, 80f);
        t.missileRadius = Slider("Hit radius", t.missileRadius, 0.05f, 1f);
        t.arcaneMissileLength = Slider("Sprite length (m)", t.arcaneMissileLength, 0.1f, 2f);
        t.chargeWindow = Slider("Charge window (s)", t.chargeWindow, 0.2f, 3f);
        t.maxCharges = IntSlider("Max charges", t.maxCharges, 1, 8);
        t.chargeDamageBonus = Slider("Damage per charge", t.chargeDamageBonus, 0f, 1f);
        t.chargeFireRateBonus = Slider("Fire rate per charge", t.chargeFireRateBonus, 0f, 0.5f);

        GUILayout.Label("Arcane Vortex");
        t.vortexRadius = Slider("Radius (m)", t.vortexRadius, 1f, 15f);
        t.vortexDuration = Slider("Duration (s)", t.vortexDuration, 0.5f, 10f);
        t.vortexCooldown = Slider("Cooldown (s)", t.vortexCooldown, 0.5f, 30f);
        t.vortexPullSpeed = Slider("Pull speed", t.vortexPullSpeed, 1f, 30f);
        t.vortexTickDamage = Slider("Tick damage", t.vortexTickDamage, 0f, 30f);
        t.vortexOpacity = Slider("Opacity", t.vortexOpacity, 0.1f, 1f);

        GUILayout.Label("Hands");
        t.showHands = GUILayout.Toggle(t.showHands, "Show hands");
        t.handScale = Slider("Hand scale", t.handScale, 0.5f, 2f);
        t.handCenterGap = Slider("Horizontal offset from centre (screen width)", t.handCenterGap, t.handMinCenterGap, 0.45f);
        t.handVerticalOffset = Slider("Vertical offset (lower only)", t.handVerticalOffset, -0.25f, 0f);
        t.handTiltLeft = Slider("Left hand tilt", t.handTiltLeft, -45f, 15f);
        t.handTiltRight = Slider("Right hand tilt", t.handTiltRight, -15f, 45f);
        t.handBobAmount = Slider("Bob amount", t.handBobAmount, 0f, 0.08f);
        t.handSwayAmount = Slider("Sway amount", t.handSwayAmount, 0f, 0.04f);
        t.handLookLag = Slider("Turn lag", t.handLookLag, 0f, 0.015f);
        t.handCastPoseTime = Slider("Cast pose time", t.handCastPoseTime, 0f, 0.5f);

        GUILayout.Label("Goblin visuals");
        if (!Art.HasGoblinSprite) GUILayout.Label("(sprite or material missing - run Grand Descent > Create Prototype Scene)");
        if (GUILayout.Button(t.useSpriteEnemies ? "Showing: SPRITE  (click for capsule)" : "Showing: CAPSULE  (click for sprite)"))
            t.useSpriteEnemies = !t.useSpriteEnemies;
        t.goblinSpriteHeight = Slider("Sprite height (m)", t.goblinSpriteHeight, 0.5f, 3f);
        t.spriteBobHeight = Slider("Walk hop height", t.spriteBobHeight, 0f, 0.3f);
        t.spriteBobSpeed = Slider("Walk steps/s", t.spriteBobSpeed, 0.5f, 8f);
        t.spriteSwayDegrees = Slider("Walk lean", t.spriteSwayDegrees, 0f, 15f);
        t.spriteWindUpStretch = Slider("Wind-up stretch", t.spriteWindUpStretch, 0f, 0.5f);
        t.spriteLungeDistance = Slider("Wind-up lunge (m)", t.spriteLungeDistance, 0f, 1.5f);
        t.spriteWindUpGlow = Slider("Wind-up red glow", t.spriteWindUpGlow, 0f, 3f);

        GUILayout.Label("Goblin");
        t.goblinHealth = Slider("Health (new spawns)", t.goblinHealth, 5f, 200f);
        t.goblinMoveSpeed = Slider("Move speed", t.goblinMoveSpeed, 0.5f, 10f);
        t.goblinDamage = Slider("Damage", t.goblinDamage, 1f, 50f);
        t.goblinWindUpTime = Slider("Wind-up time", t.goblinWindUpTime, 0.1f, 2f);
        t.goblinAttackCooldown = Slider("Attack cooldown", t.goblinAttackCooldown, 0.1f, 4f);
        t.goblinAttackRange = Slider("Attack range", t.goblinAttackRange, 0.8f, 4f);
        t.goblinHitRange = Slider("Hit range", t.goblinHitRange, 0.8f, 5f);

        GUILayout.Label("Waves (counts apply from the next wave)");
        for (int i = 0; i < t.waveEnemyCounts.Length; i++)
            t.waveEnemyCounts[i] = IntSlider($"Wave {i + 1} goblins", t.waveEnemyCounts[i], 1, 60);
        t.spawnInterval = Slider("Spawn interval", t.spawnInterval, 0.05f, 2f);
        t.timeBetweenWaves = Slider("Time between waves", t.timeBetweenWaves, 0f, 15f);

        GUILayout.Label($"Floors and gold (now on floor {Game.Floor})");
        t.floorEnemyHealthMultiplier = Slider("Enemy health x per floor", t.floorEnemyHealthMultiplier, 1f, 3f);
        t.floorEnemyCountMultiplier = Slider("Enemy count x per floor", t.floorEnemyCountMultiplier, 1f, 3f);
        t.floorEnemyDamageMultiplier = Slider("Enemy damage x per floor", t.floorEnemyDamageMultiplier, 1f, 3f);
        t.floorGoldMultiplier = Slider("Gold x per floor", t.floorGoldMultiplier, 1f, 3f);
        t.goldDropChance = Slider("Gold drop chance", t.goldDropChance, 0f, 1f);
        t.goldPerDrop = Slider("Gold per coin (floor 1)", t.goldPerDrop, 1f, 20f);
        t.deathBankFraction = Slider("Banked on death", t.deathBankFraction, 0f, 1f);
        t.collapseStartFloor = IntSlider("Collapse from floor", t.collapseStartFloor, 1, 10);
        t.collapseTime = Slider("Collapse time", t.collapseTime, 3f, 60f);
        t.collapseTimeReductionPerFloor = Slider("Collapse time -/floor", t.collapseTimeReductionPerFloor, 0f, 10f);
        t.collapseTimeMin = Slider("Collapse time minimum", t.collapseTimeMin, 1f, 60f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Descend now")) RunManager.Instance.DebugDescend();
        if (GUILayout.Button("Open portals now")) RunManager.Instance.ForceOpenPortals();
        if (GUILayout.Button("Reset banked gold + runs")) { MetaProgress.ResetAll(); Hud.ShowMessage("Meta progress reset"); }
        GUILayout.EndHorizontal();
        GUILayout.Label("Run log: " + RunLog.FilePath);

        GUILayout.Label("XP");
        t.xpPerGoblin = Slider("XP per goblin", t.xpPerGoblin, 0.1f, 5f);
        t.xpFirstLevel = Slider("XP for first level", t.xpFirstLevel, 1f, 30f);
        t.xpPerLevelIncrease = Slider("XP increase per level", t.xpPerLevelIncrease, 0f, 20f);
        t.pickupMagnetRadius = Slider("Pickup magnet radius", t.pickupMagnetRadius, 0.5f, 15f);

        GUILayout.Label("Upgrades");
        t.splitShotSpreadDegrees = Slider("Splitting Missiles spread", t.splitShotSpreadDegrees, 0f, 30f);
        t.quickHandsBonus = Slider("Quick Hands bonus", t.quickHandsBonus, 0f, 1f);
        t.arcaneWeightBonus = Slider("Arcane Weight bonus", t.arcaneWeightBonus, 0f, 1f);
        t.wispDamage = Slider("Wisp damage", t.wispDamage, 1f, 50f);
        t.wispOrbitRadius = Slider("Wisp orbit radius", t.wispOrbitRadius, 0.5f, 6f);
        t.wispOrbitSpeed = Slider("Wisp orbit speed", t.wispOrbitSpeed, 30f, 720f);
        t.poisonDamagePerSecond = Slider("Poison damage/s", t.poisonDamagePerSecond, 0f, 40f);
        t.poisonCloudRadius = Slider("Poison radius", t.poisonCloudRadius, 0.3f, 4f);
        t.poisonCloudDuration = Slider("Poison duration", t.poisonCloudDuration, 0.5f, 10f);
        GUILayout.Label("Give upgrade:");
        var upgrades = Game.Player.GetComponent<PlayerUpgrades>();
        GUILayout.BeginHorizontal();
        for (int i = 0; i < PlayerUpgrades.All.Length; i++)
        {
            if (i > 0 && i % 3 == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
            var u = PlayerUpgrades.All[i];
            if (GUILayout.Button($"{PlayerUpgrades.Name(u)} ({upgrades.Stacks(u)})")) upgrades.Apply(u);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Buzz");
        float buzzNow = Slider("Current Buzz", Game.Buzz.Value, 0f, t.buzzMax);
        if (Mathf.Abs(buzzNow - Game.Buzz.Value) > 0.5f) Game.Buzz.SetValue(buzzNow);
        t.buzzDrainPerSecond = Slider("Drain per second", t.buzzDrainPerSecond, 0f, 15f);
        t.shrineBuzzAmount = Slider("Shrine Buzz", t.shrineBuzzAmount, 5f, 100f);
        t.buzzPickupDropChance = Slider("Buzz drop chance", t.buzzPickupDropChance, 0f, 1f);
        t.buzzPickupAmount = Slider("Buzz per pickup", t.buzzPickupAmount, 1f, 30f);
        t.buzzMaxDamageBonus = Slider("Damage bonus at max", t.buzzMaxDamageBonus, 0f, 3f);
        t.wobbleMaxAmplitude = Slider("Wobble at max (m)", t.wobbleMaxAmplitude, 0f, 4f);
        t.wobbleFrequency = Slider("Wobble speed", t.wobbleFrequency, 0.2f, 8f);
        t.buzzHighThreshold = Slider("Spell swap threshold", t.buzzHighThreshold, 0f, 1f);
        t.spellSwapMinInterval = Slider("Swap min interval", t.spellSwapMinInterval, 0.5f, 15f);
        t.spellSwapMaxInterval = Slider("Swap max interval", t.spellSwapMaxInterval, 0.5f, 20f);
        t.spellSwapWarning = Slider("Swap warning time", t.spellSwapWarning, 0.2f, 3f);
        t.transcendenceDuration = Slider("Transcendence time", t.transcendenceDuration, 1f, 30f);
        t.transcendenceDamageMultiplier = Slider("Transcendence damage x", t.transcendenceDamageMultiplier, 1f, 10f);
        t.crashDuration = Slider("Crash time", t.crashDuration, 0f, 20f);
        t.crashMoveSpeedMultiplier = Slider("Crash move speed x", t.crashMoveSpeedMultiplier, 0.1f, 1f);
        t.buzzMaxChromaticAberration = Slider("Colour fringe at max", t.buzzMaxChromaticAberration, 0f, 1f);
        t.buzzMaxLensDistortion = Slider("Lens warp at max", t.buzzMaxLensDistortion, 0f, 1f);
        t.buzzMaxHueShift = Slider("Hue swing at max", t.buzzMaxHueShift, 0f, 180f);

        GUILayout.Label("Feedback");
        t.hitIndicatorDuration = Slider("Hit arrow duration", t.hitIndicatorDuration, 0.2f, 4f);
        t.hitIndicatorRadius = Slider("Hit arrow distance", t.hitIndicatorRadius, 40f, 400f);
        t.behindCueVolume = Slider("Behind-you beep volume", t.behindCueVolume, 0f, 1f);
        t.behindCuePitchHz = Slider("Behind-you beep pitch", t.behindCuePitchHz, 200f, 2000f);
        t.behindCueMinInterval = Slider("Beep min interval", t.behindCueMinInterval, 0f, 1f);
        t.behindCueEnabled = GUILayout.Toggle(t.behindCueEnabled, "Behind-you beep on");
        t.offscreenWarningArrows = GUILayout.Toggle(t.offscreenWarningArrows, "Off-screen attack arrows on");

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
