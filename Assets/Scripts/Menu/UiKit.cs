using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Builds menu UI from code: canvases, plank buttons, framed panels, sliders and text.
// Layout units are a 1920 x 1080 canvas that scales with the screen height.
public static class UiKit
{
    public static readonly Color TextLight = new Color(0.98f, 0.91f, 0.74f);
    public static readonly Color TextDim = new Color(0.72f, 0.66f, 0.56f);
    public static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
    public static readonly Color Ink = new Color(0.05f, 0.03f, 0.04f);

    public enum Font { Title, Body, Italic }

    static UiArt Art => UiArt.Current;
    static AudioSource audio;
    static AudioClip click, hover;
    static Material titleMaterial, bodyMaterial;

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f; // always 1080 units tall; the width varies with the aspect ratio
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // --- Layout -------------------------------------------------------------------------------------------------

    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static RectTransform Stretch(RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    // anchor = pivot = the same point of the parent and of the element, e.g. (0, 1) = top-left.
    public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    // --- Images -------------------------------------------------------------------------------------------------

    public static Image Image(string name, Transform parent, Sprite sprite, Color color)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        if (sprite != null && sprite.border != Vector4.zero) img.type = UnityEngine.UI.Image.Type.Sliced;
        return img;
    }

    // Scale a 9-sliced image's corners so its border is about this many canvas units thick.
    public static void BorderSize(Image img, float units)
    {
        if (img.sprite == null || img.type != UnityEngine.UI.Image.Type.Sliced) return;
        Vector4 b = img.sprite.border;
        float biggest = Mathf.Max(Mathf.Max(b.x, b.z), Mathf.Max(b.y, b.w));
        img.pixelsPerUnitMultiplier = biggest / Mathf.Max(1f, units);
    }

    // A background that covers the whole parent at any aspect ratio: cropped at the edges, never stretched.
    public static Image Cover(string name, Transform parent, Sprite sprite)
    {
        var holder = Stretch(Rect(name, parent));
        holder.gameObject.AddComponent<RectMask2D>();
        var img = Image("Image", holder, sprite, sprite != null ? Color.white : new Color(0.08f, 0.06f, 0.1f));
        var fitter = img.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite != null ? sprite.rect.width / sprite.rect.height : 16f / 9f;
        return img;
    }

    // The wooden frame panel (or a dark box if the art is missing).
    public static Image Panel(string name, Transform parent, float border = 48f)
    {
        var img = Image(name, parent, Art != null ? Art.panelFrame : null, Art != null && Art.panelFrame != null ? Color.white : new Color(0.1f, 0.08f, 0.1f, 0.95f));
        BorderSize(img, border);
        return img;
    }

    // A grey box with the missing image's name on it, so a scene still runs without its art.
    public static Image Placeholder(string name, Transform parent, string label)
    {
        var img = Image(name, parent, null, new Color(0.25f, 0.22f, 0.28f, 0.9f));
        var text = Text("Label", img.transform, "missing:\n" + label, 34, Font.Body, TextDim, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, 10f);
        return img;
    }

