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
        // Work on a copy so live slider changes don't silently overwrite the asset.
        Game.TuningAsset = config;
        Game.Tuning = config != null ? Instantiate(config) : ScriptableObject.CreateInstance<TuningConfig>();

        var run = gameObject.AddComponent<RunManager>();
        LevelLayout level = run.BuildFloor();
        PlayerBuilder.Create(level.playerStart, level.playerStartYaw, Game.Tuning);
        PostProcessSetup.Create(Game.Tuning, Game.Camera);

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
