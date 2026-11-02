using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// After the NavMesh is baked: adds NavMeshLinks so goblins can drop off ledges (RoomLedge), hop low walls
// (RoomLowWall) and climb ladders (RoomLadder). Each link also gets a TraversalLink saying how to cross it.
public static class TraversalLinkBuilder
{
    const int SolidMask = ~(1 << 2); // everything except Ignore Raycast (pit and ladder triggers)

    public static void Build(GameObject root, TuningConfig t, int agentType)
    {
        int drops = 0, hops = 0, ladders = 0;
        foreach (var ledge in root.GetComponentsInChildren<RoomLedge>()) drops += LedgeLinks(root, ledge, t, agentType);
        foreach (var wall in root.GetComponentsInChildren<RoomLowWall>()) hops += LowWallLinks(root, wall, t, agentType);
        foreach (var ladder in root.GetComponentsInChildren<RoomLadder>()) ladders += LadderLink(root, ladder, agentType);
        Debug.Log($"Goblin links: {drops} drops, {hops} wall hops, {ladders} ladders");
    }

    static int LedgeLinks(GameObject root, RoomLedge ledge, TuningConfig t, int agentType)
    {
        int made = 0;
        float spacing = Mathf.Max(0.5f, t.traversalLinkSpacing);
        int count = Mathf.Max(1, Mathf.FloorToInt(ledge.length / spacing));
        for (int i = 0; i < count; i++)
        {
            float s = -ledge.length / 2f + ledge.length * (i + 0.5f) / count;
            Vector3 edge = ledge.transform.position + ledge.Along * s;
            if (!OnNavMesh(edge - ledge.Outward * 0.7f, out Vector3 top)) continue;

            // Where would a goblin land?
            Vector3 probe = edge + ledge.Outward * 1f + Vector3.up * 0.3f;
            if (!Physics.Raycast(probe, Vector3.down, out RaycastHit ground, 40f, SolidMask, QueryTriggerInteraction.Ignore)) continue;
            float drop = ledge.transform.position.y - ground.point.y; // the real floor heights, not the NavMesh surface
            if (drop < t.minDropLinkHeight || drop > t.goblinLedgeJumpHeight + 0.25f) continue;
            if (!OnNavMesh(ground.point + ledge.Outward * 0.3f, out Vector3 bottom)) continue;

            AddLink(root, top, bottom, false, TraversalLink.Kind.Drop, null, agentType);
            made++;
        }
        return made;
    }

    static int LowWallLinks(GameObject root, RoomLowWall wall, TuningConfig t, int agentType)
    {
        Bounds b = wall.GetComponent<Collider>().bounds;
        bool alongX = b.size.x >= b.size.z;
        Vector3 along = alongX ? Vector3.right : Vector3.forward;
        Vector3 across = alongX ? Vector3.forward : Vector3.right;
        float length = alongX ? b.size.x : b.size.z;
        float thickness = alongX ? b.size.z : b.size.x;

        int made = 0;
        float spacing = Mathf.Max(0.5f, t.traversalLinkSpacing);
        int count = Mathf.Max(1, Mathf.FloorToInt((length - 0.6f) / spacing));
        for (int i = 0; i < count; i++)
        {
            float s = -length / 2f + 0.3f + (length - 0.6f) * (i + 0.5f) / count;
            Vector3 c = new Vector3(b.center.x, b.min.y, b.center.z) + along * s;
            if (!OnNavMesh(c - across * (thickness / 2f + 0.6f), out Vector3 a)) continue;
            if (!OnNavMesh(c + across * (thickness / 2f + 0.6f), out Vector3 z)) continue;
            AddLink(root, a, z, true, TraversalLink.Kind.Hop, null, agentType);
            made++;
        }
        return made;
    }

    static int LadderLink(GameObject root, RoomLadder ladder, int agentType)
    {
        if (!OnNavMesh(ladder.Foot, out Vector3 bottom) || !OnNavMesh(ladder.TopLanding, out Vector3 top))
        {
            Debug.LogWarning($"Ladder at {ladder.transform.position} has no NavMesh at its foot or top; goblins can't use it.");
            return 0;
        }
        AddLink(root, bottom, top, true, TraversalLink.Kind.Ladder, ladder, agentType);
        return 1;
    }

    static bool OnNavMesh(Vector3 near, out Vector3 point)
    {
        if (NavMesh.SamplePosition(near, out NavMeshHit hit, 0.8f, NavMesh.AllAreas) && Mathf.Abs(hit.position.y - near.y) < 0.6f)
        {
            point = hit.position;
            return true;
        }
        point = near;
        return false;
    }

    static void AddLink(GameObject root, Vector3 from, Vector3 to, bool bothWays, TraversalLink.Kind kind, RoomLadder ladder, int agentType)
    {
        var go = new GameObject("Goblin " + kind + " Link");
        go.transform.SetParent(root.transform, false); // root sits at the origin, so local = world
        var info = go.AddComponent<TraversalLink>();
        info.kind = kind;
        info.ladder = ladder;
        var link = go.AddComponent<NavMeshLink>();
        link.agentTypeID = agentType;
        link.startPoint = from;
        link.endPoint = to;
        link.width = 0f;
        link.bidirectional = bothWays;
        link.area = 0;
        link.UpdateLink();
    }
}
