using System.Collections.Generic;
using UnityEngine;

// Creates solid-colour URP materials in code, all copied from one template material.
public static class Mats
{
    static readonly Dictionary<(Color, float), Material> cache = new Dictionary<(Color, float), Material>();

    // Set by GameBootstrap: one URP Lit material that every gray-box material is copied from.
    // Referencing a real material keeps the shader in builds.
    public static Material Template;

    // A new material the caller owns (use when you want to change its colour later).
    public static Material New(Color color, bool emissive = false)
    {
        var m = new Material(Template);
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", 0.15f);
        if (emissive)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }
        return m;
    }

    // Shared, cached plain colour.
    public static Material Solid(Color color) => Get(color, 0f);

    // Shared, cached glowing colour (glow > 1 triggers bloom).
    public static Material Glow(Color color, float intensity) => Get(color, intensity);

    public static void SetEmission(Material m, Color color) => m.SetColor("_EmissionColor", color);

    static Material Get(Color color, float glow)
    {
        if (cache.TryGetValue((color, glow), out var m) && m != null) return m;
        m = New(color, glow > 0f);
        if (glow > 0f) SetEmission(m, color * glow);
        cache[(color, glow)] = m;
        return m;
    }
}
