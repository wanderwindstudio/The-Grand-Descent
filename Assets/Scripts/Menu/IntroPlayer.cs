using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Plays the intro visual-novel style: a background per scene, portraits sliding in at the bottom
// (the hero on the left, everyone else on the right), and a dialogue box with typewriter text.
// Click / Space / Enter: finish the line, then go on. Skip button or hold Esc: skip it all.
public class IntroPlayer : MonoBehaviour
{
    const float CharsPerSecond = 45f;
    const float FadeTime = 0.45f;
    const float SlideTime = 0.35f;
    const float CrossfadeTime = 0.15f;
    const float HoldToSkip = 1f;
    const float PortraitFraction = 0.7f; // of the screen height
    static readonly Color Dimmed = new Color(0.42f, 0.42f, 0.48f);

    // One side of the stage. Two images so expressions of the same character can crossfade.
    class Slot
    {
        public RectTransform root;
        public Image front, back;
        public Image maskFront, maskBack; // invisible copies of the portrait that clip the scanlines
        public string character, portrait;
        public float slide; // 0 = hidden below the screen, 1 = in place
        public float slideTarget;
        public bool speaking;
    }

    Action onDone;
    List<IntroScript.Line> lines;
    RectTransform stage;
    Image background, fader;
    RectTransform backgroundRect;
    Slot left, right;
    Image box, nameTab;
    TextMeshProUGUI dialogue, nameText, skipHint;
    RectTransform skipButton;
    string scene;
    float sceneTime, shake, escHeld;
    bool skipping, done;
    Texture2D scanlines;
    AudioClip bang;

    public static IntroPlayer Play(Action onDone)
    {
        var canvas = UiKit.CreateCanvas("Intro", 50);
        var player = canvas.gameObject.AddComponent<IntroPlayer>();
        player.onDone = onDone;
        player.Build(canvas.transform);
        var art = UiArt.Current;
        player.lines = IntroScript.Parse(art != null && art.introScript != null ? art.introScript.text : "");
        if (player.lines.Count == 0) Debug.LogWarning("The intro script is empty or missing (Assets/Data/Intro/intro_script.txt).");
        player.StartCoroutine(player.Run());
        return player;
    }

    // --- Building -----------------------------------------------------------------------------------------------

    void Build(Transform root)
    {
        var black = UiKit.Image("Black", root, null, Color.black);
        UiKit.Stretch(black.rectTransform);

        stage = UiKit.Stretch(UiKit.Rect("Stage", root));
        background = UiKit.Cover("Background", stage, null);
        backgroundRect = background.rectTransform;

        scanlines = MakeScanlines();
        float size = 1080f * PortraitFraction;
        left = MakeSlot("Left", new Vector2(0f, 0f), new Vector2(40f, 0f), size);
        right = MakeSlot("Right", new Vector2(1f, 0f), new Vector2(-40f, 0f), size);

        // Dialogue box across the bottom, in front of the portraits.
        box = UiKit.Panel("Dialogue", root, 46f);
        var boxRect = box.rectTransform;
        boxRect.anchorMin = new Vector2(0f, 0f);
        boxRect.anchorMax = new Vector2(1f, 0f);
        boxRect.pivot = new Vector2(0.5f, 0f);
        boxRect.offsetMin = new Vector2(90f, 24f);
        boxRect.offsetMax = new Vector2(-90f, 24f + 250f);
        dialogue = UiKit.Text("Text", box.transform, "", 42, UiKit.Font.Body, UiKit.TextLight, TextAlignmentOptions.TopLeft);
        UiKit.Stretch(dialogue.rectTransform);
        dialogue.rectTransform.offsetMin = new Vector2(70f, 46f);
        dialogue.rectTransform.offsetMax = new Vector2(-70f, -46f);

        nameTab = UiKit.Panel("Name", root, 26f);
        nameText = UiKit.Text("Text", nameTab.transform, "", 44, UiKit.Font.Title, UiKit.Gold, TextAlignmentOptions.Center);
        UiKit.Stretch(nameText.rectTransform);
        nameText.rectTransform.offsetMin = new Vector2(24f, 6f);
        nameText.rectTransform.offsetMax = new Vector2(-24f, -2f);
        nameText.textWrappingMode = TextWrappingModes.NoWrap;

        var skip = UiKit.Button("Skip", root, "Skip", new Vector2(200f, 72f), Skip, 36f);
        skipButton = (RectTransform)skip.transform;
        UiKit.Place(skipButton, new Vector2(1f, 1f), new Vector2(-30f, -26f), new Vector2(200f, 72f));
        skipHint = UiKit.Text("Hint", root, "", 26, UiKit.Font.Italic, UiKit.TextDim, TextAlignmentOptions.TopRight);
        UiKit.Place(skipHint.rectTransform, new Vector2(1f, 1f), new Vector2(-34f, -104f), new Vector2(400f, 40f));

        fader = UiKit.Image("Fader", root, null, Color.black);
        UiKit.Stretch(fader.rectTransform);
        fader.raycastTarget = false;

        bang = ToneGenerator.Thud("IntroBang", 70f, 0.25f);
    }

