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
}
