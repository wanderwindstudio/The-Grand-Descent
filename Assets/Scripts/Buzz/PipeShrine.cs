using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// Glowing purple pillar. Press E nearby for a big hit of Buzz. Recharges after a while.
public class PipeShrine : MonoBehaviour
{
    public static readonly List<PipeShrine> All = new List<PipeShrine>();

    Material material;
    Light glow;
    float rechargeLeft;

    public bool Ready => rechargeLeft <= 0f;

    public static PipeShrine Create(Vector3 position, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Pipe Shrine";
        go.transform.SetParent(parent, false);
        go.transform.position = position + Vector3.up * 1.5f;
        go.transform.localScale = new Vector3(0.7f, 1.5f, 0.7f);
        var shrine = go.AddComponent<PipeShrine>();
        shrine.material = Mats.New(Palette.Shrine, emissive: true);
        go.GetComponent<Renderer>().sharedMaterial = shrine.material;

        var obstacle = go.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.carving = true;

        shrine.glow = new GameObject("Glow").AddComponent<Light>();
        shrine.glow.transform.SetParent(go.transform, false);
        shrine.glow.type = LightType.Point;
        shrine.glow.color = Palette.Shrine;
        shrine.glow.range = 7f;
        return shrine;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        var t = Game.Tuning;
        if (!Ready)
        {
            rechargeLeft -= Time.deltaTime;
            Mats.SetEmission(material, Palette.Shrine * 0.1f);
            glow.intensity = 0.2f;
            return;
        }

        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f);
        Mats.SetEmission(material, Palette.Shrine * (2.5f * pulse));
        glow.intensity = 2.5f * pulse;

        var kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame && PlayerInRange() && Game.Buzz.Current == BuzzMeter.Phase.Normal && !Game.InputBlocked)
        {
            Game.Buzz.Add(t.shrineBuzzAmount);
            rechargeLeft = t.shrineRechargeTime;
            if (Game.Buzz.Current == BuzzMeter.Phase.Normal) Hud.ShowMessage($"+{t.shrineBuzzAmount:0} BUZZ");
        }
    }

    public bool PlayerInRange()
    {
        Vector3 d = Game.Player.position - transform.position;
        d.y = 0f;
        return d.magnitude <= Game.Tuning.shrineInteractRange;
    }

    // The shrine the player is standing next to, or null.
    public static PipeShrine InRange()
    {
        foreach (var s in All)
            if (s.PlayerInRange()) return s;
        return null;
    }
}
