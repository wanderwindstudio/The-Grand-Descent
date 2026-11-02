using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// One run from the first floor to the summary: builds floors, opens portals, handles extract / descend / death,
// autosaves at the start of every floor and resumes a saved run.
public class RunManager : MonoBehaviour
{
    public enum State { StartScreen, Playing, Summary }

    public const string MenuScene = "MainMenu";

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
    float savedRunSeconds; // play time before the save this run was loaded from
    int seed;              // every floor's layout comes from this, so a loaded run rebuilds the same floor
    bool hasRequest;
    PortalKind request;

    void Awake() => Instance = this;

    // Before the first floor is built: a fresh run, or the saved one.
    public void PrepareRun()
    {
        if (SaveSystem.ResumeRun)
        {
            var saved = SaveSystem.Active.run;
            seed = saved.seed;
            Game.Floor = Mathf.Max(1, saved.floor);
            savedRunSeconds = saved.runSeconds;
        }
        else
        {
            seed = System.Environment.TickCount;
            Game.Floor = 1;
        }
    }

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
        if (testRoom == null) Random.InitState(seed + Game.Floor * 7919); // same floor every time for this run
        level = FloorAssembler.Build(Game.Tuning, RoomLibrary.Current, testRoom);
        PlaceShrines();
        ExplosiveBarrel.SpawnForFloor(level.barrelSpots, level.root.transform);
        Random.InitState(System.Environment.TickCount); // the fight itself stays unpredictable
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
        if (SaveSystem.ResumeRun) RunSaver.Restore(SaveSystem.Active.run);
        BeginRun();
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
            case State.Playing:
                if (!hasRequest) break;
                hasRequest = false;
                if (request == PortalKind.Extract) EndRun(extracted: true);
                else Descend();
                break;

            case State.Summary:
                if (enter) LoadMenu();
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
        Autosave();
        waves.BeginFloor(level.spawnPoints);
        Hud.ShowMessage(SaveSystem.ActiveSlot < 0 ? $"FLOOR {Game.Floor}  (test run - not saved)" : $"FLOOR {Game.Floor}");
    }

    float RunSeconds => savedRunSeconds + Time.time - runStartTime;

    // At the start of every floor, before the first wave.
    void Autosave()
    {
        if (SaveSystem.ActiveSlot < 0) return;
        SaveSystem.Active.hasRun = true;
        SaveSystem.Active.run = RunSaver.Capture(seed, RunSeconds);
        MetaProgress.ReachedFloor(Game.Floor);
        SaveSystem.SaveActive();
    }

    // Pause menu: keep everything earned so far; loading restarts this floor from its beginning.
    public void SaveAndQuit()
    {
        if (Current == State.Playing) Autosave();
        LoadMenu();
    }

    // Pause menu: back to the menu without saving (the last autosave stays).
    public void QuitToMenu() => LoadMenu();

    public static void LoadMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Game.InputBlocked = false;
        if (Application.CanStreamedLevelBeLoaded(MenuScene)) SceneManager.LoadScene(MenuScene);
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // menu scene missing: restart
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
        Autosave();
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
        RunDuration = RunSeconds;
        LevelReached = Game.Player.GetComponent<PlayerLevel>().Level;
        GoldBankedThisRun = extracted ? CarriedGold : Mathf.FloorToInt(CarriedGold * t.deathBankFraction);

        MetaProgress.RecordRun(GoldBankedThisRun, Game.Floor);
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
