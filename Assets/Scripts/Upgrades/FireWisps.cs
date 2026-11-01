using System.Collections.Generic;
using UnityEngine;

// Fire Wisp upgrade: one orbiting fireball per stack, damaging goblins it touches.
public class FireWisps : MonoBehaviour
{
    static readonly Collider[] overlaps = new Collider[32];

    readonly List<Transform> wisps = new List<Transform>();
    readonly Dictionary<GoblinEnemy, float> nextHitTime = new Dictionary<GoblinEnemy, float>();
    PlayerUpgrades upgrades;
    float angle;

    void Awake() => upgrades = GetComponent<PlayerUpgrades>();

    void Update()
    {
        var t = Game.Tuning;
        int count = upgrades.Stacks(UpgradeType.FireWisp);
        while (wisps.Count < count) wisps.Add(CreateWisp());
        if (count == 0) return;

        angle += t.wispOrbitSpeed * Time.deltaTime;
        for (int i = 0; i < count; i++)
        {
            float a = angle + 360f * i / count;
            Vector3 offset = Quaternion.Euler(0f, a, 0f) * Vector3.forward * t.wispOrbitRadius;
            Vector3 pos = transform.position + Vector3.up * t.wispHeight + offset;
            wisps[i].position = pos;
            wisps[i].localScale = Vector3.one * t.wispSize;
            Burn(pos);
        }
    }

    void Burn(Vector3 pos)
    {
        var t = Game.Tuning;
        int n = Physics.OverlapSphereNonAlloc(pos, t.wispHitRadius, overlaps, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var goblin = overlaps[i].GetComponentInParent<GoblinEnemy>();
            if (goblin == null || goblin.IsDead) continue;
            if (nextHitTime.TryGetValue(goblin, out float next) && Time.time < next) continue;
            nextHitTime[goblin] = Time.time + t.wispHitCooldown;
            goblin.TakeDamage(t.wispDamage * Game.Buzz.DamageMultiplier);
        }
    }

    Transform CreateWisp()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Fire Wisp";
        DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = Mats.Glow(Palette.Wisp, 4f);

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;
        light.color = Palette.Wisp;
        light.range = 4f;
        light.intensity = 2f;
        return go.transform;
    }
}
