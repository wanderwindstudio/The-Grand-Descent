// Numbers collected during a run, for the run summary and the CSV log.
public static class RunStats
{
    public static int Kills;
    public static float SecondsAboveBuzzThreshold;
    public static int Transcendences;
    public static int VortexCasts;
    public static int MaxChargesReached;
    public static int Jumps;
    public static int RingOutKills;
    public static int BarrelKills;
    // Hangover mode
    public static float MaxTab, TabPaid, TabForgiven;
    public static int HangoverDeaths, PhantomsKilled, PhantomChargeResets, HangoverBillGold;

    public static void Reset()
    {
        VortexCasts = 0;
        MaxChargesReached = 0;
        Jumps = 0;
        RingOutKills = 0;
        BarrelKills = 0;
        MaxTab = TabPaid = TabForgiven = 0f;
        HangoverDeaths = PhantomsKilled = PhantomChargeResets = HangoverBillGold = 0;
        Kills = 0;
        SecondsAboveBuzzThreshold = 0f;
        Transcendences = 0;
    }
}
