using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Creates the global post-processing volume. BuzzEffects changes these settings every frame.
public static class PostProcessSetup
{
    public static VolumeProfile Profile { get; private set; }
    public static Bloom Bloom { get; private set; }
    public static Vignette Vignette { get; private set; }
    public static ChromaticAberration Chromatic { get; private set; }
    public static LensDistortion Lens { get; private set; }
    public static ColorAdjustments Colors { get; private set; }
    public static DepthOfField Blur { get; private set; }

    public static void Create(TuningConfig t, Camera cam)
    {
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

        var go = new GameObject("Post Processing");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        Profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = Profile;

        Bloom = Profile.Add<Bloom>(true);
        Bloom.intensity.Override(t.bloomIntensity);
        Bloom.threshold.Override(t.bloomThreshold);

        Vignette = Profile.Add<Vignette>(true);
        Vignette.intensity.Override(t.vignetteIntensity);

        var tonemap = Profile.Add<Tonemapping>(true);
        tonemap.mode.Override(TonemappingMode.Neutral);

        Chromatic = Profile.Add<ChromaticAberration>(true);
        Chromatic.intensity.Override(0f);

        Lens = Profile.Add<LensDistortion>(true);
        Lens.intensity.Override(0f);

        Colors = Profile.Add<ColorAdjustments>(true);
        Colors.hueShift.Override(0f);
        Colors.saturation.Override(0f);
        Colors.postExposure.Override(0f);
        Colors.colorFilter.Override(Color.white);

        Blur = Profile.Add<DepthOfField>(true);
        Blur.mode.Override(DepthOfFieldMode.Gaussian);
        Blur.gaussianStart.Override(0f);
        Blur.gaussianEnd.Override(3f);
        Blur.gaussianMaxRadius.Override(0f);
        Blur.active = false;
    }
}
