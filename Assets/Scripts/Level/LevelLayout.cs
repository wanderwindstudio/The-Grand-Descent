using System.Collections.Generic;
using UnityEngine;

// What a level builder hands back: where things go.
public class LevelLayout
{
    public GameObject root;
    public Vector3 playerStart;
    public float playerStartYaw;
    public List<Vector3> spawnPoints = new List<Vector3>();
    public Vector3 portalSpot;
    public float portalYaw;
    public List<Vector3> shrineSpots = new List<Vector3>(); // off the main path / risky places
    public List<Vector3> barrelSpots = new List<Vector3>();
    public List<string> roomNames = new List<string>();
}
