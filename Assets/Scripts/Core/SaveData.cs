using System;
using System.Collections.Generic;

// What one save slot holds. Stored as JSON; "version" lets old saves be migrated later.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public string savedAt = ""; // local time, ISO 8601
    public MetaSave meta = new MetaSave();
    public bool hasRun;
    public RunSave run = new RunSave();

    public DateTime SavedAt => DateTime.TryParse(savedAt, out var d) ? d : DateTime.MinValue;
}

// Survives every run.
[Serializable]
public class MetaSave
{
    public int bankedGold;
    public int totalRuns;
    public int bestFloor;
    public bool introSeen;
}

// The run in progress. Loading resumes at the start of this floor.
[Serializable]
public class RunSave
{
    public int floor = 1;
    public int seed;
    public int level = 1;
    public float xp;
    public int pendingLevelUps;
    public float health, maxHealth;
    public List<string> upgrades = new List<string>(); // in the order they were taken
    public List<string> spells = new List<string>();
    public int carriedGold;
    public float buzz;
    public int kills;
    public float runSeconds;
}
