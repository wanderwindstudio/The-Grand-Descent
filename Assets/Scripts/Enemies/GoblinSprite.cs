using UnityEngine;
using UnityEngine.AI;

// Goblin visuals as a billboard sprite. Purely cosmetic: reads the goblin's state, never changes it.
// Fakes motion in code: a hop while walking, stretch + lunge during the wind-up, a white hit flash.
// The capsule (and its collider) stays; only its renderer is hidden while sprites are on.
public class GoblinSprite : MonoBehaviour
{
    GoblinEnemy goblin;
    NavMeshAgent agent;
    Renderer capsuleRenderer, eyesRenderer, spriteRenderer;
    Transform pivot;
    Material material;
    float bobPhase;
    float walkBlend;

    void Awake()
    {
        goblin = GetComponent<GoblinEnemy>();
        agent = GetComponent<NavMeshAgent>();
        var body = transform.Find("Body");
        capsuleRenderer = body.GetComponent<Renderer>();
        eyesRenderer = body.Find("Eyes").GetComponent<Renderer>();

        if (!Art.HasGoblinSprite) { enabled = false; return; }
        pivot = Art.CreateBillboard("Sprite", transform, Art.GoblinIdle, out spriteRenderer);
        material = spriteRenderer.sharedMaterial;
        bobPhase = Random.Range(0f, Mathf.PI * 2f); // goblins don't hop in sync
    }

    void LateUpdate()
    {
        var t = Game.Tuning;
        bool sprites = t.useSpriteEnemies;
        capsuleRenderer.enabled = !sprites;
        eyesRenderer.enabled = !sprites;
        spriteRenderer.enabled = sprites;
        if (!sprites) return;

        float dt = Time.deltaTime;
        Art.SizeBillboard(pivot, Art.GoblinIdle, t.goblinSpriteHeight, t.goblinSpriteFillFraction);

        // Walking: hop and lean side to side, blended in and out smoothly.
        bool walking = !goblin.IsWindingUp && agent.velocity.sqrMagnitude > 0.04f;
        walkBlend = Mathf.MoveTowards(walkBlend, walking ? 1f : 0f, dt * 6f);
        bobPhase += dt * t.spriteBobSpeed * Mathf.PI;
        float hop = Mathf.Abs(Mathf.Sin(bobPhase)) * t.spriteBobHeight * walkBlend;
        float sway = Mathf.Sin(bobPhase) * t.spriteSwayDegrees * walkBlend;

        // Wind-up: stretch tall and thin, then lunge toward the player right before the hit.
        float k = goblin.WindUp01;
        float stretch = 1f + t.spriteWindUpStretch * k;
        Vector3 toPlayer = Game.Player.position - transform.position;
        toPlayer.y = 0f;
        Vector3 lunge = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized * (t.spriteLungeDistance * k * k * k) : Vector3.zero;

        pivot.position = transform.position + Vector3.up * hop + lunge;
        Art.FaceCamera(pivot);
        pivot.rotation *= Quaternion.Euler(0f, 0f, sway);
        pivot.localScale = new Vector3(1f - (stretch - 1f) * 0.5f, stretch, 1f);

        Color glow = goblin.IsHitFlashing
            ? Color.white * t.spriteHitFlashStrength
            : Palette.GoblinWindUp * (t.spriteWindUpGlow * k);
        material.SetColor("_EmissionColor", glow);
    }
}
