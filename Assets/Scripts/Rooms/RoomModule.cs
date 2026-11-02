using UnityEngine;

// Put this on the root of a room prefab in Assets/Rooms. The game finds every such prefab on its own
// (see RoomLibrary) and joins rooms with corridors at their RoomDoorway markers.
// Build the room around its own origin with the floor at height 0.
public class RoomModule : MonoBehaviour
{
    public enum WallStyle { Random, Stone, Brick }

    [Tooltip("Shown by the F8 room tester.")]
    public string displayName = "New Room";
    [Tooltip("Wall texture. Random: brick in about a third of rooms, stone blocks otherwise.")]
    public WallStyle walls = WallStyle.Random;

    // World-space box around everything solid in the room (walls included).
    public Bounds CalculateBounds()
    {
        bool any = false;
        var b = new Bounds(transform.position, Vector3.zero);
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        foreach (var c in GetComponentsInChildren<Collider>())
        {
            if (c.isTrigger) continue;
            if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
        }
        return b;
    }
}