    Slot MakeSlot(string name, Vector2 anchor, Vector2 position, float size)
    {
        var slot = new Slot { root = UiKit.Place(UiKit.Rect(name, stage), anchor, position, new Vector2(size, size)) };
        slot.back = PortraitImage("Back", slot.root, out slot.maskBack);
        slot.front = PortraitImage("Front", slot.root, out slot.maskFront);
        Hide(slot.front);
        Hide(slot.back);
        slot.root.anchoredPosition = new Vector2(position.x, -size - 20f);
        return slot;
    }

    // The portrait, plus an invisible copy of it that masks the TV scanlines to the drawn pixels.
    // (A Mask on the visible image itself would ignore its tint, so the dimming wouldn't show.)
    Image PortraitImage(string name, RectTransform parent, out Image mask)
    {
        var img = UiKit.Image(name, parent, null, Color.white);
        UiKit.Stretch(img.rectTransform);
        img.preserveAspect = true;
        mask = UiKit.Image("Scanline Mask", img.transform, null, Color.white);
        UiKit.Stretch(mask.rectTransform);
        mask.preserveAspect = true;
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var scan = UiKit.Stretch(UiKit.Rect("Scanlines", mask.transform)).gameObject.AddComponent<RawImage>();
        scan.texture = scanlines;
        scan.raycastTarget = false;
        mask.gameObject.SetActive(false);
        return img;
    }

    static void Hide(Image img)
    {
        img.sprite = null;
        img.color = Color.clear;
    }

    // Faint dark lines every few pixels, tiled over Skully's portrait during the TV scene.
    static Texture2D MakeScanlines()
    {
        var tex = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
        tex.SetPixels(new[] { new Color(0, 0, 0, 0.35f), new Color(0, 0, 0, 0.1f), Color.clear, new Color(0.6f, 0.8f, 1f, 0.08f) });
        tex.Apply();
        return tex;
    }

    // --- Playing ------------------------------------------------------------------------------------------------

    IEnumerator Run()
    {
        fader.color = Color.black;
        foreach (var line in lines)
        {
            if (skipping) break;
            if (line.scene != scene) yield return ChangeScene(line.scene);
            if (skipping) break;
            ShowSpeaker(line);
            yield return Type(line);
        }
        yield return Fade(1f);
        Finish();
    }

    IEnumerator ChangeScene(string id)
    {
        if (scene != null) yield return Fade(1f);
        scene = id;
        sceneTime = 0f;
        ClearSlot(left);
        ClearSlot(right);
        var entry = UiArt.Current != null ? UiArt.Current.Background(id) : null;
        SetBackground(entry != null ? entry.sprite : null, id);
        yield return Fade(0f);
    }

    void SetBackground(Sprite sprite, string id)
    {
        foreach (Transform child in backgroundRect) Destroy(child.gameObject);
        background.sprite = sprite;
        background.color = sprite != null ? Color.white : new Color(0.1f, 0.08f, 0.12f);
        var fitter = background.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = sprite != null ? sprite.rect.width / sprite.rect.height : 16f / 9f;
        if (sprite == null)
        {
            var label = UiKit.Text("Missing", backgroundRect, $"missing background: {id}", 48, UiKit.Font.Body, UiKit.TextDim, TextAlignmentOptions.Center);
            UiKit.Stretch(label.rectTransform);
            Debug.LogWarning($"Intro: no background for scene \"{id}\" (add it to Assets/Settings/UiArt.asset).");
        }
    }

