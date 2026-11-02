using System.Collections.Generic;
using UnityEngine;

// A ladder. Put this on an empty GameObject at the FOOT of the ladder, on the floor against the wall,
// with the blue arrow (Z) pointing into the wall (the way you climb over at the top). Set Height to the
// height of the platform it reaches. The climb box (a trigger) is set up for you.
// The player grabs it by walking into it while pressing forward; goblins climb it slowly.
[RequireComponent(typeof(BoxCollider))]
public class RoomLadder : MonoBehaviour
{
    public static readonly List<RoomLadder> All = new List<RoomLadder>();

    [Tooltip("Height of the platform the ladder reaches, in metres.")]
    public float height = 3.5f;

    BoxCollider box;

    public Vector3 ClimbDirection
    {
        get { Vector3 f = transform.forward; f.y = 0f; return f.normalized; }
    }

    public float TopY => transform.position.y + height;

    // Where you stand while climbing: just in front of the ladder, at the given height.
    public Vector3 Column(Vector3 at)
    {
        Vector3 c = transform.position - ClimbDirection * 0.45f;
        c.y = at.y;
        return c;
    }

    public Vector3 Foot => transform.position - ClimbDirection * 0.9f;
    public Vector3 TopLanding => transform.position + Vector3.up * height + ClimbDirection * 0.9f;

    void Awake() => Setup();
    void OnValidate() => Setup();

    // Sizes the climb box to the height. Called automatically; editor tools call it after changing height.
    public void Setup()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, height / 2f + 0.6f, -0.4f);
        box.size = new Vector3(1f, height + 1.2f, 1f);
        gameObject.layer = 2; // Ignore Raycast
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static RoomLadder Find(Vector3 point)
    {
        foreach (var l in All)
            if (l.box != null && l.box.bounds.Contains(point)) return l;
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * height);
        Gizmos.DrawLine(transform.position + Vector3.up * height, transform.position + Vector3.up * height + ClimbDirection);
    }
}
