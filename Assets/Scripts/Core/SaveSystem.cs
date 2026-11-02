using System;
using System.IO;
using UnityEngine;

// Three save slots, one JSON file each (slot1.json ... slot3.json in Application.persistentDataPath).
// The menu picks a slot; the game scene reads and writes that slot.
public static class SaveSystem
{
    public const int SlotCount = 3;

    // 0-based slot in use, or -1 (the game scene was started directly: a test run, nothing is saved).
    public static int ActiveSlot { get; private set; } = -1;
    public static SaveData Active { get; private set; }

    public static string Folder => Application.persistentDataPath;
    public static string PathOf(int slot) => Path.Combine(Folder, $"slot{slot + 1}.json");

    public static bool Exists(int slot) => File.Exists(PathOf(slot));

    public static SaveData Load(int slot)
    {
        if (!Exists(slot)) return null;
        try
        {
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathOf(slot)));
            return Migrate(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Save slot {slot + 1} could not be read: {e.Message}");
            return null;
        }
    }

    public static void Write(int slot, SaveData data)
    {
        data.version = SaveData.CurrentVersion;
        data.savedAt = DateTime.Now.ToString("s");
        try { File.WriteAllText(PathOf(slot), JsonUtility.ToJson(data, true)); }
        catch (Exception e) { Debug.LogWarning($"Could not write save slot {slot + 1}: {e.Message}"); }
    }

    public static void Delete(int slot)
    {
        if (Exists(slot)) File.Delete(PathOf(slot));
        if (slot == ActiveSlot) { ActiveSlot = -1; Active = null; }
    }

    // Use this slot from now on (an empty slot starts fresh).
    public static void Select(int slot, bool startFresh)
    {
        ActiveSlot = slot;
        Active = startFresh ? null : Load(slot);
        if (Active == null)
        {
            Active = new SaveData();
            Write(slot, Active);
        }
    }

    public static void SaveActive()
    {
        if (ActiveSlot >= 0 && Active != null) Write(ActiveSlot, Active);
    }

    // The slot with a run in progress that was saved most recently, or -1.
    public static int LatestRunSlot()
    {
        int best = -1;
        DateTime bestTime = DateTime.MinValue;
        for (int i = 0; i < SlotCount; i++)
        {
            var d = Load(i);
            if (d == null || !d.hasRun) continue;
            if (best < 0 || d.SavedAt > bestTime) { best = i; bestTime = d.SavedAt; }
        }
        return best;
    }

    // Older formats get upgraded here, one version at a time.
    static SaveData Migrate(SaveData data)
    {
        if (data == null) return null;
        if (data.meta == null) data.meta = new MetaSave();
        if (data.run == null) { data.run = new RunSave(); data.hasRun = false; }
        // (version 1 is the first format; nothing to do yet)
        data.version = SaveData.CurrentVersion;
        return data;
    }

    // Banked gold and run count used to live in PlayerPrefs. Moves them into slot 1 once.
    public static void MigratePlayerPrefs()
    {
        const string GoldKey = "GrandDescent.BankedGold", RunsKey = "GrandDescent.TotalRuns";
        if (!PlayerPrefs.HasKey(GoldKey) && !PlayerPrefs.HasKey(RunsKey)) return;
        var data = Load(0) ?? new SaveData();
        data.meta.bankedGold += PlayerPrefs.GetInt(GoldKey, 0);
        data.meta.totalRuns += PlayerPrefs.GetInt(RunsKey, 0);
        data.meta.introSeen = true; // they've played before
        Write(0, data);
        PlayerPrefs.DeleteKey(GoldKey);
        PlayerPrefs.DeleteKey(RunsKey);
        PlayerPrefs.Save();
        Debug.Log($"Moved old progress (banked gold {data.meta.bankedGold}, runs {data.meta.totalRuns}) into save slot 1.");
    }

    // Cross-scene hand-over: what the game scene should do when it loads.
    public static bool ResumeRun => Active != null && Active.hasRun;
}
