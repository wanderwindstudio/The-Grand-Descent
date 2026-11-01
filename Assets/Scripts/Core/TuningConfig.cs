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

    [Header("Goblin sprite (billboard visuals)")]
    [Tooltip("Off = the old green capsules. Also switchable in the F1 panel.")]
    public bool useSpriteEnemies = true;
    [Tooltip("How tall the drawn goblin is in the world, in metres.")]
    public float goblinSpriteHeight = 1.2f;
    [Tooltip("Fraction of the image height the goblin fills (1 = top to bottom; the art spec targets 0.85).")]
    [Range(0.1f, 1f)] public float goblinSpriteFillFraction = 1f;
    [Tooltip("Hop height while walking, in metres.")]
    public float spriteBobHeight = 0.06f;
    [Tooltip("Steps per second while walking.")]
    public float spriteBobSpeed = 3f;
    [Tooltip("Side-to-side lean while walking, in degrees.")]
    public float spriteSwayDegrees = 4f;
    [Tooltip("How much the sprite stretches up during the attack wind-up (0.15 = 15%).")]
    public float spriteWindUpStretch = 0.15f;
    [Tooltip("How far the sprite lunges toward you at the end of the wind-up, in metres.")]
    public float spriteLungeDistance = 0.35f;
    [Tooltip("Red glow on the sprite during the wind-up, for readability. 0 = off.")]
    public float spriteWindUpGlow = 0.25f;
    public float spriteHitFlashStrength = 1.5f;
    public float corpseDuration = 0.7f;
    public float corpseTipDegrees = 85f;

    [Header("Behind-you warning (goblin winding up off-screen)")]
    public bool behindCueEnabled = true;
    public float behindCueVolume = 0.8f;
    [Tooltip("Pitch of the first blip; the second is higher.")]
    public float behindCuePitchHz = 700f;
    public float behindCueMaxDistance = 25f;
    [Tooltip("The cue gets higher-pitched the closer the goblin is (0.3 = up to 30% higher).")]
    public float behindCueClosePitchBoost = 0.3f;
    [Tooltip("Minimum seconds between cues, so 30 goblins don't make a wall of noise.")]
    public float behindCueMinInterval = 0.15f;
    [Tooltip("Amber arrows at the screen edge for off-screen goblins about to hit you.")]
    public bool offscreenWarningArrows = true;

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

    [Header("Level - floors")]
    [Tooltip("Size of one character in the floor layouts, in metres.")]
    public float cellSize = 2f;
    public float wallHeight = 4f;

    [Header("Descending - each floor multiplies these again")]
    public float floorEnemyHealthMultiplier = 1.25f;
    public float floorEnemyCountMultiplier = 1.2f;
    public float floorEnemyDamageMultiplier = 1.15f;
    public float floorGoldMultiplier = 1.3f;

    [Header("Collapse (floors where portals start a countdown)")]
    public int collapseStartFloor = 3;
    [Tooltip("Seconds to reach a portal on the first collapsing floor.")]
    public float collapseTime = 20f;
    [Tooltip("Each deeper floor gives this many fewer seconds.")]
    public float collapseTimeReductionPerFloor = 2f;
    public float collapseTimeMin = 10f;
    [Tooltip("Last seconds where the screen pulses red.")]
    public float collapseWarningTime = 5f;
    [Tooltip("Camera shake at the very end, in metres.")]
    public float collapseShakeMax = 0.08f;
    public float collapseRumbleVolume = 0.6f;

    [Header("Portals")]
    public float portalRadius = 1.5f;
    [Tooltip("Distance of each portal from the portal spot.")]
    public float portalSpacing = 3f;

    [Header("Gold")]
    [Range(0f, 1f)] public float goldDropChance = 0.35f;
    [Tooltip("Gold per coin on floor 1.")]
    public float goldPerDrop = 1f;
    public float goldCoinSize = 0.4f;
    [Tooltip("When you die, this fraction of carried gold is banked anyway.")]
    [Range(0f, 1f)] public float deathBankFraction = 0.25f;

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

    [Header("Buzz - meter")]
    public float buzzMax = 100f;
    public float buzzDrainPerSecond = 2f;
    [Tooltip("Damage bonus at full Buzz (1 = +100%). Scales linearly from 0.")]
    public float buzzMaxDamageBonus = 1f;
    [Tooltip("Above this fraction of Buzz (0.7 = 70%) spell swaps start, and time is counted for the run stats.")]
    [Range(0f, 1f)] public float buzzHighThreshold = 0.7f;

    [Header("Buzz - pipe shrines and pickups")]
    public int shrinesMin = 1;
    public int shrinesMax = 2;
    public float shrineBuzzAmount = 40f;
    public float shrineInteractRange = 2.5f;
    [Tooltip("Seconds until a used shrine works again.")]
    public float shrineRechargeTime = 60f;
    [Range(0f, 1f)] public float buzzPickupDropChance = 0.12f;
    public float buzzPickupAmount = 5f;

    [Header("Buzz - missile wobble")]
    [Tooltip("Sideways drift of missiles at full Buzz, in metres.")]
    public float wobbleMaxAmplitude = 1.2f;
    [Tooltip("Wobbles per second.")]
    public float wobbleFrequency = 2.5f;
    [Tooltip("Seconds for the wobble to grow to full size after firing, so shots still leave from the crosshair.")]
    public float wobbleRampTime = 0.25f;

    [Header("Buzz - spell swap (above the high threshold)")]
    public float spellSwapMinInterval = 3f;
    public float spellSwapMaxInterval = 6f;
    [Tooltip("How long the 'next shot' warning shows before the swap takes effect.")]
    public float spellSwapWarning = 1f;
    public float hugeOrbDamageMultiplier = 4f;
    public float hugeOrbSpeedMultiplier = 0.35f;
    public float hugeOrbSize = 1.2f;
    public float hugeOrbHitRadius = 0.8f;
    public int hugeOrbExtraPierce = 5;
    public int sparkCount = 8;
    public float sparkSpreadDegrees = 25f;
    public float sparkDamageMultiplier = 0.3f;
    public float sparkSpeedMultiplier = 1.4f;
    public float sparkSize = 0.12f;

    [Header("Buzz - Transcendence (at 100%) and Crash")]
    public float transcendenceDuration = 10f;
    public float transcendenceDamageMultiplier = 3f;
    public float transcendenceFireRateMultiplier = 2f;
    public float crashDuration = 6f;
    public float crashMoveSpeedMultiplier = 0.5f;

    [Header("Buzz - screen effects")]
    [Range(0f, 1f)] public float buzzMaxChromaticAberration = 1f;
    [Tooltip("Lens warp at full Buzz (0 to 1).")]
    public float buzzMaxLensDistortion = 0.45f;
    [Tooltip("Degrees the colours swing back and forth at full Buzz.")]
    public float buzzMaxHueShift = 40f;
    public float buzzSaturationBoost = 30f;
    [Tooltip("Speed of the gentle pulsing.")]
    public float buzzPulseSpeed = 2f;
    public float transcendenceExposure = 0.6f;
    public float transcendenceBloom = 3f;
    public float crashExposure = -1.2f;
    public float crashSaturation = -60f;
    [Tooltip("Blur strength during the Crash (0.5 to 1.5).")]
    public float crashBlur = 1.5f;
    public float crashVignette = 0.55f;

    [Header("Post-processing")]
    public float bloomIntensity = 1.2f;
    public float bloomThreshold = 0.9f;
    public float vignetteIntensity = 0.2f;
}
