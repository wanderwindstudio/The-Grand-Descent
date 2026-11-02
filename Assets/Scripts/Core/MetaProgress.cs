// What survives between runs: banked gold, run count and best floor. Lives in the active save slot.
// A test run (game scene started directly, no slot) keeps it in memory only.
public static class MetaProgress
{
    static readonly MetaSave testRun = new MetaSave();
    static MetaSave Meta => SaveSystem.Active != null ? SaveSystem.Active.meta : testRun;

    public static int BankedGold => Meta.bankedGold;
    public static int TotalRuns => Meta.totalRuns;
    public static int BestFloor => Meta.bestFloor;

    public static void ReachedFloor(int floor)
    {
        if (floor > Meta.bestFloor) Meta.bestFloor = floor;
    }

    // The run is over (died or extracted): bank the gold and clear the run part of the save.
    public static void RecordRun(int goldBanked, int floor)
    {
        Meta.bankedGold += goldBanked;
        Meta.totalRuns++;
        ReachedFloor(floor);
        if (SaveSystem.Active != null) SaveSystem.Active.hasRun = false;
        SaveSystem.SaveActive();
    }

    // Debug panel: wipe banked gold, runs and best floor (the intro stays seen).
    public static void ResetAll()
    {
        Meta.bankedGold = 0;
        Meta.totalRuns = 0;
        Meta.bestFloor = 0;
        SaveSystem.SaveActive();
    }
}
