using System.Collections.Generic;
using UnityEngine;

// Purple scorch marks where arcane blasts hit walls and floors. Each fades out after a few seconds;
// only a limited number exist at once (the oldest is reused first).
public class ScorchMarks : MonoBehaviour
{
    class Mark
    {
        public Renderer renderer;
        public float age;
    }

    static ScorchMarks instance;
    readonly List<Mark> marks = new List<Mark>();
    int next;

    void Awake() => instance = this;

    public static void Add(Vector3 point, Vector3 normal, float size)
    {
        var tex = EnvironmentArt.Current != null ? EnvironmentArt.Current.scorch : null;
        if (instance == null || tex == null || Art.FxMaterial == null) return;
        instance.Place(point, normal, size, tex);
    }

    // An explosion on the ground: a mark straight below it.
    public static void OnFloor(Vector3 position, float size)
    {
        if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, ~(1 << 2), QueryTriggerInteraction.Ignore))
            Add(hit.point, hit.normal, size);
    }

    void Place(Vector3 point, Vector3 normal, float size, Texture2D tex)
    {
        var t = Game.Tuning;
        Mark mark;
        if (marks.Count < t.scorchMaxCount)
        {
            var quad = Art.CreateFxQuad("Scorch", tex);
            quad.transform.SetParent(transform, false);
            mark = new Mark { renderer = quad };
            marks.Add(mark);
        }
        else
        {
            mark = marks[next % marks.Count];
            next++;
        }

        Vector3 up = Mathf.Abs(normal.y) > 0.9f ? Vector3.forward : Vector3.up;
        var tr = mark.renderer.transform;
        // Each mark sits a hair further out than the last, so overlapping ones don't flicker.
        tr.SetPositionAndRotation(point + normal * (0.02f + 0.0005f * (next % 20)), Quaternion.LookRotation(-normal, up) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        tr.localScale = Vector3.one * size * Random.Range(0.8f, 1.2f);
        mark.age = 0f;
        mark.renderer.gameObject.SetActive(true);
    }

    void Update()
    {
        var t = Game.Tuning;
        foreach (var m in marks)
        {
            if (!m.renderer.gameObject.activeSelf) continue;
            m.age += Time.deltaTime;
            float left = 1f - m.age / t.scorchLifetime;
            if (left <= 0f) { m.renderer.gameObject.SetActive(false); continue; }
            // Full for most of its life, then a gentle fade.
            float alpha = Mathf.Clamp01(left * 3f) * t.scorchOpacity;
            Art.SetFxColor(m.renderer, Color.white, t.scorchBrightness, alpha);
        }
    }

    // Leaving a floor: clear them all.
    public static void Clear()
    {
        if (instance == null) return;
        foreach (var m in instance.marks) m.renderer.gameObject.SetActive(false);
    }
}
