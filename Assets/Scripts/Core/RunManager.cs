using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// One run from start screen to summary: builds floors, opens portals, handles extract / descend / death.
public class RunManager : MonoBehaviour
{
    public enum State { StartScreen, Playing, Summary }

    public static RunManager Instance { get; private set; }
    // After the last wave every pickup flies to you, so nothing is left behind.
    public static bool LootVacuum => Instance != null && Instance.PortalsOpen;

    public State Current { get; private set; }
    public GameObject LevelRoot => level != null ? level.root : null;
    public bool PortalsOpen { get; private set; }
    public int CarriedGold { get; private set; }

    // Filled in when the run ends, for the summary screen.
    public bool Extracted { get; private set; }
    public bool DiedToCollapse { get; private set; }
    public int GoldBankedThisRun { get; private set; }
    public float RunDuration { get; private set; }
    public int LevelReached { get; private set; }

    LevelLayout level;
    WaveSpawner waves;
    CollapseTimer collapse;
    float runStartTime;
    bool hasRequest;
    PortalKind request;

    void Awake() => Instance = this;

    int testRoomIndex = -1;

    // Builds (or rebuilds) the current floor. Called before the player exists for floor 1.
    // testRoom: just that one room (F8 room tester).
    public LevelLayout BuildFloor(GameObject testRoom = null)
    {
        if (level != null)
        {
            waves.ClearEnemies();
            Pickup.ReleaseAll();
            DestroyImmediate(level.root);
        }
        PortalsOpen = false;
        LevelLighting.Apply(Game.Tuning);
        level = FloorAssembler.Build(Game.Tuning, RoomLibrary.Current, testRoom);
        PlaceShrines();
        ExplosiveBarrel.SpawnForFloor(level.barrelSpots, level.root.transform);
        Debug.Log("Floor built from: " + string.Join(", ", level.roomNames));
        return level;
    }

    public void Init(WaveSpawner spawner, CollapseTimer collapseTimer)
    {
        waves = spawner;
        collapse = collapseTimer;
        waves.FloorCleared += OpenPortals;
        Game.PlayerHealth.Died += () => EndRun(extracted: false);
        Current = State.StartScreen;
    }

    public void AddGold(int amount) => CarriedGold += amount;

    public void MarkCollapseDeath() => DiedToCollapse = true;

    public void RequestPortal(PortalKind kind)
    {
        hasRequest = true;
        request = kind;
    }

    void Update()
    {
        var kb = Keyboard.current;
        bool enter = kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);

