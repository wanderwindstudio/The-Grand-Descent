using System.Collections.Generic;
using UnityEngine;

public enum PortalKind { Extract, Descend }

// A glowing standing disc. Walk into it to extract (blue) or go one floor deeper (red).
public class Portal : MonoBehaviour
{
    public static readonly List<Portal> All = new List<Portal>();

    public PortalKind Kind { get; private set; }
    Material material;
    Light glow;
    Color color;

    public static Portal Create(PortalKind kind, Vector3 position, Transform parent, float yaw = 0f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = kind + " Portal";
        Object.DestroyImmediate(go.GetComponent<Collider>()); // you walk through it
        go.transform.SetParent(parent, false);
        go.transform.position = position + Vector3.up * 1.6f;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(90f, 0f, 0f); // stand the disc up, facing along the spot
        go.transform.localScale = new Vector3(2.4f, 0.08f, 3f);

        var portal = go.AddComponent<Portal>();
        portal.Kind = kind;
        portal.color = kind == PortalKind.Extract ? Palette.ExtractPortal : Palette.DescendPortal;
        portal.material = Mats.New(portal.color, emissive: true);
        go.GetComponent<Renderer>().sharedMaterial = portal.material;

        portal.glow = new GameObject("Glow").AddComponent<Light>();
        portal.glow.transform.SetParent(go.transform, false);
        portal.glow.type = LightType.Point;
        portal.glow.color = portal.color;
        portal.glow.range = 10f;
        return portal;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 4f);
        Mats.SetEmission(material, color * (3f * pulse));
        glow.intensity = 3f * pulse;

        Vector3 d = Game.Player.position - transform.position;
        d.y = 0f;
        if (d.magnitude <= Game.Tuning.portalRadius) RunManager.Instance.RequestPortal(Kind);
    }

    public static string Label(PortalKind kind) => kind == PortalKind.Extract ? "EXTRACT\n(bank your gold)" : "DESCEND\n(harder, richer)";
}
