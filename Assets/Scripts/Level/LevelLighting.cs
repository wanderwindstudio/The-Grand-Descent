using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Ambient light, fog and torches.
public static class LevelLighting
{
    static readonly List<Light> torches = new List<Light>();

    public static void Apply(TuningConfig t)
    {
        torches.Clear();
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture = null;
        RenderSettings.reflectionIntensity = 0f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        Refresh(t);
    }

    // Re-reads the lighting values. Called by the debug panel when sliders move.
    public static void Refresh(TuningConfig t)
    {
        Color ambient = t.ambientColor * t.ambientIntensity;
        RenderSettings.ambientLight = ambient;
        var probe = new SphericalHarmonicsL2();
        probe.AddAmbientLight(ambient);
        RenderSettings.ambientProbe = probe;

        RenderSettings.fogColor = t.fogColor;
        RenderSettings.fogDensity = t.fogDensity;
        if (Game.Camera != null) Game.Camera.backgroundColor = t.fogColor;

        foreach (var light in torches)
        {
            light.intensity = t.torchIntensity;
            light.range = t.torchRange;
        }
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
        torches.Add(light);
    }
}
