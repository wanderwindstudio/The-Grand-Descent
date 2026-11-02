using UnityEngine;

// Put this on a low wall (about 1 m high, with a Box Collider). The player jumps over it;
// goblins get hop links across it automatically, every 1.5 m along its length.
[RequireComponent(typeof(Collider))]
public class RoomLowWall : MonoBehaviour
{
    void OnDrawGizmos()
    {
        var c = GetComponent<Collider>();
        if (c == null) return;
        Gizmos.color = new Color(0.4f, 0.8f, 1f);
        Gizmos.DrawWireCube(c.bounds.center, c.bounds.size + Vector3.one * 0.05f);
    }
}
