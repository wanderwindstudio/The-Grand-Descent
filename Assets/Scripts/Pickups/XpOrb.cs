using UnityEngine;

// A floating XP orb. Flies to the player once they are close enough, then gives XP.
public class XpOrb : MonoBehaviour
{
    SimplePool<XpOrb> pool;
    float value;

    public void Begin(float xp, SimplePool<XpOrb> owner)
    {
        value = xp;
        pool = owner;
    }

    void Update()
    {
        var t = Game.Tuning;
        Vector3 target = Game.Player.position + Vector3.up * t.pickupHeight;
        float dist = Vector3.Distance(transform.position, target);

        if (dist <= t.pickupCollectRadius)
        {
            Game.Player.GetComponent<PlayerLevel>().AddXp(value);
            pool.Release(this);
            return;
        }
        if (dist <= t.pickupMagnetRadius)
            transform.position = Vector3.MoveTowards(transform.position, target, t.pickupMagnetSpeed * Time.deltaTime);
    }
}
