using UnityEngine;

// Spells leave from the left hand but land exactly where the crosshair points.
public static class Aim
{
    // The point under the crosshair: the first surface or enemy hit, or far away if nothing is there.
    public static Vector3 CrosshairTarget()
    {
        Transform cam = Game.Camera.transform;
        // The ray starts inside the player's own collider, which Unity ignores, so it can't hit the player.
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, Game.Tuning.aimMaxDistance, ~0, QueryTriggerInteraction.Ignore))
            return hit.point;
        return cam.position + cam.forward * Game.Tuning.aimMaxDistance;
    }

    // Where spells appear: just in front of the left hand's fingertips on screen.
    public static Vector3 HandOrigin()
    {
        var t = Game.Tuning;
        Camera cam = Game.Camera;
        Vector2 gui = FirstPersonHands.Instance != null
            ? FirstPersonHands.Instance.FingertipGuiPosition
            : new Vector2(Screen.width * 0.3f, Screen.height * 0.75f);
        Vector3 origin = cam.ScreenToWorldPoint(new Vector3(gui.x, Screen.height - gui.y, t.handSpawnDepth));

        // Hugging a wall: don't spawn on the other side of it.
        Vector3 from = cam.transform.position;
        if (Physics.Linecast(from, origin, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
            origin = from + (origin - from).normalized * Mathf.Max(0.05f, hit.distance - 0.05f);
        return origin;
    }
}
