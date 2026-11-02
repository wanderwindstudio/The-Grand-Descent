using UnityEngine;
using UnityEngine.UI;

// Esc (the Pause action) during a run: stops the game and shows Resume, Settings, Save & Quit, Quit to Menu.
public class PauseMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    Canvas canvas;
    GameObject main;
    SettingsScreen settings;

    void Awake() => IsOpen = false;

    void Update()
    {
        if (!Controls.Pause.WasPressedThisFrame()) return;
        if (!IsOpen)
        {
            if (CanPause()) Open();
            return;
        }
        if (canvas.transform.Find("Confirm") != null) return;
        if (settings != null) { if (!settings.IsRebinding) settings.Close(); }
        else Resume();
    }

    // Not during the level-up choice, the summary, death or with the debug panel open.
    static bool CanPause() =>
        RunManager.Instance.Current == RunManager.State.Playing && Time.timeScale > 0f &&
        !Game.InputBlocked && !Game.PlayerHealth.IsDead;

    void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        Game.InputBlocked = true;
        AudioListener.pause = true;
        PlayerLook.UnlockCursor();

        canvas = UiKit.CreateCanvas("Pause Menu", 100);
        var dim = UiKit.Image("Dim", canvas.transform, null, new Color(0f, 0f, 0f, 0.6f));
        UiKit.Stretch(dim.rectTransform);
        dim.raycastTarget = true;

        main = UiKit.Panel("Panel", canvas.transform, 50f).gameObject;
        UiKit.Place((RectTransform)main.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 660f));
        var title = UiKit.Text("Title", main.transform, "Paused", 80, UiKit.Font.Title, UiKit.Gold, TMPro.TextAlignmentOptions.Center);
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(500f, 100f));

        float y = -170f;
        void Add(string label, System.Action action)
        {
            var b = UiKit.Button(label, main.transform, label, new Vector2(440f, 84f), action, 46f);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(440f, 84f));
            y -= 100f;
        }
        Add("Resume", Resume);
        Add("Settings", OpenSettings);
        Add(SaveSystem.ActiveSlot >= 0 ? "Save & Quit" : "Quit (test run)", SaveAndQuit);
        Add("Quit to Menu", AskQuit);
        main.GetComponentInChildren<Button>().Select();
    }

    public void Resume()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Destroy(canvas.gameObject);
        settings = null;
        GameSettings.Save();
        Time.timeScale = 1f;
        Game.InputBlocked = false;
        AudioListener.pause = false;
        PlayerLook.LockCursor();
    }

    void OpenSettings()
    {
        main.SetActive(false);
        settings = SettingsScreen.Create(canvas.transform, () =>
        {
            settings = null;
            main.SetActive(true);
            main.GetComponentInChildren<Button>().Select();
        });
    }

    void SaveAndQuit()
    {
        IsOpen = false;
        RunManager.Instance.SaveAndQuit();
    }

    void AskQuit()
    {
        string warning = SaveSystem.ActiveSlot >= 0
            ? "Quit to the menu?\nEverything since this floor's autosave will be lost."
            : "Quit to the menu?\nThis test run isn't saved.";
        ConfirmDialog.Show(canvas.transform, warning, "Quit", () =>
        {
            IsOpen = false;
            RunManager.Instance.QuitToMenu();
        });
    }

    void OnDestroy() => IsOpen = false;
}
