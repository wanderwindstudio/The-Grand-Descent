using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// A pipe shrine (the sleepy stone altar sprite, or a glowing purple pillar without art).
// Press Interact nearby for a big hit of Buzz. Recharges after a while.
public class PipeShrine : MonoBehaviour
{
    public static readonly List<PipeShrine> All = new List<PipeShrine>();

    Material material;
    Light glow;
    float rechargeLeft;

    public bool Ready => rechargeLeft <= 0f;

    public static PipeShrine Create(Vector3 position, Transform parent)
    {
        var art = EnvironmentArt.Current;
        if (art != null && art.pipeShrine != null && PropSprites.Ready) return CreateSprite(position, parent, art.pipeShrine);

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

    // The sprite stands on the floor; an invisible capsule keeps it solid for shots and goblins.
    static PipeShrine CreateSprite(Vector3 position, Transform parent, Texture2D texture)
    {
        var go = new GameObject("Pipe Shrine");
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        var shrine = go.AddComponent<PipeShrine>();
        PropSprites.Billboard("Sprite", go.transform, position, texture, Game.Tuning.shrineSpriteHeight, out Renderer r);
        shrine.material = r.sharedMaterial;
        shrine.glowScale = 0.06f; // tint the art only slightly; the light does the rest
        shrine.sprite = r;

        var capsule = go.AddComponent<CapsuleCollider>();
        capsule.radius = 0.45f;
        capsule.height = 1.4f;
        capsule.center = Vector3.up * 0.7f;
        var obstacle = go.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.radius = 0.5f;
        obstacle.height = 1.4f;
        obstacle.center = Vector3.up * 0.7f;
        obstacle.carving = true;

        shrine.glow = new GameObject("Glow").AddComponent<Light>();
        shrine.glow.transform.SetParent(go.transform, false);
        shrine.glow.transform.localPosition = Vector3.up * 1.4f;
        shrine.glow.type = LightType.Point;
        shrine.glow.color = Palette.Shrine;
        shrine.glow.range = 7f;
        return shrine;
    }

    float glowScale = 1f;
    Renderer sprite;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        var t = Game.Tuning;
        if (!Ready)
        {
            rechargeLeft -= Time.deltaTime;
            Mats.SetEmission(material, Palette.Shrine * (0.1f * glowScale));
            if (sprite != null) material.SetColor("_BaseColor", new Color(0.55f, 0.55f, 0.6f)); // spent: greyed out
            glow.intensity = 0.2f;
            return;
        }

        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 3f);
        Mats.SetEmission(material, Palette.Shrine * (2.5f * pulse * glowScale));
        if (sprite != null) material.SetColor("_BaseColor", Color.white);
        glow.intensity = 2.5f * pulse;

        if (Controls.Interact.WasPressedThisFrame() && PlayerInRange() && Game.Buzz.Current == BuzzMeter.Phase.Normal && !Game.InputBlocked)
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
