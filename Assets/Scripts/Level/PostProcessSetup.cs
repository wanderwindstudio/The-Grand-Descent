using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Creates the global post-processing volume. Buzz effects get added here in milestone 3.
public static class PostProcessSetup
{
    public static VolumeProfile Profile { get; private set; }

    public static void Create(TuningConfig t, Camera cam)
    {
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

        var go = new GameObject("Post Processing");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        Profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = Profile;

        var bloom = Profile.Add<Bloom>(true);
        bloom.intensity.Override(t.bloomIntensity);
        bloom.threshold.Override(t.bloomThreshold);

        var vignette = Profile.Add<Vignette>(true);
        vignette.intensity.Override(t.vignetteIntensity);

        var tonemap = Profile.Add<Tonemapping>(true);
        tonemap.mode.Override(TonemappingMode.Neutral);
    }
}
