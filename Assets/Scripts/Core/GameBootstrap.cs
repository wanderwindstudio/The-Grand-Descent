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
    [Tooltip("Assets/Rooms/RoomLibrary. Kept up to date automatically; lists every room prefab.")]
    public RoomLibrary roomLibrary;
    [Tooltip("Assets/Settings/UiArt. Sprites and fonts for the pause menu and settings.")]
    public UiArt uiArt;
    [Tooltip("Assets/Settings/EnvironmentArt. Dungeon textures, props and decals (gray-box without it).")]
    public EnvironmentArt environmentArt;
    [Header("Player and spell art (optional; coloured placeholders are used if missing)")]
    public Texture2D handLeft;
    [Tooltip("Assets/Art/Player/player_handLeft_cast_01 (optional). Shown briefly on every spell cast.")]
    public Texture2D handLeftCast;
    public Texture2D handRight;
    [Tooltip("Assets/Art/Player/player_handRight_staff_slam_01 (optional). The staff pose at max Arcane Charges.")]
    public Texture2D handRightSlam;
    public Texture2D fxArcaneMissile;
    public Texture2D fxArcaneImpact;
    public Texture2D fxArcaneVortex;
    [Header("UI icons (optional; coloured placeholders are used if missing)")]
    public Texture2D iconArcaneMissile;
    public Texture2D iconArcaneVortex;
    public Texture2D iconArcaneExplosion;
    public Texture2D iconArcaneCharge;

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
        Art.HandRightSlam = handRightSlam;
        Art.IconMissile = iconArcaneMissile != null ? iconArcaneMissile : Art.Placeholder("MissileIconPlaceholder", Palette.Arcane, square: false);
        Art.IconVortex = iconArcaneVortex != null ? iconArcaneVortex : Art.Placeholder("VortexIconPlaceholder", Palette.Arcane, square: false);
        Art.IconExplosion = iconArcaneExplosion != null ? iconArcaneExplosion : Art.Placeholder("ExplosionIconPlaceholder", Palette.Arcane, square: false);
        Art.IconCharge = iconArcaneCharge != null ? iconArcaneCharge : Art.Placeholder("ChargeIconPlaceholder", Palette.Arcane, square: false);
        Art.IconExplosionGrey = Art.Greyscale(Art.IconExplosion);
        Art.IconChargeGrey = Art.Greyscale(Art.IconCharge);
        Art.ArcaneMissile = fxArcaneMissile != null ? fxArcaneMissile : Art.Placeholder("MissilePlaceholder", Palette.Arcane, square: false);
        Art.ArcaneImpact = fxArcaneImpact != null ? fxArcaneImpact : Art.Placeholder("ImpactPlaceholder", Palette.Arcane, square: false);
        Art.ArcaneVortex = fxArcaneVortex != null ? fxArcaneVortex : Art.Placeholder("VortexPlaceholder", Palette.Arcane, square: false);
        Controls.Init(controls);
        RoomLibrary.Current = roomLibrary;
        UiArt.Current = uiArt;
        EnvironmentArt.Current = environmentArt;
        // Work on a copy so live slider changes don't silently overwrite the asset.
        Game.TuningAsset = config;
        Game.Tuning = config != null ? Instantiate(config) : ScriptableObject.CreateInstance<TuningConfig>();

        GameSettings.Load();
        GameSettings.Apply();
        Controls.ApplyOverrides();
        AudioListener.pause = false;

        var run = gameObject.AddComponent<RunManager>();
        run.PrepareRun();
        LevelLayout level = run.BuildFloor();
        PlayerBuilder.Create(level.playerStart, level.playerStartYaw, Game.Tuning);
        PostProcessSetup.Create(Game.Tuning, Game.Camera);

        gameObject.AddComponent<SpellFx>();
        gameObject.AddComponent<FirstPersonHands>();
        gameObject.AddComponent<LootDropper>();
        gameObject.AddComponent<GoblinCorpses>();
        var waves = gameObject.AddComponent<WaveSpawner>();
        gameObject.AddComponent<PhantomSpawner>();
        gameObject.AddComponent<CampingWatcher>();
        gameObject.AddComponent<WorldPopup>();
        gameObject.AddComponent<ScorchMarks>();
        gameObject.AddComponent<Hud>();
        gameObject.AddComponent<SpellBar>();
        gameObject.AddComponent<LevelUpScreen>();
        gameObject.AddComponent<BuzzEffects>();
        var collapse = gameObject.AddComponent<CollapseTimer>();
        gameObject.AddComponent<RunScreens>();
        gameObject.AddComponent<DebugPanel>();
        gameObject.AddComponent<PauseMenu>();
        run.Init(waves, collapse);
    }
}
