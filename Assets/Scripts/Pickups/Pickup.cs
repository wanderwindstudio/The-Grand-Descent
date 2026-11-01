using UnityEngine;

public enum PickupKind { Xp, Buzz }

// A floating pickup (XP orb, Buzz drop). Flies to the player once they are close enough.
public class Pickup : MonoBehaviour
{
    SimplePool<Pickup> pool;
    PickupKind kind;
    float value;

    public void Begin(PickupKind pickupKind, float amount, SimplePool<Pickup> owner)
    {
        kind = pickupKind;
        value = amount;
        pool = owner;
    }

    void Update()
    {
        var t = Game.Tuning;
        Vector3 target = Game.Player.position + Vector3.up * t.pickupHeight;
        float dist = Vector3.Distance(transform.position, target);

        if (dist <= t.pickupCollectRadius)
        {
            Collect();
            pool.Release(this);
            return;
        }
        if (dist <= t.pickupMagnetRadius)
            transform.position = Vector3.MoveTowards(transform.position, target, t.pickupMagnetSpeed * Time.deltaTime);
    }

    void Collect()
    {
        switch (kind)
        {
            case PickupKind.Xp: Game.Player.GetComponent<PlayerLevel>().AddXp(value); break;
            case PickupKind.Buzz: Game.Buzz.Add(value); break;
        }
    }
}
