using System.Collections.Generic;
using UnityEngine;

// Short text that pops up at a spot in the world and floats up while fading ("RING OUT!").
public class WorldPopup : MonoBehaviour
{
    class Popup { public Vector3 position; public string text; public Color color; public float age; }

    const float Lifetime = 1.2f;
    static readonly List<Popup> popups = new List<Popup>();
    GUIStyle style;

    void Awake() => popups.Clear();

    public static void Show(Vector3 position, string text, Color color)
    {
        popups.Add(new Popup { position = position, text = text, color = color });
    }

    void Update()
    {
        for (int i = popups.Count - 1; i >= 0; i--)
        {
            popups[i].age += Time.deltaTime;
            if (popups[i].age >= Lifetime) popups.RemoveAt(i);
        }
    }

    void OnGUI()
    {
        if (popups.Count == 0 || Game.Camera == null || PauseMenu.IsOpen) return;
        if (style == null) style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.045f);

        foreach (var p in popups)
        {
            float k = p.age / Lifetime;
            Vector3 s = Game.Camera.WorldToScreenPoint(p.position + Vector3.up * (k * 1.2f));
            if (s.z <= 0f) continue;
            float scale = 1f + 0.4f * Mathf.Clamp01(1f - k * 6f); // starts big, settles
            var rect = new Rect(s.x - 200f, Screen.height - s.y - 30f * scale, 400f, 60f * scale);
            GUI.color = new Color(0f, 0f, 0f, (1f - k) * 0.8f);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), p.text, style);
            GUI.color = new Color(p.color.r, p.color.g, p.color.b, 1f - k * k);
            GUI.Label(rect, p.text, style);
        }
        GUI.color = Color.white;
    }
}
