using UnityEngine;

// What survives between runs: banked gold and number of runs. Saved with PlayerPrefs.
public static class MetaProgress
{
    const string GoldKey = "GrandDescent.BankedGold";
    const string RunsKey = "GrandDescent.TotalRuns";

    public static int BankedGold => PlayerPrefs.GetInt(GoldKey, 0);
    public static int TotalRuns => PlayerPrefs.GetInt(RunsKey, 0);

    public static void RecordRun(int goldBanked)
    {
        PlayerPrefs.SetInt(GoldKey, BankedGold + goldBanked);
        PlayerPrefs.SetInt(RunsKey, TotalRuns + 1);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(GoldKey);
        PlayerPrefs.DeleteKey(RunsKey);
        PlayerPrefs.Save();
    }
}
