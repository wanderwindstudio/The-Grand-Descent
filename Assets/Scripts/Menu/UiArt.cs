using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Menu, settings and intro art in one place (Assets/Settings/UiArt.asset). Filled in by
// Grand Descent > Create Prototype Scene; both scenes' bootstraps hand it over. Anything missing gets a placeholder.
[CreateAssetMenu(fileName = "UiArt", menuName = "Grand Descent/UI Art")]
public class UiArt : ScriptableObject
{
    public static UiArt Current;

    [Serializable]
    public class StoryImage
    {
        [Tooltip("The id used in the intro script (scene column or portrait column).")]
        public string id;
        [Tooltip("Portraits only: who this is. Portraits of the same character crossfade; the hero stands left, everyone else right.")]
        public string character;
        public Sprite sprite;
    }

    [Header("Menu (trimmed copies from Assets/Art/Generated/UI)")]
    public Sprite menuBackground;
    public Sprite logo;
    public Sprite buttonNormal;
    public Sprite buttonHover;
    public Sprite panelFrame;
    public Sprite sliderTrack;
    public Sprite sliderKnob;
    public Sprite saveSlotFrame;

    [Header("Fonts")]
    [Tooltip("Buttons, titles and names (Pirata One).")]
    public TMP_FontAsset titleFont;
    [Tooltip("Dialogue and small text (IM Fell English).")]
    public TMP_FontAsset bodyFont;
    [Tooltip("Narration (IM Fell English Italic).")]
    public TMP_FontAsset italicFont;

    [Header("Intro")]
    [Tooltip("Assets/Data/Intro/intro_script.txt. Format per line: scene | speaker | portrait | line")]
    public TextAsset introScript;
    public List<StoryImage> storyBackgrounds = new List<StoryImage>();
    public List<StoryImage> portraits = new List<StoryImage>();

    public StoryImage Background(string id) => storyBackgrounds.Find(b => b.id == id);
    public StoryImage Portrait(string id) => portraits.Find(p => p.id == id);
}
