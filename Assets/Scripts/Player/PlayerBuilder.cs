using UnityEngine;

// Assembles the player: body, camera, and all player scripts.
public static class PlayerBuilder
{
    public static PlayerHealth Create(Vector3 position, float yaw, TuningConfig t)
    {
        var root = new GameObject("Player");
        root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

        var cc = root.AddComponent<CharacterController>();
        cc.height = t.playerHeight;
        cc.radius = t.playerRadius;
        cc.center = Vector3.up * (t.playerHeight / 2f);

        var head = new GameObject("Head");
        head.tag = "MainCamera";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = Vector3.up * t.eyeHeight;
        var cam = head.AddComponent<Camera>();
        cam.fieldOfView = GameSettings.Current.fieldOfView;
        cam.nearClipPlane = 0.05f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = t.fogColor;
        head.AddComponent<AudioListener>();
        head.AddComponent<CameraOffsets>();

        Game.Player = root.transform;
        Game.Camera = cam;
        var health = root.AddComponent<PlayerHealth>();
        Game.PlayerHealth = health;

        root.AddComponent<PlayerUpgrades>();
        Game.Charges = root.AddComponent<ArcaneCharges>();
        root.AddComponent<PlayerLevel>();
        Game.Buzz = root.AddComponent<BuzzMeter>();
        root.AddComponent<SpellSwap>();
        root.AddComponent<PlayerMotor>();
        root.AddComponent<LandingFeedback>();
        root.AddComponent<PlayerLook>().head = head.transform;
        root.AddComponent<PlayerShooter>();
        root.AddComponent<VortexCaster>();
        root.AddComponent<FireWisps>();
        root.AddComponent<PoisonTrail>();
        return health;
    }
}
