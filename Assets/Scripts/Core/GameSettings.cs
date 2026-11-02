using System;
using System.IO;
using UnityEngine;

// Player settings (controls, video, audio, comfort). Saved in settings.json next to the save slots,
// separately from them. Changes apply live; Save() writes the file.
[Serializable]
public class GameSettings
{
    // Controls
    public float mouseSensitivity = 1f;  // multiplier on the tuning value (F1 panel)
    public bool invertY;
    public string bindingOverrides = ""; // Input System overrides, as JSON

    // Video
    public int resolutionWidth, resolutionHeight; // 0 = leave as it is
    public bool fullscreen = true;
    public bool vSync = true;
    public float fieldOfView = 80f;
    public float brightness;             // exposure offset, -1 to +1

    // Audio (0 to 1)
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;

    // Comfort (0 to 1). Each one scales every related effect.
    public float screenShake = 1f;
    public float buzzDistortion = 1f;
    public float headBob = 1f;

    public static GameSettings Current { get; private set; } = new GameSettings();

    static string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath)) Current = JsonUtility.FromJson<GameSettings>(File.ReadAllText(FilePath)) ?? new GameSettings();
            else Current = new GameSettings { fullscreen = Screen.fullScreenMode != FullScreenMode.Windowed }; // as launched
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not read settings.json, using defaults: " + e.Message);
            Current = new GameSettings();
        }
    }

    public static void Save()
    {
        try { File.WriteAllText(FilePath, JsonUtility.ToJson(Current, true)); }
        catch (Exception e) { Debug.LogWarning("Could not write settings.json: " + e.Message); }
    }

    // Everything that isn't read every frame by the systems that use it.
    public static void Apply()
    {
        var s = Current;
        AudioListener.volume = s.masterVolume;
        QualitySettings.vSyncCount = s.vSync ? 1 : 0;
        if (!Application.isEditor)
        {
            var mode = s.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            int w = s.resolutionWidth > 0 ? s.resolutionWidth : Screen.width;
            int h = s.resolutionHeight > 0 ? s.resolutionHeight : Screen.height;
            if (w != Screen.width || h != Screen.height || mode != Screen.fullScreenMode) Screen.SetResolution(w, h, mode);
        }
    }

    // Shortcuts for the systems that scale by a setting.
    public static float Shake => Current.screenShake;
    public static float Distortion => Current.buzzDistortion;
    public static float Bob => Current.headBob;
    public static float Sfx => Current.sfxVolume;
    public static float Music => Current.musicVolume;
}
