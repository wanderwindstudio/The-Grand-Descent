using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Grand Descent > Create Prototype Scene.
// Makes the TuningConfig asset and base material (if missing) and a scene with just the GameBootstrap object.
public static class PrototypeSceneSetup
{
    const string ConfigPath = "Assets/Settings/TuningConfig.asset";
    const string MaterialPath = "Assets/Settings/GrayboxLit.mat";
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
            material.EnableKeyword("_EMISSION"); // so the glow variant is kept in builds
            material.SetColor("_EmissionColor", Color.black);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("GameBootstrap");
        var bootstrap = go.AddComponent<GameBootstrap>();
        bootstrap.config = config;
        bootstrap.baseMaterial = material;
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Prototype scene created at " + ScenePath);
    }
}
