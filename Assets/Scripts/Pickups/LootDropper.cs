using UnityEngine;

// Drops loot where goblins die: always an XP orb, sometimes gold, sometimes a Buzz pickup.
public class LootDropper : MonoBehaviour
{
    SimplePool<Pickup> xpPool, buzzPool, goldPool;

    void Awake()
    {
        xpPool = new SimplePool<Pickup>(() => CreatePickup("XP Orb", Palette.XpOrb));
        buzzPool = new SimplePool<Pickup>(() => CreatePickup("Buzz Pickup", Palette.Buzz));
        goldPool = new SimplePool<Pickup>(CreateCoin);
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
        if (Random.value < t.goldDropChance)
        {
            Vector2 nudge = Random.insideUnitCircle * 0.5f;
            int gold = Mathf.Max(1, Mathf.RoundToInt(t.goldPerDrop * FloorScaling.Gold));
            goldPool.Get(pos + new Vector3(nudge.x, 0f, nudge.y)).Begin(PickupKind.Gold, gold, goldPool);
        }
    }

    // A gold coin: a flat spinning disc standing on its edge.
    static Pickup CreateCoin()
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Gold Coin";
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>());
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(t.goldCoinSize, 0.04f, t.goldCoinSize);
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(Palette.Gold, 1.5f);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go.AddComponent<Pickup>();
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
