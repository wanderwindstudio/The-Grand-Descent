using System.Collections.Generic;
using UnityEngine;

// What a level builder hands back: where things go.
public class LevelLayout
{
    public GameObject root;
    public Vector3 playerStart;
    public float playerStartYaw;
    public List<Vector3> spawnPoints = new List<Vector3>();
    public List<Vector3> shrineSpots = new List<Vector3>(); // off the main path / risky places
}
