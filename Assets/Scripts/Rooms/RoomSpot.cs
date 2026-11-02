using UnityEngine;

// A marker in a room: drop an empty GameObject on the floor, add this, pick what it marks.
//   Enemy Spawn  - goblins appear here (any number per room)
//   Shrine       - a pipe shrine may appear here (optional; best somewhere risky)
//   Portal       - the extract/descend portals open here (one per room; they appear left and right of it)
//   Player Start - where you begin if this is the first room (optional)
//   Barrel       - an explosive barrel may appear here (optional; near ledges and pits is best)
//   Torch        - a wall torch (place it right against a wall, at torch height)
public class RoomSpot : MonoBehaviour
{
    public enum Kind { EnemySpawn, Shrine, Portal, PlayerStart, Barrel, Torch }

    public Kind kind;

    void OnDrawGizmos()
    {
        switch (kind)
        {
            case Kind.EnemySpawn: Gizmos.color = new Color(1f, 0.25f, 0.2f); break;
            case Kind.Shrine: Gizmos.color = Palette.Shrine; break;
            case Kind.Portal: Gizmos.color = Palette.ExtractPortal; break;
            case Kind.PlayerStart: Gizmos.color = Color.white; break;
            case Kind.Barrel: Gizmos.color = new Color(1f, 0.5f, 0f); break;
            case Kind.Torch: Gizmos.color = Palette.Torch; break;
        }
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + transform.forward);
    }
}
