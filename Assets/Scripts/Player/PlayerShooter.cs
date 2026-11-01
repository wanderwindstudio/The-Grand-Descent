using UnityEngine;
using UnityEngine.InputSystem;

// Left mouse fires magic missiles from the camera. Hold to auto-fire.
public class PlayerShooter : MonoBehaviour
{
    SimplePool<Missile> pool;
    float nextFireTime;

    void Awake()
    {
        pool = new SimplePool<Missile>(CreateMissile);
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || Game.InputBlocked || Game.PlayerHealth.IsDead || Time.timeScale == 0f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return; // the click that grabs the mouse shouldn't shoot

        if (mouse.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / Game.Tuning.fireRate;
            Fire();
        }
    }

    void Fire()
    {
        var t = Game.Tuning;
        Transform cam = Game.Camera.transform;
        Vector3 origin = cam.position + cam.forward * t.missileSpawnOffset;
        pool.Get(origin).Launch(cam.forward, t.missileDamage, t.missilePierce);
    }

    Missile CreateMissile()
    {
        var t = Game.Tuning;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Missile";
        go.SetActive(false);
        DestroyImmediate(go.GetComponent<Collider>()); // must be gone before its first sphere cast
        go.transform.localScale = Vector3.one * t.missileVisualSize;
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = Mats.Glow(Palette.Missile, t.missileGlowIntensity);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var light = new GameObject("Glow").AddComponent<Light>();
        light.transform.SetParent(go.transform, false);
        light.type = LightType.Point;
        light.color = Palette.Missile;
        light.range = t.missileLightRange;
        light.intensity = t.missileLightIntensity;

        var missile = go.AddComponent<Missile>();
        missile.SetPool(pool);
        return missile;
    }
}