    void ShowSpeaker(IntroScript.Line line)
    {
        left.speaking = right.speaking = false;
        nameTab.gameObject.SetActive(line.speaker != null);
        var art = UiArt.Current;
        TMP_FontAsset italic = art != null ? art.italicFont : null, body = art != null ? art.bodyFont : null;
        TMP_FontAsset font = line.IsNarration ? italic : body;
        if (font != null) dialogue.font = font;
        dialogue.alignment = line.IsNarration ? TextAlignmentOptions.Center : TextAlignmentOptions.TopLeft;
        dialogue.fontStyle = line.IsNarration && italic == null ? FontStyles.Italic : FontStyles.Normal;
        dialogue.color = line.IsNarration ? new Color(0.9f, 0.86f, 0.8f) : UiKit.TextLight;
        if (line.speaker != null) nameText.text = line.speaker;

        Slot slot = null;
        if (line.portrait != null)
        {
            var entry = UiArt.Current != null ? UiArt.Current.Portrait(line.portrait) : null;
            string character = entry != null && !string.IsNullOrEmpty(entry.character) ? entry.character : (line.speaker ?? line.portrait).ToLowerInvariant();
            slot = character == "hero" ? left : right;
            SetPortrait(slot, character, line.portrait, entry != null ? entry.sprite : null);
            slot.speaking = true;
        }
        else if (line.speaker != null)
        {
            // A named speaker without a portrait: light up whoever already stands there as that character.
            if (left.character == line.speaker.ToLowerInvariant()) { left.speaking = true; slot = left; }
            if (right.character == line.speaker.ToLowerInvariant()) { right.speaking = true; slot = right; }
        }

        // The name tab sits above the box on the speaker's side.
        bool onLeft = slot == null || slot == left;
        var tab = nameTab.rectTransform;
        UiKit.Place(tab, new Vector2(onLeft ? 0f : 1f, 0f), new Vector2(onLeft ? 150f : -150f, 24f + 250f - 14f), new Vector2(Mathf.Max(260f, 60f + nameText.GetPreferredValues(line.speaker ?? "").x), 84f));
        tab.SetAsLastSibling();
        fader.transform.SetAsLastSibling();
    }

    void SetPortrait(Slot slot, string character, string portrait, Sprite sprite)
    {
        if (slot.character == character && slot.portrait == portrait) return;
        if (slot.character == character)
        {
            // Same character, new expression: crossfade.
            (slot.front, slot.back) = (slot.back, slot.front);
            (slot.maskFront, slot.maskBack) = (slot.maskBack, slot.maskFront);
            slot.front.transform.SetAsLastSibling();
            Assign(slot.front, sprite, portrait);
            slot.portrait = portrait;
            StartCoroutine(Crossfade(slot));
            return;
        }
        // Someone new: the old one drops out, the new one slides up.
        StartCoroutine(SwapIn(slot, character, portrait, sprite));
    }

    IEnumerator SwapIn(Slot slot, string character, string portrait, Sprite sprite)
    {
        bool wasThere = slot.character != null;
        slot.character = character;
        slot.portrait = portrait;
        if (wasThere)
        {
            slot.slideTarget = 0f;
            while (slot.slide > 0.01f) yield return null;
        }
        Hide(slot.back);
        Assign(slot.front, sprite, portrait);
        slot.slideTarget = 1f;
    }

    void Assign(Image img, Sprite sprite, string id)
    {
        foreach (Transform child in img.transform)
            if (child.name == "Missing") Destroy(child.gameObject);
        img.sprite = sprite;
        img.color = Color.white;
        if (sprite == null)
        {
            img.color = new Color(0.25f, 0.22f, 0.28f, 0.85f);
            var label = UiKit.Text("Missing", img.transform, $"missing portrait:\n{id}", 40, UiKit.Font.Body, UiKit.TextLight, TextAlignmentOptions.Center);
            UiKit.Stretch(label.rectTransform);
            Debug.LogWarning($"Intro: no portrait \"{id}\" (add it to Assets/Settings/UiArt.asset).");
        }
    }

    IEnumerator Crossfade(Slot slot)
    {
        for (float t = 0f; t < CrossfadeTime; t += Time.unscaledDeltaTime)
        {
            float k = t / CrossfadeTime;
            slot.front.canvasRenderer.SetAlpha(k);
            slot.back.canvasRenderer.SetAlpha(1f - k);
            yield return null;
        }
        slot.front.canvasRenderer.SetAlpha(1f);
        Hide(slot.back);
        slot.back.canvasRenderer.SetAlpha(1f);
    }

    void ClearSlot(Slot slot)
    {
        slot.character = slot.portrait = null;
        slot.slide = slot.slideTarget = 0f;
        Hide(slot.front);
        Hide(slot.back);
        PlaceSlot(slot);
    }

