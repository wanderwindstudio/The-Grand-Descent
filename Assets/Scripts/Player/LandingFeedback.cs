using UnityEngine;

// Landing feel: a quick camera dip and a soft thud, bigger after a long (damaging) fall.
// The hands bounce too (FirstPersonHands listens to the same event).
public class LandingFeedback : MonoBehaviour
{
    AudioSource audioSource;
    AudioClip thud, heavyThud;
    float dipAmount, dipT = 99f;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        thud = ToneGenerator.Thud("Land", 95f, 0.12f);
        heavyThud = ToneGenerator.Thud("Heavy Land", 60f, 0.3f);
        GetComponent<PlayerMotor>().Landed += OnLanded;
    }

    void OnLanded(float fall, bool big)
    {
        var t = Game.Tuning;
        if (fall < 0.25f) return; // stepping down a stair, not a landing
        float k = Mathf.Clamp01(fall / Mathf.Max(0.1f, t.jumpHeight * 1.5f));
        dipAmount = big ? t.bigLandingDip : t.landingDip * Mathf.Lerp(0.4f, 1f, k);
        dipT = 0f;
        audioSource.PlayOneShot(big ? heavyThud : thud, t.landingSoundVolume * (big ? 1.6f : Mathf.Lerp(0.5f, 1f, k)) * GameSettings.Sfx);
        FirstPersonHands.Instance?.OnLand(big ? 1.6f : Mathf.Lerp(0.5f, 1f, k));
    }

    void Update()
    {
        var t = Game.Tuning;
        dipT += Time.deltaTime;
        float d = DipCurve(dipT / Mathf.Max(0.01f, t.landingDipTime));
        CameraOffsets.Dip = Vector3.down * (dipAmount * d);
    }

    // Down fast, back up slowly: 0 -> 1 in the first fifth, then eases back to 0.
    static float DipCurve(float k)
    {
        if (k >= 1f) return 0f;
        if (k < 0.2f) return k / 0.2f;
        float r = (k - 0.2f) / 0.8f;
        return 1f - r * r * (3f - 2f * r);
    }
}
