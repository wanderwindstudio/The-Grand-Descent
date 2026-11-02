using UnityEngine;

// The only thing in the scene. Builds everything else at runtime.
public class GameBootstrap : MonoBehaviour
{
    [Tooltip("Drag Assets/Settings/TuningConfig here. If empty, default values are used.")]
    public TuningConfig config;
    [Tooltip("Assets/Settings/GrayboxLit. Every coloured material is copied from it.")]
    public Material baseMaterial;
    [Tooltip("Assets/Art/Materials/mat_billboardCutout. Lit, alpha-cutout material for sprites.")]
    public Material billboardMaterial;
    [Tooltip("Assets/Art/Enemies/enemy_goblinIntern_idle_01")]
    public Texture2D goblinSprite;
    [Tooltip("Assets/Art/Enemies/enemy_goblinIntern_windup_01 (optional)")]
    public Texture2D goblinWindUpSprite;
    [Tooltip("Assets/Art/Enemies/enemy_goblinIntern_hit_01 (optional)")]
    public Texture2D goblinHitSprite;
    [Tooltip("Assets/Art/Enemies/enemy_goblinIntern_dead_01 (optional)")]
    public Texture2D goblinDeadSprite;
    [Tooltip("Assets/Art/Materials/mat_fxTransparent. Glowing, fading material for spell effects.")]
    public Material fxMaterial;
    [Tooltip("Assets/Settings/GrandDescentControls. If empty, default bindings are used.")]
    public UnityEngine.InputSystem.InputActionAsset controls;
    [Header("Player and spell art (optional; coloured placeholders are used if missing)")]
    public Texture2D handLeft;
    [Tooltip("Assets/Art/Player/player_handLeft_cast_01 (optional). Shown briefly on every spell cast.")]
    public Texture2D handLeftCast;
    public Texture2D handRight;
    public Texture2D fxArcaneMissile;
    public Texture2D fxArcaneImpact;
    public Texture2D fxArcaneVortex;

    void Awake()
    {
        Time.timeScale = 1f;
        GoblinEnemy.Active.Clear();
        RunStats.Reset();
        Game.Floor = 1;
        Game.GodMode = false;
        Game.InputBlocked = false;

        Mats.Template = baseMaterial;
        Art.BillboardMaterial = billboardMaterial;
        Art.GoblinIdle = goblinSprite;
        Art.GoblinWindUp = goblinWindUpSprite != null ? goblinWindUpSprite : goblinSprite;
        Art.GoblinHit = goblinHitSprite != null ? goblinHitSprite : goblinSprite;
        Art.GoblinDead = goblinDeadSprite; // null = tip the idle sprite over instead
        Art.FxMaterial = fxMaterial;
        Art.HandLeft = handLeft != null ? handLeft : Art.Placeholder("HandLeftPlaceholder", Palette.HandPlaceholder, square: true);
        Art.HandLeftCast = handLeftCast;
        Art.HandRight = handRight != null ? handRight : Art.Placeholder("HandRightPlaceholder", Palette.HandPlaceholder, square: true);
        Art.ArcaneMissile = fxArcaneMissile != null ? fxArcaneMissile : Art.Placeholder("MissilePlaceholder", Palette.Arcane, square: false);
        Art.ArcaneImpact = fxArcaneImpact != null ? fxArcaneImpact : Art.Placeholder("ImpactPlaceholder", Palette.Arcane, square: false);
        Art.ArcaneVortex = fxArcaneVortex != null ? fxArcaneVortex : Art.Placeholder("VortexPlaceholder", Palette.Arcane, square: false);
        Controls.Init(controls);
        // Work on a copy so live slider changes don't silently overwrite the asset.
        Game.TuningAsset = config;
        Game.Tuning = config != null ? Instantiate(config) : ScriptableObject.CreateInstance<TuningConfig>();

        var run = gameObject.AddComponent<RunManager>();
        LevelLayout level = run.BuildFloor();
        PlayerBuilder.Create(level.playerStart, level.playerStartYaw, Game.Tuning);
        PostProcessSetup.Create(Game.Tuning, Game.Camera);

        gameObject.AddComponent<SpellFx>();
        gameObject.AddComponent<FirstPersonHands>();
        gameObject.AddComponent<LootDropper>();
        gameObject.AddComponent<GoblinCorpses>();
        var waves = gameObject.AddComponent<WaveSpawner>();
        gameObject.AddComponent<Hud>();
        gameObject.AddComponent<LevelUpScreen>();
        gameObject.AddComponent<BuzzEffects>();
        var collapse = gameObject.AddComponent<CollapseTimer>();
        gameObject.AddComponent<RunScreens>();
        gameObject.AddComponent<DebugPanel>();
        run.Init(waves, collapse);
    }
}
