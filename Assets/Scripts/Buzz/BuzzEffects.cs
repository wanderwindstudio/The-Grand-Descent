using UnityEngine;

// Drives the screen effects from the current Buzz: colour fringing, lens warp, hue drift,
// a golden flare during Transcendence, and blur + darkness during the Crash.
public class BuzzEffects : MonoBehaviour
{
    float shown; // smoothed Buzz, so effects don't jump

    void Update()
    {
        var t = Game.Tuning;
        var buzz = Game.Buzz;
        shown = Mathf.MoveTowards(shown, buzz.Normalized, Time.deltaTime * 2f);

        float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * t.buzzPulseSpeed);
        float k = shown * pulse;

        bool transcendent = buzz.Current == BuzzMeter.Phase.Transcendent;
        bool crash = buzz.Current == BuzzMeter.Phase.Crash;

        PostProcessSetup.Chromatic.intensity.value = Mathf.Clamp01(t.buzzMaxChromaticAberration * k);
        PostProcessSetup.Lens.intensity.value = -t.buzzMaxLensDistortion * k;
        PostProcessSetup.Colors.hueShift.value = t.buzzMaxHueShift * shown * Mathf.Sin(Time.time * t.buzzPulseSpeed * 0.5f);

        float saturation = t.buzzSaturationBoost * shown;
        float exposure = 0f;
        float bloom = t.bloomIntensity;
        float vignette = t.vignetteIntensity;
        Color filter = Color.white;

        if (transcendent)
        {
            exposure = t.transcendenceExposure * pulse;
            bloom = t.transcendenceBloom;
            filter = Palette.TranscendenceTint;
        }
        if (crash)
        {
            // Fade back to normal over the last second of the Crash.
            float fade = Mathf.Clamp01(buzz.PhaseTimeLeft);
            exposure = t.crashExposure * fade;
            saturation = t.crashSaturation * fade;
            vignette = Mathf.Lerp(t.vignetteIntensity, t.crashVignette, fade);
            PostProcessSetup.Blur.gaussianMaxRadius.value = t.crashBlur * fade;
        }
        PostProcessSetup.Blur.active = crash;

        PostProcessSetup.Colors.saturation.value = saturation;
        PostProcessSetup.Colors.postExposure.value = exposure;
        PostProcessSetup.Colors.colorFilter.value = filter;
        PostProcessSetup.Bloom.intensity.value = bloom;
        PostProcessSetup.Vignette.intensity.value = vignette;
    }
}
