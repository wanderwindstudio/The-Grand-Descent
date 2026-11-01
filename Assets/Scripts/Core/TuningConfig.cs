using UnityEngine;

// Every gameplay number lives here. Edit the asset at Assets/Settings/TuningConfig.asset,
// or tweak live in play mode with the F1 debug panel.
[CreateAssetMenu(fileName = "TuningConfig", menuName = "Grand Descent/Tuning Config")]
public class TuningConfig : ScriptableObject
{
    [Header("Player - movement")]
    public float moveSpeed = 6f;
    [Tooltip("Degrees of turn per pixel of mouse movement.")]
    public float mouseSensitivity = 0.12f;
    public float pitchClamp = 85f;
    public float gravity = -20f;
    [Tooltip("Small downward speed that keeps the player glued to the floor.")]
    public float groundStickSpeed = -2f;
    public float playerHeight = 1.8f;
    public float playerRadius = 0.4f;
    public float eyeHeight = 1.6f;
    public float fieldOfView = 80f;

    [Header("Player - dash (Space)")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 1.5f;
    [Tooltip("Seconds of invulnerability when a dash starts.")]
    public float dashInvulnerability = 0.3f;

    [Header("Player - health")]
    public float playerMaxHealth = 100f;

    [Header("Magic missile (left mouse)")]
    public float missileDamage = 10f;
    [Tooltip("Shots per second while holding the mouse button.")]
    public float fireRate = 4f;
    public float missileSpeed = 30f;
    [Tooltip("Hit radius of the missile.")]
    public float missileRadius = 0.2f;
    public float missileVisualSize = 0.3f;
    public float missileLifetime = 2f;
    [Tooltip("How far in front of the camera the missile appears.")]
    public float missileSpawnOffset = 0.6f;
    [Tooltip("How many extra enemies a missile passes through.")]
    public int missilePierce = 0;
    public float missileGlowIntensity = 4f;
    public float missileLightRange = 4f;
    public float missileLightIntensity = 2f;

    [Header("Goblin intern")]
    public float goblinHealth = 30f;
    public float goblinMoveSpeed = 3.5f;
    public float goblinAcceleration = 20f;
    public float goblinDamage = 10f;
    [Tooltip("Distance at which a goblin stops and starts its wind-up.")]
    public float goblinAttackRange = 1.6f;
    [Tooltip("When the wind-up ends, the hit lands only if you are still this close.")]
    public float goblinHitRange = 2.1f;
    [Tooltip("Telegraph time before the hit lands. Your window to dash away.")]
    public float goblinWindUpTime = 0.6f;
    [Tooltip("Pause after an attack before it chases again.")]
    public float goblinAttackCooldown = 1.0f;
    [Tooltip("How much bigger the goblin swells during wind-up (0.3 = 30%).")]
    public float goblinWindUpSwell = 0.3f;
    public float goblinWindUpGlow = 3f;
    [Tooltip("Faint green self-glow so goblins stand out in the dark. 0 = off.")]
    public float goblinBaseGlow = 0.25f;
    public float goblinRepathInterval = 0.2f;
    public float goblinHeight = 1.5f;
    public float goblinWidth = 0.8f;
    public float enemyHitFlashTime = 0.08f;

    [Header("Behind-you audio cue")]
    public bool behindCueEnabled = true;
    public float behindCueVolume = 0.7f;
    public float behindCuePitchHz = 880f;
    public float behindCueDuration = 0.15f;
    public float behindCueMaxDistance = 25f;

    [Header("Waves")]
    [Tooltip("Goblins per wave. Add or remove entries to change the number of waves.")]
    public int[] waveEnemyCounts = { 10, 18, 30 };
    public float firstWaveDelay = 2f;
    public float timeBetweenWaves = 4f;
    [Tooltip("Seconds between each goblin appearing during a wave.")]
    public float spawnInterval = 0.3f;
    [Tooltip("Goblins never appear closer to you than this.")]
    public float minSpawnDistance = 8f;
    public float spawnScatter = 1.5f;
    [Tooltip("Milestone 2 only: after the last wave, the waves restart after this many seconds.")]
    public float floorClearedRestartDelay = 6f;

    [Header("XP and levels")]
    public float xpPerGoblin = 1f;
    [Tooltip("XP needed to go from level 1 to 2.")]
    public float xpFirstLevel = 5f;
    [Tooltip("Each level needs this much more XP than the one before.")]
    public float xpPerLevelIncrease = 3f;
    public int upgradeChoices = 3;

    [Header("Pickups (XP orbs, later gold)")]
    public float pickupMagnetRadius = 4f;
    public float pickupMagnetSpeed = 14f;
    public float pickupCollectRadius = 0.8f;
    public float xpOrbSize = 0.25f;
    public float xpOrbGlow = 3f;
    public float pickupHeight = 0.5f;

    [Header("Upgrades")]
    [Tooltip("Split Shot: angle between missiles in the fan.")]
    public float splitShotSpreadDegrees = 6f;
    [Tooltip("Quick Hands: fire rate bonus per stack (0.2 = +20%).")]
    public float quickHandsBonus = 0.2f;
    [Tooltip("Arcane Weight: damage bonus per stack (0.25 = +25%).")]
    public float arcaneWeightBonus = 0.25f;
    [Tooltip("Piercing: extra enemies passed through per stack.")]
    public int piercingPerStack = 1;
    [Tooltip("Second Wind: max health added per stack.")]
    public float secondWindHealth = 25f;

    [Header("Upgrade - Fire Wisp")]
    public float wispDamage = 8f;
    [Tooltip("A wisp can hit the same goblin once per this many seconds.")]
    public float wispHitCooldown = 0.4f;
    public float wispOrbitRadius = 2f;
    [Tooltip("Degrees per second.")]
    public float wispOrbitSpeed = 200f;
    public float wispHeight = 1f;
    public float wispHitRadius = 0.5f;
    public float wispSize = 0.4f;

    [Header("Upgrade - Poison Trail")]
    [Tooltip("Drop a new cloud every time you move this far.")]
    public float poisonDropDistance = 1.2f;
    public float poisonCloudDuration = 3f;
    public float poisonCloudRadius = 1.3f;
    [Tooltip("Per stack.")]
    public float poisonDamagePerSecond = 6f;
    public float poisonTickInterval = 0.5f;

    [Header("Hurt feedback")]
    public float damageFlashDuration = 0.35f;
    [Range(0f, 1f)] public float damageFlashMaxAlpha = 0.35f;
    public float hitIndicatorDuration = 1.2f;
    [Tooltip("Distance of the hit arrows from the screen centre, in pixels.")]
    public float hitIndicatorRadius = 160f;
    public float hitIndicatorSize = 48f;

    [Header("Level - room")]
    public float roomSize = 30f;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;
    public float pillarSize = 1.5f;

    [Header("Level - lighting")]
    [Tooltip("Colour of the light that fills every shadow.")]
    public Color ambientColor = new Color(0.85f, 0.85f, 1f);
    [Tooltip("Overall brightness of the shadows. 0.08 = pitch-dark dungeon, 0.3 = readable.")]
    public float ambientIntensity = 0.3f;
    public Color fogColor = new Color(0.06f, 0.06f, 0.08f);
    [Tooltip("Higher = things vanish into the dark sooner.")]
    public float fogDensity = 0.015f;
    public Color torchColor = new Color(1f, 0.6f, 0.3f);
    public float torchIntensity = 4f;
    public float torchRange = 16f;
    public float torchHeight = 2.6f;

    [Header("Post-processing")]
    public float bloomIntensity = 1.2f;
    public float bloomThreshold = 0.9f;
    public float vignetteIntensity = 0.2f;
}
