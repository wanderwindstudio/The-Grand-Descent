using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// The only thing in the scene. Builds everything else at runtime.
public class GameBootstrap : MonoBehaviour
{
    [Tooltip("Drag Assets/Settings/TuningConfig here. If empty, default values are used.")]
    public TuningConfig config;
    [Tooltip("Assets/Settings/GrayboxLit. Every coloured material is copied from it.")]
    public Material baseMaterial;

    void Awake()
    {
        Time.timeScale = 1f;
        GoblinEnemy.Active.Clear();
        Game.GodMode = false;
        Game.InputBlocked = false;

        Mats.Template = baseMaterial;
        // Work on a copy so live slider changes don't silently overwrite the asset.
        Game.TuningAsset = config;
        Game.Tuning = config != null ? Instantiate(config) : ScriptableObject.CreateInstance<TuningConfig>();

        LevelLighting.Apply(Game.Tuning);
        LevelLayout level = RoomBuilder.Build(Game.Tuning);
        PlayerBuilder.Create(level.playerStart, level.playerStartYaw, Game.Tuning);
        PostProcessSetup.Create(Game.Tuning, Game.Camera);

        gameObject.AddComponent<LootDropper>();
        gameObject.AddComponent<WaveSpawner>().Init(level.spawnPoints);
        gameObject.AddComponent<Hud>();
        gameObject.AddComponent<LevelUpScreen>();
        gameObject.AddComponent<DebugPanel>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (Game.PlayerHealth.IsDead && kb != null && kb.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
