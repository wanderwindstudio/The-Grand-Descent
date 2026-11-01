using UnityEngine;

// Shared references every system can reach. Filled in by GameBootstrap.
public static class Game
{
    public static TuningConfig Tuning;      // live copy, changed by the debug panel
    public static TuningConfig TuningAsset; // the asset on disk
    public static Transform Player;
    public static PlayerHealth PlayerHealth;
    public static Camera Camera;
    public static BuzzMeter Buzz;
    public static ArcaneCharges Charges;
    public static int Floor = 1;
    public static bool GodMode;
    public static bool InputBlocked; // true while the debug panel is open
}
