using UnityEngine;

// Drops loot where goblins die: always an XP orb, sometimes a Buzz pickup. Gold comes later.
public class LootDropper : MonoBehaviour
{
    SimplePool<Pickup> xpPool, buzzPool;

    void Awake()
    {
        xpPool = new SimplePool<Pickup>(() => CreatePickup("XP Orb", Palette.XpOrb));
        buzzPool = new SimplePool<Pickup>(() => CreatePickup("Buzz Pickup", Palette.Buzz));
        GoblinEnemy.Killed += OnGoblinKilled;
    }

    void OnDestroy() => GoblinEnemy.Killed -= OnGoblinKilled;

    void OnGoblinKilled(Vector3 where)
    {
        var t = Game.Tuning;
        Vector3 pos = where + Vector3.up * t.pickupHeight;
        xpPool.Get(pos).Begin(PickupKind.Xp, t.xpPerGoblin, xpPool);

        if (Random.value < t.buzzPickupDropChance)
        {
            Vector2 nudge = Random.insideUnitCircle * 0.5f; // don't overlap the XP orb
            buzzPool.Get(pos + new Vector3(nudge.x, 0f, nudge.y)).Begin(PickupKind.Buzz, t.buzzPickupAmount, buzzPool);
        }
    }

    static Pickup CreatePickup(string name, Color color)
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>());
        go.transform.localScale = Vector3.one * t.xpOrbSize;
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(color, t.xpOrbGlow);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go.AddComponent<Pickup>();
    }
}
