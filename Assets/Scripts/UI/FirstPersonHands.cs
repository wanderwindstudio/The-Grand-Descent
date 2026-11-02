using UnityEngine;

// The wizard's hands as screen overlays (they can never clip into walls).
// Left hand bottom-left (idle pose, open cast pose for a moment after each spell), right hand with the staff
// bottom-right. Everything else is faked:
// breathing sway, walk bob, lag behind fast turns, cast pushes, dash swing, hit jolt, Buzz effects.
// The staff crystal glows with Arcane Charges, and the hands take on the light around the player.
public class FirstPersonHands : MonoBehaviour
{
    public static FirstPersonHands Instance { get; private set; }

    // Fingertips of the left hand in GUI coordinates (y down). Spells spawn here.
    public Vector2 FingertipGuiPosition { get; private set; }

    Rect leftRect, rightRect;
    Vector2 leftOffset, rightOffset;   // in fractions of screen height
    Vector2 lookLag;
    float leftScale = 1f;
    float bobPhase, prevYaw, prevPitch;
    float castT = 99f, poseT = 99f, throwT = 99f, dashT = 99f, hitT = 99f, flashT = 99f;
    float hitSide;     // -1 = hit from the left, 1 = from the right
    float flashRoll;
    float crashBlend;
    Color lightTint = Color.white;
    float nextLightSample;
    Texture2D glowTexture;
    CharacterController body;

    void Awake()
    {
        Instance = this;
        glowTexture = MakeGlowTexture(64);
    }

    void Start()
    {
        body = Game.Player.GetComponent<CharacterController>();
        Game.PlayerHealth.Damaged += OnDamaged;
        prevYaw = Game.Camera.transform.eulerAngles.y;
        prevPitch = Game.Camera.transform.eulerAngles.x;
    }

    // Events from the spells and the player
    public void OnMissileCast() { castT = 0f; poseT = 0f; flashT = 0f; flashRoll = Random.Range(0f, 360f); }
    public void OnVortexThrow() { throwT = 0f; poseT = 0f; flashT = 0f; flashRoll = Random.Range(0f, 360f); }

    bool CastPose => Art.HandLeftCast != null && poseT < Game.Tuning.handCastPoseTime;
    public void OnDash() => dashT = 0f;

    void OnDamaged(Vector3 source, float amount)
    {
        hitT = 0f;
        Vector3 to = source - Game.Player.position;
        to.y = 0f;
        hitSide = Mathf.Sign(Vector3.SignedAngle(Game.Player.forward, to, Vector3.up));
    }

    void LateUpdate()
    {
        var t = Game.Tuning;
        var buzz = Game.Buzz;
        float dt = Time.deltaTime;
        float h = Screen.height, w = Screen.width;

        bool crash = buzz.Current == BuzzMeter.Phase.Crash;
        bool transcendent = buzz.Current == BuzzMeter.Phase.Transcendent;
        crashBlend = Mathf.MoveTowards(crashBlend, crash ? 1f : 0f, dt * 2f);
        float pace = Mathf.Lerp(1f, t.handCrashSluggishness, crashBlend); // everything slows during the Crash
        float buzzBoost = 1f + buzz.Normalized * t.handBuzzBoost;

        castT += dt; poseT += dt; throwT += dt; dashT += dt; hitT += dt; flashT += dt;

        // Breathing sway
        float time = Time.time * t.handSwaySpeed * pace;
        Vector2 sway = new Vector2(Mathf.Cos(time * 0.5f), Mathf.Sin(time)) * (t.handSwayAmount * buzzBoost);

        // Walking bob, the two hands slightly out of phase
        Vector3 v = body.velocity;
        v.y = 0f;
        float speed01 = Mathf.Clamp(v.magnitude / Mathf.Max(0.1f, t.moveSpeed), 0f, 1.5f);
        bobPhase += dt * t.handBobSpeed * 2f * Mathf.PI * speed01 * pace;
        float bob = t.handBobAmount * buzzBoost * Mathf.Min(1f, speed01);
        Vector2 bobLeft = new Vector2(Mathf.Cos(bobPhase) * 0.5f, Mathf.Abs(Mathf.Sin(bobPhase))) * bob;
        Vector2 bobRight = new Vector2(Mathf.Cos(bobPhase + 0.8f) * 0.5f, Mathf.Abs(Mathf.Sin(bobPhase + 0.8f))) * bob;

        // Lag behind fast turns, then catch up
        Vector3 e = Game.Camera.transform.eulerAngles;
        float dYaw = Mathf.DeltaAngle(prevYaw, e.y), dPitch = Mathf.DeltaAngle(prevPitch, e.x);
        prevYaw = e.y;
        prevPitch = e.x;
        // Turn right: hands drift left. Look up: hands drift down (pitch shrinks when looking up).
        if (Time.timeScale > 0f) lookLag += new Vector2(-dYaw, -dPitch) * t.handLookLag;
        lookLag = Vector2.ClampMagnitude(lookLag, t.handLookLagMax);
        lookLag = Vector2.Lerp(lookLag, Vector2.zero, 1f - Mathf.Exp(-t.handCatchUpSpeed * pace * dt));

        // Short moves: missile push, vortex throw, dash swing, hit jolt
        float cast = Pulse(castT, t.handCastPushTime / pace) * t.handCastPush;
        float throwPush = Pulse(throwT, t.handThrowTime / pace) * t.handThrowPush;
        float push = Mathf.Max(cast, throwPush);
        float dash = Pulse(dashT, t.handDashTime / pace) * t.handDashSwing;
        float jolt = Mathf.Clamp01(1f - hitT / t.handHitJoltTime) * t.handHitJolt;

        Vector2 common = sway + lookLag
                         + new Vector2(-hitSide * jolt, jolt * 0.5f)                  // knocked away from the hit
                         + new Vector2(0f, t.handCrashDroop * crashBlend);            // Crash: hands hang lower
        if (transcendent) common += Random.insideUnitCircle * t.handTranscendenceTremble;

        leftOffset = common + bobLeft + new Vector2(push * 0.6f, -push) + new Vector2(-dash * 0.5f, dash);
        rightOffset = common + bobRight + new Vector2(dash * 0.5f, dash);
        leftScale = 1f + push * 1.5f; // pushing forward = slightly bigger

        // Layout
        float size = h * t.handScreenHeight;
        float ls = size * leftScale;
        leftRect = new Rect(leftOffset.x * h, h - ls + leftOffset.y * h, ls, ls);
        rightRect = new Rect(w - size + rightOffset.x * h, h - size + rightOffset.y * h, size, size);
        // With cast art, spells always leave from its flare: the hand opens the moment a spell goes.
        Vector2 tip = Art.HandLeftCast != null ? t.castFingertip : t.leftFingertip;
        FingertipGuiPosition = new Vector2(leftRect.x + tip.x * ls, leftRect.y + tip.y * ls);

        if (Time.time >= nextLightSample)
        {
            nextLightSample = Time.time + 0.1f;
            Color target = SampleLight();
            lightTint = Color.Lerp(lightTint, target, 0.5f);
        }
    }

