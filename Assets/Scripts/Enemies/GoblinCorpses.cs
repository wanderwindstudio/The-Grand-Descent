using UnityEngine;

// When a goblin dies in sprite mode, a short-lived corpse sprite plays out the death:
// a white flash in the flinch frame, then the dead frame on the floor, then it darkens and sinks.
// (Without a dead frame, the idle sprite tips over instead.)
// The goblin itself goes straight back to its pool, so gameplay is unaffected.
public class GoblinCorpses : MonoBehaviour
{
    SimplePool<Corpse> pool;

    void Awake()
    {
        pool = new SimplePool<Corpse>(CreateCorpse);
        GoblinEnemy.Killed += OnGoblinKilled;
    }

    void OnDestroy() => GoblinEnemy.Killed -= OnGoblinKilled;

    void OnGoblinKilled(Vector3 where)
    {
        if (!Game.Tuning.useSpriteEnemies || !Art.HasGoblinSprite) return;
        pool.Get(where).Begin(pool);
    }

    static Corpse CreateCorpse()
    {
        var root = new GameObject("Goblin Corpse");
        root.SetActive(false);
        var corpse = root.AddComponent<Corpse>();
        corpse.pivot = Art.CreateBillboard("Sprite", root.transform, Art.GoblinIdle, out var renderer);
        corpse.material = renderer.sharedMaterial;
        return corpse;
    }

    public class Corpse : MonoBehaviour
    {
        public Transform pivot;
        public Material material;
        SimplePool<Corpse> pool;
        float age;
        float side;

        public void Begin(SimplePool<Corpse> owner)
        {
            pool = owner;
            age = 0f;
            side = Random.value < 0.5f ? -1f : 1f; // fall left or right
            Update();
        }

        void Update()
        {
            var t = Game.Tuning;
            age += Time.deltaTime;
            float k = Mathf.Clamp01(age / t.corpseDuration);
            if (k >= 1f) { pool.Release(this); return; }

            Art.SizeBillboard(pivot, Art.GoblinIdle, t.goblinSpriteHeight, t.goblinSpriteFillFraction);
            bool flinching = age < t.spriteHitPoseTime;
            bool hasDeadFrame = Art.GoblinDead != null;
            material.SetTexture("_BaseMap", flinching ? Art.GoblinHit : hasDeadFrame ? Art.GoblinDead : Art.GoblinIdle);

            // Fade out: cutout sprites can't go see-through, so near the end it darkens and sinks into the floor.
            float fade = Mathf.InverseLerp(0.6f, 1f, k);
            pivot.position = transform.position + Vector3.down * (t.goblinSpriteHeight * 0.6f * fade * fade);
            Art.FaceCamera(pivot);
            if (!hasDeadFrame && !flinching)
                pivot.rotation *= Quaternion.Euler(0f, 0f, side * t.corpseTipDegrees * Mathf.Min(1f, k * 2.5f)); // tip over around the feet

            material.SetColor("_EmissionColor", age < t.enemyHitFlashTime ? Color.white * t.spriteHitFlashStrength : Color.black);
            material.SetColor("_BaseColor", Color.Lerp(Color.white, new Color(0.15f, 0.15f, 0.15f), fade));
        }
    }
}
