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

    [Header("Test spawner (milestone 1 only)")]
    public int testGoblinCount = 5;
    public float testRespawnDelay = 3f;
    public float spawnScatter = 1.5f;

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
    public Color ambientColor = new Color(0.07f, 0.07f, 0.1f);
    public Color fogColor = new Color(0.02f, 0.02f, 0.03f);
    public float fogDensity = 0.04f;
    public Color torchColor = new Color(1f, 0.6f, 0.3f);
    public float torchIntensity = 3f;
    public float torchRange = 12f;
    public float torchHeight = 2.6f;

    [Header("Post-processing")]
    public float bloomIntensity = 1.2f;
    public float bloomThreshold = 0.9f;
    public float vignetteIntensity = 0.3f;
}