    // 0 -> 1 -> 0 over the duration (a quick out-and-back).
    static float Pulse(float elapsed, float duration)
    {
        if (elapsed >= duration) return 0f;
        return Mathf.Sin(Mathf.PI * elapsed / duration);
    }

    // How lit the player is: ambient light plus every nearby point light, clamped to a sensible range.
    Color SampleLight()
    {
        var t = Game.Tuning;
        Vector3 pos = Game.Camera.transform.position;
        Color c = RenderSettings.ambientLight;
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) // sampled 10x a second, so this is cheap enough
        {
            if (!l.isActiveAndEnabled || l.type != LightType.Point) continue;
            float d = Vector3.Distance(pos, l.transform.position);
            if (d >= l.range) continue;
            float att = 1f - d / l.range;
            c += l.color * (l.intensity * att * att * t.handLightGain);
        }
        return new Color(
            Mathf.Clamp(c.r, t.handLightMin, t.handLightMax),
            Mathf.Clamp(c.g, t.handLightMin, t.handLightMax),
            Mathf.Clamp(c.b, t.handLightMin, t.handLightMax), 1f);
    }

    void OnGUI()
    {
        var t = Game.Tuning;
        if (!t.showHands || RunManager.Instance.Current != RunManager.State.Playing) return;
        GUI.depth = 10; // behind the HUD

        // Cast flash at the fingertips (drawn behind the hand so it seems to come from the palm).
        // The cast art has its own flare, so this is only for the idle-only fallback.
        if (Art.HandLeftCast == null && flashT < t.castFlashDuration)
        {
            float k = flashT / t.castFlashDuration;
            float s = Screen.height * t.castFlashSize * (0.5f + k);
            Vector2 p = FingertipGuiPosition;
            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(flashRoll, p);
            GUI.color = new Color(1f, 1f, 1f, 1f - k);
            GUI.DrawTexture(new Rect(p.x - s / 2f, p.y - s / 2f, s, s), Art.ArcaneImpact);
            GUI.matrix = old;
        }

        GUI.color = lightTint;
        GUI.DrawTexture(leftRect, CastPose ? Art.HandLeftCast : Art.HandLeft);
        GUI.DrawTexture(rightRect, Art.HandRight);

        // Staff crystal glow: brighter and bigger with each Arcane Charge, pulsing at max.
        var charges = Game.Charges;
        float c01 = charges.Max > 0 ? (float)charges.Charges / charges.Max : 0f;
        if (c01 > 0f)
        {
            float pulse = charges.AtMax ? 0.85f + 0.15f * Mathf.Sin(Time.time * 10f) : 1f;
            float gs = rightRect.width * t.staffCrystalGlowSize * (0.6f + 0.6f * c01) * pulse;
            Vector2 cp = new Vector2(rightRect.x + t.staffCrystal.x * rightRect.width, rightRect.y + t.staffCrystal.y * rightRect.height);
            Color glow = Palette.Arcane;
            glow.a = Mathf.Lerp(0.25f, 0.95f, c01) * pulse;
            GUI.color = glow;
            GUI.DrawTexture(new Rect(cp.x - gs / 2f, cp.y - gs / 2f, gs, gs), glowTexture);
        }
        GUI.color = Color.white;
    }

    // A soft round glow, bright in the middle.
    static Texture2D MakeGlowTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f, dy = (y + 0.5f) / size - 0.5f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        tex.Apply();
        return tex;
    }
}
