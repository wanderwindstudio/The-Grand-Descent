using UnityEngine;
using UnityEngine.Rendering;

// Dark ambient light, fog and torches.
public static class LevelLighting
{
    public static void Apply(TuningConfig t)
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = t.ambientColor;
        var probe = new SphericalHarmonicsL2();
        probe.AddAmbientLight(t.ambientColor);
        RenderSettings.ambientProbe = probe;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture = null;
        RenderSettings.reflectionIntensity = 0f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = t.fogColor;
        RenderSettings.fogDensity = t.fogDensity;
    }

    public static void AddTorch(GameObject parent, Vector3 position, TuningConfig t)
    {
        var flame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flame.name = "Torch";
        Object.DestroyImmediate(flame.GetComponent<Collider>());
        flame.transform.SetParent(parent.transform, false);
        flame.transform.localPosition = position;
        flame.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
        flame.GetComponent<Renderer>().sharedMaterial = Mats.Glow(Palette.Torch, 3f);

        var light = new GameObject("Torch Light").AddComponent<Light>();
        light.transform.SetParent(flame.transform, false);
        light.type = LightType.Point;
        light.color = t.torchColor;
        light.intensity = t.torchIntensity;
        light.range = t.torchRange;
    }
}
