using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

// Menu: Grand Descent > Create Prototype Scene.
// Makes the TuningConfig asset and materials (if missing), the starter rooms (if missing),
// and a scene with just the GameBootstrap object.
// Safe to run again: it reuses the existing scene and only refreshes the bootstrap's references.
public static class PrototypeSceneSetup
{
    const string ConfigPath = "Assets/Settings/TuningConfig.asset";
    const string MaterialPath = "Assets/Settings/GrayboxLit.mat";
    const string BillboardMaterialPath = "Assets/Art/Materials/mat_billboardCutout.mat";
    const string GoblinSpritePath = "Assets/Art/Enemies/enemy_goblinIntern_idle_01.png";
    const string GoblinWindUpPath = "Assets/Art/Enemies/enemy_goblinIntern_windup_01.png";
    const string GoblinHitPath = "Assets/Art/Enemies/enemy_goblinIntern_hit_01.png";
    const string GoblinDeadPath = "Assets/Art/Enemies/enemy_goblinIntern_dead_01.png";
    const string FxMaterialPath = "Assets/Art/Materials/mat_fxTransparent.mat";
    const string ControlsPath = "Assets/Settings/GrandDescentControls.inputactions";
    const string HandLeftPath = "Assets/Art/Player/player_handLeft_idle_01.png";
    const string HandLeftCastPath = "Assets/Art/Player/player_handLeft_cast_01.png";
    const string HandRightPath = "Assets/Art/Player/player_handRight_staff_idle_01.png";
    const string HandRightSlamPath = "Assets/Art/Player/player_handRight_staff_slam_01.png";
    const string IconMissilePath = "Assets/Art/UI/icon_spell_arcaneMissile.png";
    const string IconVortexPath = "Assets/Art/UI/icon_spell_arcaneVortex.png";
    const string IconExplosionPath = "Assets/Art/UI/icon_spell_arcaneExplosion.png";
    const string IconChargePath = "Assets/Art/UI/icon_hud_arcaneCharge.png";
    const string FxMissilePath = "Assets/Art/FX/fx_arcaneMissile_01.png";
    const string FxImpactPath = "Assets/Art/FX/fx_arcaneImpact_01.png";
    const string FxVortexPath = "Assets/Art/FX/fx_arcaneVortex_01.png";
    const string ScenePath = "Assets/Scenes/Prototype.unity";

    [MenuItem("Grand Descent/Create Prototype Scene")]
    public static void CreateScene()
    {
        var config = AssetDatabase.LoadAssetAtPath<TuningConfig>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<TuningConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        KeepEmission(material);

        var billboard = AssetDatabase.LoadAssetAtPath<Material>(BillboardMaterialPath);
        if (billboard == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials")) AssetDatabase.CreateFolder("Assets/Art", "Materials");
            billboard = CreateBillboardMaterial();
            AssetDatabase.CreateAsset(billboard, BillboardMaterialPath);
        }
        KeepEmission(billboard);

        var fx = AssetDatabase.LoadAssetAtPath<Material>(FxMaterialPath);
        if (fx == null)
        {
            fx = CreateFxMaterial();
            AssetDatabase.CreateAsset(fx, FxMaterialPath);
        }

        if (!StarterRooms.Exist()) StarterRooms.BuildAll();
        var roomLibrary = RoomLibraryUpdater.Refresh();

        var goblinSprite = LoadArt(GoblinSpritePath);
        if (goblinSprite == null) Debug.LogWarning("Goblin sprite not found at " + GoblinSpritePath);

        var scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
        if (bootstrap == null) bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        bootstrap.config = config;
        bootstrap.baseMaterial = material;
        bootstrap.billboardMaterial = billboard;
        bootstrap.goblinSprite = goblinSprite;
        bootstrap.goblinWindUpSprite = LoadArt(GoblinWindUpPath);
        bootstrap.goblinHitSprite = LoadArt(GoblinHitPath);
        bootstrap.goblinDeadSprite = LoadArt(GoblinDeadPath);
        bootstrap.fxMaterial = fx;
        bootstrap.controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        bootstrap.roomLibrary = roomLibrary;
        bootstrap.handLeft = LoadArt(HandLeftPath);
        bootstrap.handLeftCast = LoadArt(HandLeftCastPath);
        bootstrap.handRight = LoadArt(HandRightPath);
        bootstrap.handRightSlam = LoadArt(HandRightSlamPath);
        bootstrap.iconArcaneMissile = LoadArt(IconMissilePath);
        bootstrap.iconArcaneVortex = LoadArt(IconVortexPath);
        bootstrap.iconArcaneExplosion = LoadArt(IconExplosionPath);
        bootstrap.iconArcaneCharge = LoadArt(IconChargePath);
        bootstrap.fxArcaneMissile = LoadArt(FxMissilePath);
        bootstrap.fxArcaneImpact = LoadArt(FxImpactPath);
        bootstrap.fxArcaneVortex = LoadArt(FxVortexPath);
        if (bootstrap.controls == null) Debug.LogWarning("Controls asset not found at " + ControlsPath + " - using built-in default bindings.");
        EditorUtility.SetDirty(bootstrap);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Prototype scene ready at " + ScenePath);
    }

    // Re-imports an art file (so ArtImportSettings applies) and loads it. Null if the file isn't there.
    static Texture2D LoadArt(string path)
    {
        if (!System.IO.File.Exists(path)) return null;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // URP Lit with alpha clipping: pixels are fully visible or fully gone, so sprites are lit by torches,
    // fade into fog and sort correctly behind walls. Double-sided, with emission for hit flashes.
    static Material CreateBillboardMaterial()
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetFloat("_Surface", 0f);
        m.SetFloat("_AlphaClip", 1f);
        m.SetFloat("_Cutoff", 0.5f);
        m.EnableKeyword("_ALPHATEST_ON");
        m.SetOverrideTag("RenderType", "TransparentCutout");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        m.SetFloat("_Cull", 0f);
        m.SetFloat("_Smoothness", 0f);
        m.SetFloat("_SpecularHighlights", 0f);
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        m.SetFloat("_EnvironmentReflections", 0f);
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        return m;
    }

    // URP Unlit, alpha-blended, double-sided: for spell effects that glow and fade out.
    static Material CreateFxMaterial()
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return m;
    }

    // URP switches emission off on any material whose emission colour is pure black, and builds then
    // leave out the glow shader. A barely-there emission colour keeps it on; the game sets the real glow.
    static void KeepEmission(Material m)
    {
        m.SetColor("_EmissionColor", new Color(0.002f, 0.002f, 0.002f));
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        EditorUtility.SetDirty(m);
    }
}
