using System.Collections.Generic;
using UnityEngine;

public enum PickupKind { Xp, Buzz, Gold }

// A floating pickup (XP orb, Buzz drop, gold coin). Flies to the player once they are close enough.
public class Pickup : MonoBehaviour
{
    static readonly List<Pickup> active = new List<Pickup>();

    SimplePool<Pickup> pool;
    PickupKind kind;
    float value;

    public void Begin(PickupKind pickupKind, float amount, SimplePool<Pickup> owner)
    {
        kind = pickupKind;
        value = amount;
        pool = owner;
        active.Add(this);
    }

    // Removes every pickup still lying around (used when leaving a floor).
    public static void ReleaseAll()
    {
        foreach (var p in active.ToArray()) p.Release();
    }

    void Release()
    {
        active.Remove(this);
        pool.Release(this);
    }

    void OnDestroy() => active.Remove(this);

    void Update()
    {
        var t = Game.Tuning;
        Vector3 target = Game.Player.position + Vector3.up * t.pickupHeight;
        float dist = Vector3.Distance(transform.position, target);

        if (dist <= t.pickupCollectRadius)
        {
            Collect();
            Release();
            return;
        }
        if (kind == PickupKind.Gold) transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.World);
        if (RunManager.LootVacuum)
            transform.position = Vector3.MoveTowards(transform.position, target, t.pickupMagnetSpeed * 2f * Time.deltaTime);
        else if (dist <= t.pickupMagnetRadius)
            transform.position = Vector3.MoveTowards(transform.position, target, t.pickupMagnetSpeed * Time.deltaTime);
    }

    void Collect()
    {
        switch (kind)
        {
            case PickupKind.Xp: Game.Player.GetComponent<PlayerLevel>().AddXp(value); break;
            case PickupKind.Buzz: Game.Buzz.Add(value); break;
            case PickupKind.Gold: RunManager.Instance.AddGold(Mathf.RoundToInt(value)); break;
        }
    }
}
