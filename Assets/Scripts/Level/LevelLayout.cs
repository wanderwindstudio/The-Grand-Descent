using System.Collections.Generic;
using UnityEngine;

// What a level builder hands back: where things go.
public class LevelLayout
{
    public GameObject root;
    public Vector3 playerStart;
    public float playerStartYaw;
    public List<Vector3> spawnPoints = new List<Vector3>();
}
