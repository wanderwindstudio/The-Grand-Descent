using UnityEngine;

// Where a corridor can join the room. Place it on the floor in the middle of a wall opening, on the
// OUTSIDE face of the wall, with the blue arrow (Z) pointing out of the room. Unused doorways get bricked up.
public class RoomDoorway : MonoBehaviour
{
    public float width = 3f;
    public float height = 3.5f;

    public Vector3 Outward
    {
        get { Vector3 f = transform.forward; f.y = 0f; return f.normalized; }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Outward), Vector3.one);
        Gizmos.DrawWireCube(new Vector3(0f, height / 2f, 0f), new Vector3(width, height, 0.1f));
        Gizmos.DrawLine(Vector3.zero, Vector3.forward * 1.5f);
    }
}
