using System.Collections.Generic;
using UnityEngine;

// A pit: a box covering the hollow below the rim (from the pit floor up to the rim).
// Goblins knocked or dropped into it from outside are a RING OUT (instant kill).
// Walking in (e.g. down a ramp) is fine. Every pit needs a way back up for both you and goblins.
[RequireComponent(typeof(BoxCollider))]
public class RoomPit : MonoBehaviour
{
    public static readonly List<RoomPit> All = new List<RoomPit>();

    BoxCollider box;

    public Bounds Bounds => box.bounds;
    public float RimY => box.bounds.max.y;

    void Awake() => Setup();
    void OnValidate() => Setup();

    void Setup()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        gameObject.layer = 2; // Ignore Raycast: never part of the NavMesh, never blocks shots
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    // Inside the hollow.
    public bool Contains(Vector3 p)
    {
        Bounds b = Bounds;
        return p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && p.y >= b.min.y - 0.5f && p.y <= b.max.y;
    }

    // Inside the hollow or in the air above it.
    public bool IsOver(Vector3 p)
    {
        Bounds b = Bounds;
        return p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && p.y >= b.min.y - 0.5f && p.y <= b.max.y + 4f;
    }

    public static RoomPit At(Vector3 p)
    {
        foreach (var pit in All) if (pit.Contains(p)) return pit;
        return null;
    }

    public static RoomPit Above(Vector3 p)
    {
        foreach (var pit in All) if (pit.IsOver(p)) return pit;
        return null;
    }

    void OnDrawGizmos()
    {
        var b = GetComponent<BoxCollider>();
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(b.center, b.size);
    }
}
