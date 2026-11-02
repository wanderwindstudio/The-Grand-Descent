using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Settings panel, used by the main menu and the pause menu. Four tabs: Controls, Video, Audio, Comfort.
// Every change applies immediately; Back writes settings.json.
public class SettingsScreen : MonoBehaviour
{
    enum Tab { Controls, Video, Audio, Comfort }

    const float RowHeight = 60f, LabelWidth = 430f, ControlWidth = 520f;

    Action onBack;
    RectTransform content;
    Tab tab;
    readonly List<MenuButton> tabButtons = new List<MenuButton>();
    InputActionRebindingExtensions.RebindingOperation rebinding;

    public bool IsRebinding => rebinding != null;

    public static SettingsScreen Create(Transform parent, Action onBack)
    {
        var backdrop = UiKit.Image("Settings", parent, null, new Color(0f, 0f, 0f, 0.55f));
        UiKit.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;
        var screen = backdrop.gameObject.AddComponent<SettingsScreen>();
        screen.onBack = onBack;
        screen.Build(backdrop.transform);
        return screen;
    }

    void Build(Transform root)
    {
        var panel = UiKit.Panel("Panel", root, 56f);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 960f));

        var title = UiKit.Text("Title", panel.transform, "Settings", 72, UiKit.Font.Title, UiKit.Gold, TextAlignmentOptions.Center);
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 90f));

        var tabs = (Tab[])Enum.GetValues(typeof(Tab));
        for (int i = 0; i < tabs.Length; i++)
        {
            Tab t = tabs[i];
            var b = UiKit.Button("Tab " + t, panel.transform, t.ToString(), new Vector2(240f, 74f), () => Show(t), 38f);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2((i - 1.5f) * 250f, -150f), new Vector2(240f, 74f));
            tabButtons.Add(b);
        }

        content = UiKit.Rect("Content", panel.transform);
        UiKit.Place(content, new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(LabelWidth + ControlWidth + 40f, 580f));

        var back = UiKit.Button("Back", panel.transform, "Back", new Vector2(260f, 84f), Close, 42f);
        UiKit.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(260f, 84f));

        Show(Tab.Controls);
    }

    public void Close()
    {
        CancelRebind();
        GameSettings.Save();
        Destroy(gameObject);
        onBack?.Invoke();
    }

    void Show(Tab t)
    {
        CancelRebind();
        tab = t;
        foreach (Transform child in content) Destroy(child.gameObject);
        rows = 0;
        for (int i = 0; i < tabButtons.Count; i++) tabButtons[i].Label.fontStyle = i == (int)t ? FontStyles.Underline : FontStyles.Normal;

        var s = GameSettings.Current;
        switch (t)
        {
            case Tab.Controls:
                SliderRow("Mouse sensitivity", 0.2f, 3f, s.mouseSensitivity, v => s.mouseSensitivity = v, v => $"{v * 100f:0}%");
                ToggleRow("Invert Y", () => s.invertY, v => s.invertY = v);
                foreach (var action in Controls.All) BindingRow(action);
                var reset = UiKit.Button("Reset", content, "Reset to defaults", new Vector2(340f, 70f), () => { Controls.ResetToDefaults(); Show(Tab.Controls); }, 34f);
                UiKit.Place((RectTransform)reset.transform, new Vector2(0f, 0f), new Vector2(0f, -70f), new Vector2(340f, 70f));
                break;

            case Tab.Video:
                ResolutionRow();
                ToggleRow("Fullscreen", () => s.fullscreen, v => { s.fullscreen = v; GameSettings.Apply(); });
                ToggleRow("VSync", () => s.vSync, v => { s.vSync = v; GameSettings.Apply(); });
                SliderRow("Field of view", 70f, 110f, s.fieldOfView, v => s.fieldOfView = Mathf.Round(v), v => $"{Mathf.Round(v):0}°");
                SliderRow("Brightness", -1f, 1f, s.brightness, v => s.brightness = v, v => $"{(v >= 0f ? "+" : "")}{v * 100f:0}%");
                break;

            case Tab.Audio:
                SliderRow("Master volume", 0f, 1f, s.masterVolume, v => { s.masterVolume = v; GameSettings.Apply(); }, Percent);
                SliderRow("Music volume", 0f, 1f, s.musicVolume, v => s.musicVolume = v, Percent);
                SliderRow("Sound effects", 0f, 1f, s.sfxVolume, v => s.sfxVolume = v, Percent);
                break;

            case Tab.Comfort:
                SliderRow("Screen shake", 0f, 1f, s.screenShake, v => s.screenShake = v, Percent);
                SliderRow("Buzz distortion", 0f, 1f, s.buzzDistortion, v => s.buzzDistortion = v, Percent);
                SliderRow("Head and hand bob", 0f, 1f, s.headBob, v => s.headBob = v, Percent);
                var note = UiKit.Text("Note", content, "Lower these if the shaking or the Buzz warping makes you feel unwell.", 30, UiKit.Font.Italic, UiKit.TextDim, TextAlignmentOptions.Center);
                UiKit.Place(note.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -rows * RowHeight - 20f), new Vector2(LabelWidth + ControlWidth, 80f));
                break;
        }
    }

    static string Percent(float v) => $"{v * 100f:0}%";

    // --- Rows ---------------------------------------------------------------------------------------------------

    int rows;

    RectTransform Row(string label)
    {
        var row = UiKit.Rect(label, content);
        UiKit.Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -rows * RowHeight), new Vector2(LabelWidth + ControlWidth + 40f, RowHeight));
        rows++;
        var text = UiKit.Text("Label", row, label, 36, UiKit.Font.Body, UiKit.TextLight, TextAlignmentOptions.MidlineLeft);
        UiKit.Place(text.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(LabelWidth, RowHeight));
        return row;
    }

    void SliderRow(string label, float min, float max, float value, Action<float> set, Func<float, string> format)
    {
        var row = Row(label);
        var valueText = UiKit.Text("Value", row, format(value), 32, UiKit.Font.Body, UiKit.Gold, TextAlignmentOptions.MidlineRight);
        UiKit.Place(valueText.rectTransform, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(110f, RowHeight));
        var slider = UiKit.Slider("Slider", row, new Vector2(ControlWidth - 130f, 40f), min, max, value, v =>
        {
            set(v);
            valueText.text = format(v);
        });
        UiKit.Place((RectTransform)slider.transform, new Vector2(1f, 0.5f), new Vector2(-125f, 0f), new Vector2(ControlWidth - 130f, 40f));
    }

    void ToggleRow(string label, Func<bool> get, Action<bool> set)
    {
        var row = Row(label);
        MenuButton button = null;
        button = UiKit.Button("Toggle", row, get() ? "On" : "Off", new Vector2(200f, 58f), () =>
        {
            set(!get());
            button.SetLabel(get() ? "On" : "Off");
        }, 32f);
        UiKit.Place((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(-125f, 0f), new Vector2(200f, 58f));
    }

    void ResolutionRow()
    {
        var row = Row("Resolution");
        var options = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().ToList();
        var current = new Vector2Int(Screen.width, Screen.height);
        if (!options.Contains(current)) options.Add(current);
        int index = options.IndexOf(current);
        MenuButton button = null;
        button = UiKit.Button("Resolution", row, Format(options[index]), new Vector2(ControlWidth - 130f, 58f), () =>
        {
            index = (index + 1) % options.Count;
            var s = GameSettings.Current;
            s.resolutionWidth = options[index].x;
            s.resolutionHeight = options[index].y;
            GameSettings.Apply();
            button.SetLabel(Format(options[index]));
        }, 32f);
        UiKit.Place((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(-125f, 0f), new Vector2(ControlWidth - 130f, 58f));
        if (Application.isEditor)
        {
            var note = UiKit.Text("Note", row, "(build only)", 24, UiKit.Font.Italic, UiKit.TextDim, TextAlignmentOptions.MidlineRight);
            UiKit.Place(note.rectTransform, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(120f, RowHeight));
        }
    }

    static string Format(Vector2Int r) => $"{r.x} x {r.y}";

    void BindingRow(InputAction action)
    {
        var row = Row(Controls.Label(action));
        MenuButton button = null;
        button = UiKit.Button("Key", row, Controls.KeyName(action), new Vector2(ControlWidth - 130f, 58f), () => StartRebind(action, button), 32f);
        UiKit.Place((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(-125f, 0f), new Vector2(ControlWidth - 130f, 58f));
    }

    // Click a key button, then press the new key or mouse button. Backspace cancels.
    void StartRebind(InputAction action, MenuButton button)
    {
        CancelRebind();
        button.SetLabel("Press a key... (Backspace cancels)");
        Controls.Disable();
        rebinding = action.PerformInteractiveRebinding(0)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .WithControlsExcluding("<Pointer>/position")
            .WithCancelingThrough("<Keyboard>/backspace")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op => FinishRebind(action, button))
            .OnCancel(op => FinishRebind(action, button))
            .Start();
    }

    void FinishRebind(InputAction action, MenuButton button)
    {
        rebinding?.Dispose();
        rebinding = null;
        Controls.Enable();
        Controls.StoreOverrides();
        if (button != null) button.SetLabel(Controls.KeyName(action));
    }

    void CancelRebind()
    {
        if (rebinding == null) return;
        rebinding.Cancel();
        rebinding?.Dispose();
        rebinding = null;
        Controls.Enable();
    }

    void OnDestroy() => CancelRebind();
}
