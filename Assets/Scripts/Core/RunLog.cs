using System;
using System.Globalization;
using System.IO;
using UnityEngine;

// Appends one line per run to run_log.csv in Application.persistentDataPath.
public static class RunLog
{
    const string Header = "date_time,outcome,floor_reached,duration_seconds,kills,gold_carried,gold_banked,level_reached,upgrades_taken,seconds_above_70_buzz,transcendences,vortex_casts,max_charges_reached,arcane_upgrades_taken";

    public static string FilePath => Path.Combine(Application.persistentDataPath, "run_log.csv");

    public static void Append(string outcome, int floor, float duration, int kills, int goldCarried, int goldBanked, int level, string upgrades,
        float secondsHighBuzz, int transcendences, int vortexCasts, int maxChargesReached, string arcaneUpgrades)
    {
        var inv = CultureInfo.InvariantCulture;
        string line = string.Join(",",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", inv),
            outcome,
            floor.ToString(inv),
            duration.ToString("0.0", inv),
            kills.ToString(inv),
            goldCarried.ToString(inv),
            goldBanked.ToString(inv),
            level.ToString(inv),
            "\"" + upgrades + "\"",
            secondsHighBuzz.ToString("0.0", inv),
            transcendences.ToString(inv),
            vortexCasts.ToString(inv),
            maxChargesReached.ToString(inv),
            "\"" + arcaneUpgrades + "\"");
        try
        {
            KeepOldLogIfColumnsChanged();
            bool isNew = !File.Exists(FilePath);
            using (var writer = new StreamWriter(FilePath, append: true))
            {
                if (isNew) writer.WriteLine(Header);
                writer.WriteLine(line);
            }
            Debug.Log("Run logged to " + FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not write run log: " + e.Message);
        }
    }

    // A log written with older columns is renamed (run_log_before_<date>.csv) so rows never get mixed up.
    static void KeepOldLogIfColumnsChanged()
    {
        if (!File.Exists(FilePath)) return;
        string firstLine;
        using (var reader = new StreamReader(FilePath)) firstLine = reader.ReadLine();
        if (firstLine == Header) return;
        string old = Path.Combine(Application.persistentDataPath, $"run_log_before_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.Move(FilePath, old);
        Debug.Log("Run log columns changed; the old log was kept as " + old);
    }
}
