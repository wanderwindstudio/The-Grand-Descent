using UnityEngine;

// Poison Trail upgrade: drops a poison cloud every time the player has moved a set distance.
public class PoisonTrail : MonoBehaviour
{
    PlayerUpgrades upgrades;
    SimplePool<PoisonCloud> pool;
    Vector3 lastDrop;

    void Awake()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        pool = new SimplePool<PoisonCloud>(CreateCloud);
    }

    void Update()
    {
        int stacks = upgrades.Stacks(UpgradeType.PoisonTrail);
        Vector3 pos = transform.position;
        if (stacks == 0) { lastDrop = pos; return; }

        float d = Game.Tuning.poisonDropDistance;
        if ((pos - lastDrop).sqrMagnitude >= d * d)
        {
            lastDrop = pos;
            pool.Get(pos).Begin(stacks, pool);
        }
    }

    static PoisonCloud CreateCloud()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Poison Cloud";
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>());
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(Palette.Poison, 0.8f);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go.AddComponent<PoisonCloud>();
    }
}
