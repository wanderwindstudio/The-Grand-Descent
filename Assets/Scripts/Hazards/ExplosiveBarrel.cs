using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// A red "sponsored" barrel (gold crown logo). Shoot it: it flashes for a moment, then explodes,
// hurting everyone nearby - you included - and throwing them back. Barrels set each other off.
public class ExplosiveBarrel : MonoBehaviour
{
    public static readonly List<ExplosiveBarrel> All = new List<ExplosiveBarrel>();

    static AudioClip boom;

    Material material;
    Transform body;
    float fuseLeft = -1f;
    bool exploded;

    public static void SpawnForFloor(List<Vector3> spots, Transform parent)
    {
        var t = Game.Tuning;
        var free = new List<Vector3>(spots);
        int count = Mathf.Min(free.Count, Random.Range(t.barrelsMin, t.barrelsMax + 1));
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, free.Count);
            Create(free[index], parent);
            free.RemoveAt(index);
        }
    }

    public static ExplosiveBarrel Create(Vector3 position, Transform parent)
    {
        var root = new GameObject("Explosive Barrel");
        root.transform.SetParent(parent, false);
        root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        var barrel = root.AddComponent<ExplosiveBarrel>();

        var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drum.name = "Drum";
        drum.transform.SetParent(root.transform, false);
        drum.transform.localPosition = Vector3.up * 0.45f;
        drum.transform.localScale = new Vector3(0.7f, 0.45f, 0.7f);
        barrel.material = Mats.New(Palette.Barrel, emissive: true);
        drum.GetComponent<Renderer>().sharedMaterial = barrel.material;
        barrel.body = drum.transform;

        // Gold rim band and a little crown "sponsor" logo on two sides.
        Material gold = Mats.Glow(Palette.Gold, 0.4f);
        Deco(root, "Band", new Vector3(0f, 0.72f, 0f), new Vector3(0.72f, 0.03f, 0.72f), gold, PrimitiveType.Cylinder);
        foreach (float side in new[] { 1f, -1f })
        {
            float z = side * 0.36f;
            Deco(root, "Crown Base", new Vector3(0f, 0.42f, z), new Vector3(0.24f, 0.07f, 0.03f), gold, PrimitiveType.Cube);
            for (int i = -1; i <= 1; i++)
                Deco(root, "Crown Point", new Vector3(i * 0.09f, 0.5f + (i == 0 ? 0.03f : 0f), z), new Vector3(0.05f, 0.09f + (i == 0 ? 0.04f : 0f), 0.03f), gold, PrimitiveType.Cube);
        }

        var obstacle = root.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.center = Vector3.up * 0.45f;
        obstacle.radius = 0.4f;
        obstacle.height = 0.9f;
        obstacle.carving = true;
        return barrel;
    }

    static void Deco(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat, PrimitiveType shape)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    // Start the fuse (extra delay for chain reactions).
    public void Ignite(float extraDelay)
    {
        if (exploded || fuseLeft >= 0f) return;
        fuseLeft = Game.Tuning.barrelFuseTime + extraDelay;
    }

    void Update()
    {
        if (fuseLeft < 0f) return;
        fuseLeft -= Time.deltaTime;
        // Flash faster and faster, and swell a little.
        bool on = Mathf.Sin(Time.time * 50f) > 0f;
        Mats.SetEmission(material, on ? Palette.BarrelFlash * 3f : Color.black);
        body.localScale = new Vector3(0.7f, 0.45f, 0.7f) * (1f + 0.08f * (1f - Mathf.Clamp01(fuseLeft / Game.Tuning.barrelFuseTime)));
        if (fuseLeft <= 0f) Explode();
    }

    void Explode()
    {
        var t = Game.Tuning;
        exploded = true;
        Vector3 centre = transform.position + Vector3.up * 0.5f;

        SpellFx.Impact(centre, 0.5f, t.barrelRadius * 2f, 0.5f, 8f, t.barrelRadius * 4f, Palette.Explosion, Palette.Explosion);
        if (boom == null) boom = ToneGenerator.Boom("Barrel Boom");
        AudioSource.PlayClipAtPoint(boom, centre, 1f);

        foreach (var g in GoblinEnemy.Active.ToArray())
        {
            Vector3 d = g.transform.position + Vector3.up * 0.7f - centre;
            if (d.magnitude > t.barrelRadius) continue;
            float k = Falloff(d.magnitude, t.barrelRadius);
            g.TakeDamage(t.barrelDamage * k, GoblinEnemy.Cause.Barrel);
            if (!g.IsDead) g.Knockback(Push(d, k), GoblinEnemy.Cause.Barrel);
        }

        Vector3 toPlayer = Game.Player.position + Vector3.up * 0.9f - centre;
        if (toPlayer.magnitude <= t.barrelRadius)
        {
            float k = Falloff(toPlayer.magnitude, t.barrelRadius);
            Game.PlayerHealth.TakeDamage(t.barrelPlayerDamage * k, centre);
            Game.Player.GetComponent<PlayerMotor>().AddImpulse(Push(toPlayer, k));
        }

        foreach (var other in All.ToArray())
            if (other != this && Vector3.Distance(other.transform.position, transform.position) <= t.barrelRadius)
                other.Ignite(t.barrelChainDelay);

        Destroy(gameObject);
    }

    // Full strength in the middle, half at the edge.
    static float Falloff(float distance, float radius) => Mathf.Lerp(1f, 0.5f, distance / radius);

    static Vector3 Push(Vector3 fromCentre, float k)
    {
        var t = Game.Tuning;
        Vector3 flat = new Vector3(fromCentre.x, 0f, fromCentre.z);
        if (flat.sqrMagnitude < 0.01f) flat = Random.insideUnitSphere;
        flat.y = 0f;
        return flat.normalized * (t.barrelKnockback * k) + Vector3.up * (t.barrelKnockbackUp * k);
    }
}
