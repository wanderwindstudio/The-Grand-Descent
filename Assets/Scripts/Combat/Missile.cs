using System.Collections.Generic;
using UnityEngine;

// What to fire: filled in by the caster, handed to Missile.Launch.
public struct MissileShot
{
    public Vector3 direction;
    public float damage;
    public int pierce;
    public float speed;
    public float hitRadius;
    public float length;      // sprite length in metres
    public float wobble;      // Buzz wobble amplitude in metres
    public Color tint;
    public bool countsHits;   // hits build Arcane Charges
    public bool countsMisses; // a miss resets Arcane Charges
}

// An Arcane Missile in flight. Sweeps a sphere ahead each frame to find what it hits.
// The sprite always faces the camera, turned so its bright head points along its on-screen travel.
// At high Buzz it drifts off its line in a sine wave (wobble).
public class Missile : MonoBehaviour
{
    // The source art points up and to the right.
    const float ArtHeadingDegrees = 45f;

    static readonly RaycastHit[] hits = new RaycastHit[16];
    static readonly HitDistanceComparer byDistance = new HitDistanceComparer();

    SimplePool<Missile> pool;
    Renderer sprite;
    Light glow;
    readonly HashSet<GoblinEnemy> alreadyHit = new HashSet<GoblinEnemy>();

    MissileShot shot;
    Vector3 side, lift;
    Vector3 straightPosition; // where it would be without wobble
    float phase, age, lifeLeft, screenAngle;
    int hitsLeft;
    bool hitSomething;

    public void SetPool(SimplePool<Missile> owner) => pool = owner;

    void Awake()
    {
        sprite = transform.Find("Sprite").GetComponent<Renderer>();
        glow = GetComponentInChildren<Light>();
    }

    public void Launch(MissileShot s)
    {
        var t = Game.Tuning;
        shot = s;
        shot.direction = s.direction.normalized;
        side = Vector3.Cross(Vector3.up, shot.direction).normalized;
        if (side.sqrMagnitude < 0.01f) side = Vector3.right;
        lift = Vector3.Cross(shot.direction, side);

        hitsLeft = 1 + s.pierce;
        hitSomething = false;
        phase = Random.Range(0f, Mathf.PI * 2f);
        age = 0f;
        lifeLeft = t.missileLifetime / Mathf.Max(0.1f, s.speed / t.missileSpeed); // slow orbs fly as far as normal ones
        straightPosition = transform.position;
        alreadyHit.Clear();

        // The art fills roughly the diagonal of its square, so the quad side is a bit shorter than the length.
        sprite.transform.localScale = Vector3.one * (s.length / 1.2f);
        Art.SetFxColor(sprite, s.tint, t.spellSpriteBrightness, 1f);
        glow.color = Palette.ArcaneLight;
        glow.range = t.missileLightRange;
        glow.intensity = t.missileLightIntensity;
        screenAngle = AngleOnScreen(transform.position, transform.position + shot.direction, screenAngle);
        OrientSprite();
    }

    void Update()
    {
        var t = Game.Tuning;
        float dt = Time.deltaTime;
        age += dt;
        straightPosition += shot.direction * shot.speed * dt;

        float ramp = Mathf.Clamp01(age / t.wobbleRampTime);
        float w = 2f * Mathf.PI * t.wobbleFrequency * age + phase;
        Vector3 offset = (side * Mathf.Sin(w) + lift * (0.5f * Mathf.Cos(w * 0.7f))) * (shot.wobble * ramp);
        Vector3 from = transform.position;
        Vector3 to = straightPosition + offset;
        Vector3 move = to - from;
        float dist = move.magnitude;

        if (dist > 0.0001f && SweepHits(from, move / dist, dist)) return;

        transform.position = to;
        screenAngle = AngleOnScreen(from, to, screenAngle);
        OrientSprite();

        lifeLeft -= dt;
        if (lifeLeft <= 0f) End(missed: !hitSomething);
    }

    // Returns true if the missile was used up.
    bool SweepHits(Vector3 from, Vector3 dir, float dist)
    {
        int count = Physics.SphereCastNonAlloc(from, shot.hitRadius, dir, hits, dist, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, 0, count, byDistance);

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i].collider;
            if (col.GetComponentInParent<PlayerHealth>() != null) continue;
            Vector3 point = hits[i].distance > 0f ? hits[i].point : from;

            var goblin = col.GetComponentInParent<GoblinEnemy>();
            if (goblin == null) // a wall or the floor
            {
                SpellFx.MissileImpact(point);
                End(missed: !hitSomething);
                return true;
            }

            if (goblin.IsDead || alreadyHit.Contains(goblin)) continue;
            alreadyHit.Add(goblin);
            goblin.TakeDamage(shot.damage);
            SpellFx.MissileImpact(point);
            if (shot.countsHits) Game.Charges.RegisterHit();
            hitSomething = true;
            hitsLeft--;
            if (hitsLeft <= 0) { End(missed: false); return true; }
        }
        return false;
    }

    void End(bool missed)
    {
        if (missed && shot.countsMisses) Game.Charges.RegisterMiss();
        pool.Release(this);
    }

    void OrientSprite() => Art.FaceCameraFully(sprite.transform, screenAngle - ArtHeadingDegrees);

    // Direction of travel on screen, in degrees (0 = right, 90 = up). Keeps the old angle if there's no clear direction.
    static float AngleOnScreen(Vector3 from, Vector3 to, float fallback)
    {
        Camera cam = Game.Camera;
        Vector3 a = cam.WorldToScreenPoint(from), b = cam.WorldToScreenPoint(to);
        if (a.z <= 0f || b.z <= 0f) return fallback;
        Vector2 d = new Vector2(b.x - a.x, b.y - a.y);
        return d.sqrMagnitude < 0.01f ? fallback : Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
    }

    class HitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
