using UnityEngine;

// A sprayer's slow glob. Flies straight, stops at walls, hurts the player on contact. Easy to dodge.
public class EnemyProjectile : MonoBehaviour
{
    const int SolidMask = ~(1 << 2);

    Vector3 velocity;
    float lifeLeft = 8f;

    public static void Fire(Vector3 from, Vector3 target)
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Sprayer Shot";
        Destroy(go.GetComponent<Collider>());
        if (RunManager.Instance != null && RunManager.Instance.LevelRoot != null) go.transform.SetParent(RunManager.Instance.LevelRoot.transform, true);
        go.transform.position = from;
        go.transform.localScale = Vector3.one * t.sprayerProjectileSize;
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(Palette.SprayerShot, 2.5f);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;
        light.color = Palette.SprayerShot;
        light.range = 3f;
        light.intensity = 1.5f;

        var shot = go.AddComponent<EnemyProjectile>();
        shot.velocity = (target - from).normalized * t.sprayerProjectileSpeed;
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        Vector3 step = velocity * dt;
        lifeLeft -= dt;

        // Player: distance from the glob to the player's body line.
        Vector3 feet = Game.Player.position;
        Vector3 closest = ClosestOnSegment(feet + Vector3.up * 0.3f, feet + Vector3.up * 1.5f, transform.position);
        if (Vector3.Distance(closest, transform.position) <= t.sprayerProjectileSize * 0.5f + Game.Tuning.playerRadius)
        {
            Game.PlayerHealth.TakeDamage(t.sprayerProjectileDamage * FloorScaling.EnemyDamage, transform.position - velocity.normalized);
            Destroy(gameObject);
            return;
        }

        if (lifeLeft <= 0f || Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hit, step.magnitude, SolidMask, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<GoblinEnemy>() == null && hit.collider.GetComponentInParent<PlayerHealth>() == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += step;
    }

    static Vector3 ClosestOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float k = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return a + ab * k;
    }
}
