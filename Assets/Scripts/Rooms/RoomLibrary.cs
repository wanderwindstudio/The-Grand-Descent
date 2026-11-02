using System.Collections.Generic;
using UnityEngine;

// Every room prefab in Assets/Rooms. Kept up to date automatically by the editor
// (RoomLibraryUpdater) whenever a prefab there is saved, added or deleted.
public class RoomLibrary : ScriptableObject
{
    public static RoomLibrary Current;

    public List<GameObject> rooms = new List<GameObject>();
}