    // --- Text ---------------------------------------------------------------------------------------------------

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Font font, Color color, TextAlignmentOptions align)
    {
        var rt = Rect(name, parent);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        var asset = FontAsset(font);
        if (asset != null)
        {
            tmp.font = asset;
            tmp.fontSharedMaterial = OutlinedMaterial(font, asset);
        }
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    static TMP_FontAsset FontAsset(Font font)
    {
        if (Art == null) return null;
        switch (font)
        {
            case Font.Title: return Art.titleFont;
            case Font.Italic: return Art.italicFont != null ? Art.italicFont : Art.bodyFont;
            default: return Art.bodyFont;
        }
    }

    // Light text with a dark ink outline, shared by every text using that font.
    static Material OutlinedMaterial(Font font, TMP_FontAsset asset)
    {
        bool title = font == Font.Title;
        ref Material cached = ref title ? ref titleMaterial : ref bodyMaterial;
        if (font == Font.Italic) return asset.material; // narration: a soft shadow is enough, set per text
        if (cached == null || cached.mainTexture != asset.material.mainTexture)
        {
            cached = new Material(asset.material) { name = asset.name + " Outlined" };
            cached.EnableKeyword(ShaderUtilities.Keyword_Outline);
            cached.SetFloat(ShaderUtilities.ID_OutlineWidth, title ? 0.22f : 0.15f);
            cached.SetColor(ShaderUtilities.ID_OutlineColor, Ink);
            cached.SetFloat(ShaderUtilities.ID_FaceDilate, title ? 0.12f : 0.05f);
        }
        return cached;
    }

    // --- Controls -----------------------------------------------------------------------------------------------

    public static MenuButton Button(string name, Transform parent, string label, Vector2 size, Action onClick, float fontSize = 46f)
    {
        bool hasArt = Art != null && Art.buttonNormal != null;
        var img = Image(name, parent, hasArt ? Art.buttonNormal : null, hasArt ? Color.white : new Color(0.25f, 0.16f, 0.1f, 0.95f));
        img.raycastTarget = true;
        img.rectTransform.sizeDelta = size;
        BorderSize(img, size.y * 0.4f);

        var text = Text("Label", img.transform, label, fontSize, Font.Title, TextLight, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(size.y * 0.5f, 0f);
        text.rectTransform.offsetMax = new Vector2(-size.y * 0.5f, -size.y * 0.04f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = fontSize;

        var button = img.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = img;
        var menuButton = img.gameObject.AddComponent<MenuButton>();
        menuButton.Setup(img, text, hasArt ? Art.buttonNormal : null, hasArt ? Art.buttonHover : null);
        button.onClick.AddListener(() => { PlayClick(); onClick?.Invoke(); });
        return menuButton;
    }

    public static Slider Slider(string name, Transform parent, Vector2 size, float min, float max, float value, Action<float> onChange)
    {
        var root = Rect(name, parent);
        root.sizeDelta = size;

        var track = Image("Track", root, Art != null ? Art.sliderTrack : null, Art != null && Art.sliderTrack != null ? Color.white : new Color(0.15f, 0.1f, 0.08f));
        Stretch(track.rectTransform);
        track.rectTransform.offsetMin = new Vector2(0f, size.y * 0.25f);
        track.rectTransform.offsetMax = new Vector2(0f, -size.y * 0.25f);
        BorderSize(track, size.y * 0.5f);

        float knob = size.y * 1.1f;
        var area = Stretch(Rect("Handle Area", root));
        area.offsetMin = new Vector2(knob / 2f, 0f);
        area.offsetMax = new Vector2(-knob / 2f, 0f);
        var handle = Image("Knob", area, Art != null ? Art.sliderKnob : null, Art != null && Art.sliderKnob != null ? Color.white : Gold);
        handle.preserveAspect = true;
        handle.raycastTarget = true;
        handle.rectTransform.sizeDelta = new Vector2(knob, 0f);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.transition = Selectable.Transition.ColorTint;
        var colors = slider.colors;
        colors.highlightedColor = new Color(1f, 0.92f, 0.7f);
        colors.pressedColor = new Color(0.85f, 0.75f, 0.55f);
        slider.colors = colors;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        // Clicking the track also moves the knob.
        track.raycastTarget = true;
        slider.onValueChanged.AddListener(v => onChange?.Invoke(v));
        return slider;
    }

    // --- Sound --------------------------------------------------------------------------------------------------

    public static void PlayClick() => Play(ref click, () => ToneGenerator.Thud("UiClick", 180f, 0.09f), 0.6f);
    public static void PlayHover() => Play(ref hover, () => ToneGenerator.Thud("UiHover", 420f, 0.04f), 0.18f);

    static void Play(ref AudioClip clip, Func<AudioClip> make, float volume)
    {
        if (clip == null) clip = make();
        PlaySound(clip, volume);
    }

    // A non-positional sound for menus and the intro (follows the SFX volume, plays while paused).
    public static void PlaySound(AudioClip clip, float volume)
    {
        if (audio == null)
        {
            var go = new GameObject("UI Sound");
            audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;
            audio.ignoreListenerPause = true; // still clicks while the game is paused
        }
        audio.PlayOneShot(clip, volume * GameSettings.Sfx);
    }
}
