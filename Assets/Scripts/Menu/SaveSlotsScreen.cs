using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The three save slots as parchment tickets. Load mode: pick a save to play (each has a Delete button).
// New Game mode: pick a slot to start in (an occupied one asks before overwriting).
public class SaveSlotsScreen : MonoBehaviour
{
    public enum Mode { Load, NewGame }

    static readonly Color InkBrown = new Color(0.22f, 0.13f, 0.07f);

    Mode mode;
    Action<int> onPick;
    Action onBack;
    Transform canvas;

    public static SaveSlotsScreen Create(Transform canvas, Mode mode, Action<int> onPick, Action onBack)
    {
        var backdrop = UiKit.Image("Save Slots", canvas, null, new Color(0f, 0f, 0f, 0.6f));
        UiKit.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;
        var screen = backdrop.gameObject.AddComponent<SaveSlotsScreen>();
        screen.mode = mode;
        screen.onPick = onPick;
        screen.onBack = onBack;
        screen.canvas = canvas;
        screen.Build();
        return screen;
    }

    void Build()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        string heading = mode == Mode.Load ? "Load Game" : "New Game - choose a slot";
        var title = UiKit.Text("Title", transform, heading, 76, UiKit.Font.Title, UiKit.Gold, TextAlignmentOptions.Center);
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 462f), new Vector2(1200f, 100f));

        for (int i = 0; i < SaveSystem.SlotCount; i++) Card(i, new Vector2(0f, 270f - i * 270f));

        var back = UiKit.Button("Back", transform, "Back", new Vector2(280f, 84f), Close, 42f);
        UiKit.Place((RectTransform)back.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -470f), new Vector2(280f, 84f));
    }

    void Card(int slot, Vector2 position)
    {
        var data = SaveSystem.Load(slot);
        var art = UiArt.Current;
        bool hasArt = art != null && art.saveSlotFrame != null;
        var frame = UiKit.Image("Slot " + (slot + 1), transform, hasArt ? art.saveSlotFrame : null, hasArt ? Color.white : new Color(0.85f, 0.75f, 0.55f));
        UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(940f, 258f));
        UiKit.BorderSize(frame, 70f);
        frame.raycastTarget = true;

        bool usable = mode == Mode.NewGame || data != null;
        var button = frame.gameObject.AddComponent<Button>();
        button.targetGraphic = frame;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.93f, 0.75f);
        colors.pressedColor = new Color(0.85f, 0.75f, 0.6f);
        colors.disabledColor = new Color(0.7f, 0.68f, 0.64f);
        button.colors = colors;
        button.interactable = usable;
        button.onClick.AddListener(() => { UiKit.PlayClick(); Pick(slot, data); });

        var name = Plain(UiKit.Text("Name", frame.transform, $"Slot {slot + 1}", 50, UiKit.Font.Title, InkBrown, TextAlignmentOptions.TopLeft));
        UiKit.Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(100f, -34f), new Vector2(300f, 64f));

        string details;
        if (data == null) details = "Empty";
        else
        {
            var m = data.meta;
            details = $"Banked gold {m.bankedGold}   ·   Runs {m.totalRuns}   ·   Best floor {m.bestFloor}\n" +
                      (data.hasRun ? $"Run in progress: floor {data.run.floor}" : "No run in progress") + "\n" +
                      $"Saved {data.SavedAt:d MMM yyyy, HH:mm}";
        }
        var text = Plain(UiKit.Text("Details", frame.transform, details, 29, UiKit.Font.Body, InkBrown, TextAlignmentOptions.TopLeft));
        UiKit.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(100f, -94f), new Vector2(580f, 115f));
        text.lineSpacing = -12f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        if (data == null)
        {
            text.fontSize = 44;
            text.color = new Color(InkBrown.r, InkBrown.g, InkBrown.b, 0.6f);
        }

        if (data != null)
        {
            var delete = UiKit.Button("Delete", frame.transform, "Delete", new Vector2(190f, 66f), () => AskDelete(slot), 32f);
            UiKit.Place((RectTransform)delete.transform, new Vector2(1f, 0.5f), new Vector2(-95f, 0f), new Vector2(190f, 66f));
        }
    }

    // Dark ink straight on the parchment, without the light-text outline.
    static TextMeshProUGUI Plain(TextMeshProUGUI text)
    {
        if (text.font != null) text.fontSharedMaterial = text.font.material;
        return text;
    }

    void Pick(int slot, SaveData data)
    {
        if (mode == Mode.NewGame && data != null)
        {
            ConfirmDialog.Show(canvas, $"Overwrite this save?\nSlot {slot + 1} will be lost.", "Overwrite", () => Done(slot));
            return;
        }
        Done(slot);
    }

    void AskDelete(int slot)
    {
        ConfirmDialog.Show(canvas, $"Delete slot {slot + 1}?\nThis can't be undone.", "Delete", () =>
        {
            SaveSystem.Delete(slot);
            Build();
        });
    }

    void Done(int slot)
    {
        Destroy(gameObject);
        onPick?.Invoke(slot);
    }

    public void Close()
    {
        Destroy(gameObject);
        onBack?.Invoke();
    }
}
