// Numbers collected during a run, for the run summary and the CSV log.
public static class RunStats
{
    public static int Kills;
    public static float SecondsAboveBuzzThreshold;
    public static int Transcendences;

    public static void Reset()
    {
        Kills = 0;
        SecondsAboveBuzzThreshold = 0f;
        Transcendences = 0;
    }
}
