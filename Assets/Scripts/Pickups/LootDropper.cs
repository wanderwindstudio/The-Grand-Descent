using UnityEngine;

// Drops loot where goblins die. Milestone 2: XP orbs. Gold and Buzz pickups come later.
public class LootDropper : MonoBehaviour
{
    SimplePool<XpOrb> xpPool;

    void Awake()
    {
        xpPool = new SimplePool<XpOrb>(CreateXpOrb);
        GoblinEnemy.Killed += OnGoblinKilled;
    }

    void OnDestroy() => GoblinEnemy.Killed -= OnGoblinKilled;

    void OnGoblinKilled(Vector3 where)
    {
        var t = Game.Tuning;
        xpPool.Get(where + Vector3.up * t.pickupHeight).Begin(t.xpPerGoblin, xpPool);
    }

    static XpOrb CreateXpOrb()
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "XP Orb";
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>());
        go.transform.localScale = Vector3.one * t.xpOrbSize;
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(Palette.XpOrb, t.xpOrbGlow);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go.AddComponent<XpOrb>();
    }
}
