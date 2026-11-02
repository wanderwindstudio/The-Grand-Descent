using UnityEngine;

// The edge of a platform goblins may jump down from (balcony edge, pit rim, bridge side).
// Place it on the edge itself at the platform's floor height, red axis (X) along the edge,
// blue arrow (Z) pointing out over the drop, and set Length. Drop links are made automatically
// wherever the fall is between 1 m and the goblin ledge-jump height (TuningConfig).
public class RoomLedge : MonoBehaviour
{
    public float length = 6f;

    public Vector3 Outward
    {
        get { Vector3 f = transform.forward; f.y = 0f; return f.normalized; }
    }

    public Vector3 Along
    {
        get { Vector3 r = transform.right; r.y = 0f; return r.normalized; }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f);
        Vector3 a = transform.position - Along * (length / 2f), b = transform.position + Along * (length / 2f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(transform.position, transform.position + Outward + Vector3.down * 0.5f);
    }
}
