using UnityEngine;

// Everything that nudges the camera (collapse shake, landing dip) adds its offset here;
// this applies the sum on top of the eye height, so the effects never overwrite each other.
// The comfort settings scale them: screen shake for Shake, head bob for Dip.
public class CameraOffsets : MonoBehaviour
{
    public static Vector3 Shake;
    public static Vector3 Dip;

    void Awake() => Shake = Dip = Vector3.zero;

    void LateUpdate() => transform.localPosition = Vector3.up * Game.Tuning.eyeHeight + Shake * GameSettings.Shake + Dip * GameSettings.Bob;
}
