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
        RunStats.Reset();
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
        PlaceShrines(level);

        gameObject.AddComponent<LootDropper>();
        gameObject.AddComponent<WaveSpawner>().Init(level.spawnPoints);
        gameObject.AddComponent<Hud>();
        gameObject.AddComponent<LevelUpScreen>();
        gameObject.AddComponent<BuzzEffects>();
        gameObject.AddComponent<DebugPanel>();
    }

    // 1-2 shrines on random spots from the layout's list.
    static void PlaceShrines(LevelLayout level)
    {
        var spots = new System.Collections.Generic.List<Vector3>(level.shrineSpots);
        int count = Mathf.Min(spots.Count, Random.Range(Game.Tuning.shrinesMin, Game.Tuning.shrinesMax + 1));
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, spots.Count);
            PipeShrine.Create(spots[index]);
            spots.RemoveAt(index);
        }
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
