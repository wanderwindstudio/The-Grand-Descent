using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.LowLevel;

// Part of Grand Descent > Create Prototype Scene: the TextMeshPro resources, the menu fonts,
// the trimmed menu art, Assets/Settings/UiArt.asset and the MainMenu scene.
public static class MenuSetup
{
    public const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    const string UiArtPath = "Assets/Settings/UiArt.asset";
    const string ControlsPath = "Assets/Settings/GrandDescentControls.inputactions";
    const string IntroScriptPath = "Assets/Data/Intro/intro_script.txt";
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    const string Story = "Assets/Art/Story/";

    // Free fonts (SIL Open Font License) in Assets/Fonts.
    const string TitleFontFile = "Assets/Fonts/PirataOne-Regular.ttf";
    const string BodyFontFile = "Assets/Fonts/IMFeENrm28P.ttf";
    const string ItalicFontFile = "Assets/Fonts/IMFeENit28P.ttf";

    // Every character the menus and intro may need: ASCII, Latin-1 (accents, umlauts) and typographic punctuation.
    static string Characters()
    {
        var sb = new System.Text.StringBuilder();
        for (char c = ' '; c <= '~'; c++) sb.Append(c);
        for (char c = ' '; c <= 'ÿ'; c++) sb.Append(c);
        sb.Append("—–‘’“”…•€");
        return sb.ToString();
    }

    [MenuItem("Grand Descent/Import TextMeshPro Resources")]
    public static void ImportTmpEssentials()
    {
        if (File.Exists(TmpSettingsPath)) return;
        var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
        string package = Path.Combine(info.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
        AssetDatabase.ImportPackage(package, false);
        Debug.Log("Imported TextMeshPro essential resources.");
    }

    public static UiArt Setup()
    {
        if (!File.Exists(TmpSettingsPath))
            Debug.LogWarning("TextMeshPro resources are missing: run Grand Descent > Import TextMeshPro Resources first, then this again.");
        MenuArtPrep.PrepareAll();

        var art = AssetDatabase.LoadAssetAtPath<UiArt>(UiArtPath);
        if (art == null)
        {
            art = ScriptableObject.CreateInstance<UiArt>();
            AssetDatabase.CreateAsset(art, UiArtPath);
        }

        art.menuBackground = LoadSprite("Assets/Art/UI/Menu/ui_menu_bg_01.png");
        art.logo = LoadSprite(MenuArtPrep.OutputPath("ui_menu_logo_01"));
        art.buttonNormal = LoadSprite(MenuArtPrep.OutputPath("ui_button_normal_01"));
        art.buttonHover = LoadSprite(MenuArtPrep.OutputPath("ui_button_hover_01"));
        art.panelFrame = LoadSprite(MenuArtPrep.OutputPath("ui_panel_frame_01"));
        art.sliderTrack = LoadSprite(MenuArtPrep.OutputPath("ui_slider_track_01"));
        art.sliderKnob = LoadSprite(MenuArtPrep.OutputPath("ui_slider_knob_01"));
        art.saveSlotFrame = LoadSprite(MenuArtPrep.OutputPath("ui_saveSlot_frame_01"));

        if (File.Exists(TmpSettingsPath))
        {
            art.titleFont = FontAsset(TitleFontFile, "Assets/Fonts/PirataOne SDF.asset");
            art.bodyFont = FontAsset(BodyFontFile, "Assets/Fonts/IMFellEnglish SDF.asset");
            art.italicFont = FontAsset(ItalicFontFile, "Assets/Fonts/IMFellEnglish Italic SDF.asset");
        }

        art.introScript = AssetDatabase.LoadAssetAtPath<TextAsset>(IntroScriptPath);
        // Only filled in when empty, so ids and images added by hand are kept.
        if (art.storyBackgrounds.Count == 0)
        {
            AddStory(art.storyBackgrounds, "attic", null, "bg_story_attic_01");
            AddStory(art.storyBackgrounds, "tv", null, "bg_story_tvSkully_01");
            AddStory(art.storyBackgrounds, "desk", null, "bg_story_arenaDesk_01");
        }
        if (art.portraits.Count == 0)
        {
            AddStory(art.portraits, "stoned", "hero", "char_hero_stoned_01");
            AddStory(art.portraits, "startled", "hero", "char_hero_startled_01");
            AddStory(art.portraits, "determined", "hero", "char_hero_determined_01");
            AddStory(art.portraits, "angry", "grumbleton", "char_grumbleton_angry_01");
            AddStory(art.portraits, "notice", "grumbleton", "char_grumbleton_notice_01");
            AddStory(art.portraits, "host", "skully", "char_skully_host_01");
            AddStory(art.portraits, "clerk", "clerk", "char_goblinClerk_01");
        }
        else
        {
            foreach (var p in art.portraits) if (p.sprite == null) p.sprite = LoadSprite(Story + p.id + ".png");
        }
        EditorUtility.SetDirty(art);
        AssetDatabase.SaveAssets();
        return art;
    }

    static void AddStory(System.Collections.Generic.List<UiArt.StoryImage> list, string id, string character, string file)
    {
        list.Add(new UiArt.StoryImage { id = id, character = character, sprite = LoadSprite(Story + file + ".png") });
    }

    static Sprite LoadSprite(string path)
    {
        if (!File.Exists(path)) { Debug.LogWarning("Menu art missing: " + path); return null; }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); // so ArtImportSettings makes it a sprite
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // A TextMeshPro font asset with every needed character baked into its atlas (static: no changes at runtime).
    static TMP_FontAsset FontAsset(string fontFile, string assetPath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null) return existing;
        var font = AssetDatabase.LoadAssetAtPath<Font>(fontFile);
        if (font == null) { Debug.LogWarning("Font missing: " + fontFile); return null; }

        var asset = TMP_FontAsset.CreateFontAsset(font, 48, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        asset.name = Path.GetFileNameWithoutExtension(assetPath);
        asset.TryAddCharacters(Characters(), out string missing);
        if (!string.IsNullOrEmpty(missing)) Debug.Log($"{asset.name}: the font has no glyphs for \"{missing}\"");
        asset.atlasPopulationMode = AtlasPopulationMode.Static;

        AssetDatabase.CreateAsset(asset, assetPath);
        asset.atlasTexture.name = asset.name + " Atlas";
        AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
        asset.material.name = asset.name + " Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }

    // MainMenu.unity with just the MenuBootstrap object.
    public static void CreateMenuScene(UiArt art)
    {
        var scene = File.Exists(MenuScenePath)
            ? EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bootstrap = Object.FindAnyObjectByType<MenuBootstrap>();
        if (bootstrap == null) bootstrap = new GameObject("MenuBootstrap").AddComponent<MenuBootstrap>();
        bootstrap.art = art;
        bootstrap.controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        EditorUtility.SetDirty(bootstrap);
        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }
}
