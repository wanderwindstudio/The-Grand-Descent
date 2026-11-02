using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The only object in the MainMenu scene (the first scene that loads). Builds the title screen:
// drifting background, purple smoke, logo, and the button column in the dark left third.
public class MenuBootstrap : MonoBehaviour
{
    public const string GameScene = "Prototype";

    [Tooltip("Assets/Settings/UiArt. Menu sprites, fonts and the intro.")]
    public UiArt art;
    [Tooltip("Assets/Settings/GrandDescentControls. Needed here so keys can be rebound in Settings.")]
    public InputActionAsset controls;

    Canvas canvas;
    RectTransform column, logo;
    RectTransform background;
    float time;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UiArt.Current = art;
        Controls.Init(controls);
        GameSettings.Load();
        GameSettings.Apply();
        Controls.ApplyOverrides();
        SaveSystem.MigratePlayerPrefs();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var cam = new GameObject("Menu Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 0;
        cam.gameObject.AddComponent<AudioListener>();

        canvas = UiKit.CreateCanvas("Main Menu", 0);
        Build();
    }

    void Build()
    {
        var root = canvas.transform;
        var bg = UiKit.Cover("Background", root, art != null ? art.menuBackground : null);
        background = bg.rectTransform;
        MenuSmoke.Create(root);

        // The artwork keeps its left third dark: logo top-left, buttons under it.
        var logoImage = UiKit.Image("Logo", root, art != null ? art.logo : null, Color.white);
        logoImage.preserveAspect = true;
        logo = UiKit.Place(logoImage.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -28f), new Vector2(600f, 356f));
        if (logoImage.sprite == null)
        {
            logoImage.color = Color.clear;
            var title = UiKit.Text("Title", logo, "THE GRAND DESCENT", 80, UiKit.Font.Title, UiKit.Gold, TMPro.TextAlignmentOptions.Center);
            UiKit.Stretch(title.rectTransform);
        }

        column = UiKit.Place(UiKit.Rect("Buttons", root), new Vector2(0f, 1f), new Vector2(120f, -410f), new Vector2(500f, 640f));
        float y = 0f;
        void Add(string label, System.Action action)
        {
            var b = UiKit.Button(label, column, label, new Vector2(480f, 86f), action, 50f);
            UiKit.Place((RectTransform)b.transform, new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(480f, 86f));
            y -= 100f;
        }

        if (SaveSystem.LatestRunSlot() >= 0) Add("Continue", Continue);
        Add("New Game", () => OpenSlots(SaveSlotsScreen.Mode.NewGame));
        Add("Load Game", () => OpenSlots(SaveSlotsScreen.Mode.Load));
        Add("Settings", OpenSettings);
        Add("Replay Intro", ReplayIntro);
        Add("Quit", Quit);
        column.GetComponentInChildren<Button>().Select();
    }

    void SetMainVisible(bool visible)
    {
        column.gameObject.SetActive(visible);
        logo.gameObject.SetActive(visible);
        if (visible) column.GetComponentInChildren<Button>().Select();
    }

    void Continue()
    {
        int slot = SaveSystem.LatestRunSlot();
        if (slot < 0) return;
        SaveSystem.Select(slot, startFresh: false);
        StartGame();
    }

    void OpenSlots(SaveSlotsScreen.Mode mode)
    {
        SetMainVisible(false);
        SaveSlotsScreen.Create(canvas.transform, mode, slot =>
        {
            SaveSystem.Select(slot, startFresh: mode == SaveSlotsScreen.Mode.NewGame);
            StartGame();
        }, () => SetMainVisible(true));
    }

    void OpenSettings()
    {
        SetMainVisible(false);
        SettingsScreen.Create(canvas.transform, () => SetMainVisible(true));
    }

    // The intro plays the first time a slot starts a game; afterwards straight in.
    void StartGame()
    {
        if (SaveSystem.Active.meta.introSeen)
        {
            SceneManager.LoadScene(GameScene);
            return;
        }
        SetMainVisible(false);
        IntroPlayer.Play(() =>
        {
            SaveSystem.Active.meta.introSeen = true;
            SaveSystem.SaveActive();
            SceneManager.LoadScene(GameScene);
        });
    }

    void ReplayIntro()
    {
        SetMainVisible(false);
        IntroPlayer.Play(() => SetMainVisible(true));
    }

    static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        // A very slow zoom on the background.
        time += Time.unscaledDeltaTime;
        background.localScale = Vector3.one * (1.04f + 0.035f * Mathf.Sin(time * 2f * Mathf.PI / 60f));

        // Esc closes an open sub-screen (not while the intro or a question is up).
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame && FindAnyObjectByType<IntroPlayer>() == null && GameObject.Find("Confirm") == null)
        {
            var settings = FindAnyObjectByType<SettingsScreen>();
            if (settings != null && !settings.IsRebinding) settings.Close();
            else if (settings == null) FindAnyObjectByType<SaveSlotsScreen>()?.Close();
        }
    }
}