    IEnumerator Type(IntroScript.Line line)
    {
        dialogue.text = line.text;
        dialogue.maxVisibleCharacters = 0;
        dialogue.ForceMeshUpdate();
        int total = dialogue.textInfo.characterCount;
        if (line.IsLoud)
        {
            shake = 1f;
            UiKit.PlaySound(bang, 0.8f);
        }

        float shown = 0f;
        while (shown < total && !skipping)
        {
            yield return null;
            if (AdvancePressed()) break;
            shown += CharsPerSecond * Time.unscaledDeltaTime;
            dialogue.maxVisibleCharacters = Mathf.FloorToInt(shown);
        }
        dialogue.maxVisibleCharacters = 99999;
        while (!skipping)
        {
            yield return null;
            if (AdvancePressed()) break;
        }
    }

    bool AdvancePressed()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) return true;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return !RectTransformUtility.RectangleContainsScreenPoint(skipButton, mouse.position.ReadValue(), null);
        return false;
    }

    IEnumerator Fade(float to)
    {
        float from = fader.color.a;
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            fader.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / FadeTime));
            yield return null;
        }
        fader.color = new Color(0f, 0f, 0f, to);
    }

    public void Skip() => skipping = true;

    void Finish()
    {
        if (done) return;
        done = true;
        var callback = onDone;
        Destroy(gameObject);
        callback?.Invoke();
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        sceneTime += dt;

        // Hold Esc to skip.
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.isPressed)
        {
            escHeld += dt;
            if (escHeld >= HoldToSkip) Skip();
        }
        else escHeld = 0f;
        skipHint.text = escHeld > 0f ? $"Skipping... {Mathf.CeilToInt((HoldToSkip - escHeld) * 10f) / 10f:0.0}" : "or hold Esc";

        // Backgrounds drift: a slow zoom and pan.
        float k = Mathf.Clamp01(sceneTime / 30f);
        float zoom = Mathf.Lerp(1.03f, 1.09f, k);
        backgroundRect.localScale = Vector3.one * zoom;
        backgroundRect.anchoredPosition = new Vector2(Mathf.Lerp(-1f, 1f, k) * 0.4f * (zoom - 1f) * backgroundRect.rect.width * 0.5f, 0f);

        // Screen shake for loud lines (scaled by the comfort setting).
        shake = Mathf.MoveTowards(shake, 0f, dt * 2.5f);
        stage.anchoredPosition = UnityEngine.Random.insideUnitCircle * (22f * shake * shake * GameSettings.Shake);

        UpdateSlot(left, dt);
        UpdateSlot(right, dt);
    }

    void UpdateSlot(Slot slot, float dt)
    {
        slot.slide = Mathf.MoveTowards(slot.slide, slot.slideTarget, dt / SlideTime);
        PlaceSlot(slot);

        bool tv = scene == "tv" && slot.character == "skully";
        Color tint = slot.speaking ? Color.white : Dimmed;
        if (tv)
        {
            // Old TV: a soft blue pulse with the odd flicker.
            float pulse = 0.5f + 0.5f * Mathf.Sin(sceneTime * 7f);
            float flicker = UnityEngine.Random.value < 0.04f ? 0.8f : 1f;
            tint *= Color.Lerp(Color.white, new Color(0.72f, 0.86f, 1f), 0.4f + 0.3f * pulse) * flicker;
            tint.a = 1f;
        }
        foreach (var (img, mask) in new[] { (slot.front, slot.maskFront), (slot.back, slot.maskBack) })
        {
            if (img.sprite != null) img.color = new Color(tint.r, tint.g, tint.b, img.color.a);
            bool scan = tv && img.sprite != null;
            if (mask.gameObject.activeSelf != scan) mask.gameObject.SetActive(scan);
            if (!scan) continue;
            mask.sprite = img.sprite;
            var lines = mask.transform.GetChild(0).GetComponent<RawImage>();
            lines.uvRect = new Rect(0f, -sceneTime * 0.6f, 1f, img.rectTransform.rect.height / 4f);
            lines.color = new Color(1f, 1f, 1f, img.canvasRenderer.GetAlpha());
        }
    }

    static void PlaceSlot(Slot slot)
    {
        float size = slot.root.sizeDelta.y;
        float eased = 1f - (1f - slot.slide) * (1f - slot.slide); // fast in, gentle stop
        var p = slot.root.anchoredPosition;
        slot.root.anchoredPosition = new Vector2(p.x, Mathf.Lerp(-size - 20f, 0f, eased));
    }
}