        switch (Current)
        {
            case State.StartScreen:
                Time.timeScale = 0f;
                Game.InputBlocked = true;
                PlayerLook.UnlockCursor();
                if (enter) BeginRun();
                break;

            case State.Playing:
                if (!hasRequest) break;
                hasRequest = false;
                if (request == PortalKind.Extract) EndRun(extracted: true);
                else Descend();
                break;

            case State.Summary:
                if (enter)
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                break;
        }
    }

    void BeginRun()
    {
        Current = State.Playing;
        Time.timeScale = 1f;
        Game.InputBlocked = false;
        PlayerLook.LockCursor();
        runStartTime = Time.time;
        waves.BeginFloor(level.spawnPoints);
        Hud.ShowMessage("FLOOR 1");
    }

    void OpenPortals()
    {
        PortalsOpen = true;
        float s = Game.Tuning.portalSpacing;
        Vector3 right = Quaternion.Euler(0f, level.portalYaw, 0f) * Vector3.right;
        Portal.Create(PortalKind.Extract, level.portalSpot - right * s, level.root.transform, level.portalYaw);
        Portal.Create(PortalKind.Descend, level.portalSpot + right * s, level.root.transform, level.portalYaw);
        if (Game.Floor >= Game.Tuning.collapseStartFloor)
        {
            collapse.Begin();
            Hud.ShowMessage($"THE FLOOR IS COLLAPSING - {collapse.Duration:0} SECONDS!");
        }
        else Hud.ShowMessage("PORTALS OPEN - Extract or Descend?");
    }

    void Descend()
    {
        collapse.Stop();
        Game.Floor++;
        BuildFloor();
        TeleportPlayer(level.playerStart, level.playerStartYaw);
        waves.BeginFloor(level.spawnPoints);
        Hud.ShowMessage($"FLOOR {Game.Floor}");
    }

    void EndRun(bool extracted)
    {
        if (Current == State.Summary) return;
        collapse.Stop();
        var t = Game.Tuning;
        Current = State.Summary;
        Extracted = extracted;
        RunDuration = Time.time - runStartTime;
        LevelReached = Game.Player.GetComponent<PlayerLevel>().Level;
        GoldBankedThisRun = extracted ? CarriedGold : Mathf.FloorToInt(CarriedGold * t.deathBankFraction);

        MetaProgress.RecordRun(GoldBankedThisRun);
        RunLog.Append(extracted ? "extracted" : DiedToCollapse ? "collapsed" : "died", Game.Floor, RunDuration, RunStats.Kills, CarriedGold, GoldBankedThisRun,
            LevelReached, UpgradeList(arcaneOnly: false), RunStats.SecondsAboveBuzzThreshold, RunStats.Transcendences,
            RunStats.VortexCasts, RunStats.MaxChargesReached, UpgradeList(arcaneOnly: true),
            RunStats.RingOutKills, RunStats.BarrelKills, RunStats.Jumps);

        Time.timeScale = 0f;
        Game.InputBlocked = true;
        PlayerLook.UnlockCursor();
    }

    static string UpgradeList(bool arcaneOnly)
    {
        var names = new System.Collections.Generic.List<string>();
        foreach (var u in Game.Player.GetComponent<PlayerUpgrades>().TakenInOrder)
            if (!arcaneOnly || PlayerUpgrades.IsArcane(u)) names.Add(PlayerUpgrades.Name(u));
        return string.Join("; ", names);
    }

    void PlaceShrines()
    {
        var spots = new System.Collections.Generic.List<Vector3>(level.shrineSpots);
        int count = Mathf.Min(spots.Count, Random.Range(Game.Tuning.shrinesMin, Game.Tuning.shrinesMax + 1));
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, spots.Count);
            PipeShrine.Create(spots[index], level.root.transform);
            spots.RemoveAt(index);
        }
    }

    static void TeleportPlayer(Vector3 position, float yaw)
    {
        var cc = Game.Player.GetComponent<CharacterController>();
        cc.enabled = false;
        Game.Player.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        cc.enabled = true;
        Game.Player.GetComponent<PlayerMotor>().ResetMotion();
    }

    // Debug (F8): rebuild the floor as just the next room prefab, to try it out.
    public void DebugNextTestRoom()
    {
        if (Current != State.Playing) return;
        var rooms = RoomLibrary.Current != null ? RoomLibrary.Current.rooms : null;
        if (rooms == null || rooms.Count == 0) { Hud.ShowMessage("No room prefabs in Assets/Rooms"); return; }
        collapse.Stop();
        testRoomIndex = (testRoomIndex + 1) % rooms.Count;
        BuildFloor(rooms[testRoomIndex]);
        TeleportPlayer(level.playerStart, level.playerStartYaw);
        waves.BeginFloor(level.spawnPoints);
        Hud.ShowMessage($"TEST ROOM {testRoomIndex + 1}/{rooms.Count}: {level.roomNames[0]}");
    }

    // Debug: skip straight to the next floor.
    public void DebugDescend()
    {
        if (Current == State.Playing) Descend();
    }

    // Debug: open the portals right now.
    public void ForceOpenPortals()
    {
        if (PortalsOpen || Current != State.Playing) return;
        waves.ClearEnemies();
        OpenPortals();
    }
}
