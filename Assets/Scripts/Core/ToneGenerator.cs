using UnityEngine;

// Makes placeholder sounds in code.
public static class ToneGenerator
{
    const int SampleRate = 44100;

    public static AudioClip Beep(string name, float frequency, float duration)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)SampleRate;
            float fade = 1f - i / (float)count;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade;
        }
        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Two quick rising blips with a soft edge: "bip-BIP". Easy to locate by ear.
    public static AudioClip WarningChirp(string name, float frequency)
    {
        const float blip = 0.07f, gap = 0.04f;
        int blipSamples = Mathf.RoundToInt(SampleRate * blip);
        int gapSamples = Mathf.RoundToInt(SampleRate * gap);
        var data = new float[blipSamples * 2 + gapSamples];
        for (int b = 0; b < 2; b++)
        {
            float f = frequency * (b == 0 ? 1f : 1.5f);
            int offset = b * (blipSamples + gapSamples);
            for (int i = 0; i < blipSamples; i++)
            {
                float t = i / (float)SampleRate;
                float edge = Mathf.Clamp01(Mathf.Min(i, blipSamples - i) / (SampleRate * 0.008f)); // no clicks
                // A little square-ish harmonic makes it cut through the fight noise.
                float wave = Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(6f * Mathf.PI * f * t);
                data[offset + i] = wave * 0.7f * edge;
            }
        }
        var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // A seamless low rumble for looping (filtered noise plus a deep hum).
    public static AudioClip Rumble(string name)
    {
        int count = SampleRate * 2;
        var data = new float[count];
        var rng = new System.Random(7);
        float smooth = 0f;
        for (int i = 0; i < count; i++)
        {
            float noise = (float)rng.NextDouble() * 2f - 1f;
            smooth += (noise - smooth) * 0.02f; // low-pass
            float t = i / (float)SampleRate;
            float hum = Mathf.Sin(2f * Mathf.PI * 45f * t) * 0.3f; // deep hum under the noise
            data[i] = Mathf.Clamp(smooth * 6f + hum, -1f, 1f);
        }
        // Cross-fade the ends so the loop has no click.
        int fade = SampleRate / 10;
        for (int i = 0; i < fade; i++)
        {
            float k = i / (float)fade;
            data[i] = data[i] * k + data[count - fade + i] * (1f - k);
        }
        var clip = AudioClip.Create(name, count - fade, 1, SampleRate, false);
        var trimmed = new float[count - fade];
        System.Array.Copy(data, trimmed, trimmed.Length);
        clip.SetData(trimmed, 0);
        return clip;
    }

    // A soft, low thump (landing): a falling-pitch sine with a little noise, fading out fast.
    public static AudioClip Thud(string name, float frequency, float duration)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        var data = new float[count];
        var rng = new System.Random(3);
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float k = i / (float)count;
            phase += 2f * Mathf.PI * frequency * (1f - 0.5f * k) / SampleRate;
            float env = Mathf.Exp(-k * 6f) * Mathf.Clamp01(i / (SampleRate * 0.003f));
            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.25f * Mathf.Exp(-k * 20f);
            data[i] = (Mathf.Sin(phase) + noise) * env * 0.8f;
        }
        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // An explosion: a noisy boom that drops in pitch.
    public static AudioClip Boom(string name)
    {
        int count = SampleRate * 7 / 10;
        var data = new float[count];
        var rng = new System.Random(11);
        float smooth = 0f, phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float k = i / (float)count;
            float noise = (float)rng.NextDouble() * 2f - 1f;
            smooth += (noise - smooth) * Mathf.Lerp(0.5f, 0.05f, k); // gets duller as it fades
            phase += 2f * Mathf.PI * Mathf.Lerp(70f, 35f, k) / SampleRate;
            float env = Mathf.Exp(-k * 5f);
            data[i] = Mathf.Clamp((smooth * 2.5f + Mathf.Sin(phase) * 0.6f) * env, -1f, 1f);
        }
        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
