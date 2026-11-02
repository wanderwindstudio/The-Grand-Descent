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

    // position = where the flame is. With torch art, the iron torch sprite hangs flat on the nearest wall
    // (or faces the camera if there's no wall close by); without it, a glowing block.
    public static void AddTorch(GameObject parent, Vector3 position, TuningConfig t)
    {
        var torch = new GameObject("Torch");
        torch.transform.SetParent(parent.transform, false);
        torch.transform.position = position;

        var art = EnvironmentArt.Current;
        Vector3 lightOffset = Vector3.zero;
        if (art != null && art.torchWall != null && PropSprites.Ready) lightOffset = AddTorchSprite(torch.transform, position, art.torchWall, t);
        else
        {
            var flame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flame.name = "Flame";
            Object.DestroyImmediate(flame.GetComponent<Collider>());
            flame.transform.SetParent(torch.transform, false);
            flame.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
            flame.GetComponent<Renderer>().sharedMaterial = Mats.Glow(Palette.Torch, 3f);
        }

        var light = new GameObject("Torch Light").AddComponent<Light>();
        light.transform.SetParent(torch.transform, false);
        light.transform.localPosition = lightOffset;
        light.type = LightType.Point;
        light.color = t.torchColor;
        light.intensity = t.torchIntensity;
        light.range = t.torchRange;
        light.gameObject.AddComponent<TorchFlicker>();
        torches.Add(light);
    }

    // Returns where the light goes (relative to the flame): a little out from the wall, so the
    // sprite right next to it isn't blown out.
    static Vector3 AddTorchSprite(Transform torch, Vector3 flame, Texture2D texture, TuningConfig t)
    {
        float size = t.torchSpriteSize;
        // The flame sits in the top fifth of the art: drop the canvas so the flame is at the light.
        Vector3 bottom = flame + Vector3.down * (size * 0.82f);
        if (FindWall(flame, out RaycastHit wall))
        {
            Vector3 normal = new Vector3(wall.normal.x, 0f, wall.normal.z).normalized;
            Vector3 onWall = new Vector3(wall.point.x, flame.y, wall.point.z);
            var sprite = PropSprites.Decal("Torch Sprite", torch, onWall + normal * 0.1f + Vector3.down * (size * 0.32f), normal, texture, size, size);
            sprite.sharedMaterial = TorchMaterial(sprite.sharedMaterial, t);
            return onWall + normal * 0.75f - flame;
        }
        PropSprites.Billboard("Torch Sprite", torch, bottom, texture, size, out _);
        return Vector3.zero;
    }

    // The torch hangs right next to its own light, which would wash it out: a darker copy of the material.
    static Material torchMaterial;
    static Material TorchMaterial(Material decal, TuningConfig t)
    {
        if (torchMaterial != null) return torchMaterial;
        torchMaterial = new Material(decal) { name = "Torch Sprite" };
        torchMaterial.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f));
        return torchMaterial;
    }

    // The closest wall within reach of a torch spot, looking in the four flat directions.
    static bool FindWall(Vector3 from, out RaycastHit best)
    {
        best = default;
        bool found = false;
        foreach (var dir in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
            if (Physics.Raycast(from, dir, out RaycastHit hit, 0.8f, ~(1 << 2), QueryTriggerInteraction.Ignore) && (!found || hit.distance < best.distance))
            {
                best = hit;
                found = true;
            }
        return found;
    }
}
