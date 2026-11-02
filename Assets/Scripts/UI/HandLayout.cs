using System.Collections.Generic;
using UnityEngine;

// Where one hand image sits on screen. The hand art's sleeve runs off the bottom and outer side of its canvas,
// so a hand moved toward the centre would show a hard cut. The hand is tilted around the middle of that sleeve
// cut and lowered just enough that the whole cut stays below the screen's bottom edge, then slid sideways until
// its innermost point is a set distance from the crosshair. Everything is a fraction of the screen,
// so it looks the same at any resolution or aspect ratio.
public class HandLayout
{
    const int Grid = 128; // resolution the art's outline is measured at

    readonly bool left;
    Vector2 cutInner, cutOuter, corner; // uv, y down: where the sleeve leaves the canvas
    readonly List<Vector2> innerEdge = new List<Vector2>(); // the innermost opaque point of each row (uv)

    public float Size { get; private set; }
    public float Angle { get; private set; }  // degrees, clockwise
    public Vector2 Pivot { get; private set; } // screen position of the middle of the sleeve cut
    Vector2 PivotUv => (cutInner + cutOuter) / 2f;
    Rect Rect => new Rect(Pivot.x - PivotUv.x * Size, Pivot.y - PivotUv.y * Size, Size, Size);

    // Measures the outline of all poses together, so every pose fits the same placement.
    public HandLayout(bool left, params Texture2D[] poses)
    {
        this.left = left;
        float inner = left ? 0f : 1f, top = 1f;
        bool touchesBottom = false;
        foreach (var tex in poses)
        {
            if (tex == null) continue;
            var px = Art.ReadPixels(tex, Grid, Grid);
            bool Opaque(int x, int row) => px[(Grid - 1 - row) * Grid + x].a >= 128; // row 0 = top

            for (int x = 0; x < Grid; x++)
                if (Opaque(x, Grid - 1))
                {
                    touchesBottom = true;
                    inner = left ? Mathf.Max(inner, (x + 1f) / Grid) : Mathf.Min(inner, x / (float)Grid);
                }
            int outerX = left ? 0 : Grid - 1;
            for (int row = 0; row < Grid; row++)
                if (Opaque(outerX, row)) { top = Mathf.Min(top, row / (float)Grid); break; }

            for (int row = 0; row < Grid; row += 2)
            {
                int x = left ? Grid - 1 : 0, step = left ? -1 : 1;
                while (x >= 0 && x < Grid && !Opaque(x, row)) x += step;
                if (x >= 0 && x < Grid) innerEdge.Add(new Vector2(left ? (x + 1f) / Grid : x / (float)Grid, (row + 0.5f) / Grid));
            }
        }
        if (!touchesBottom) { inner = left ? 0.45f : 0.55f; top = 0.8f; } // placeholder art: assume a typical sleeve

        float outer = left ? 0f : 1f;
        cutInner = new Vector2(inner, 1f);
        cutOuter = new Vector2(outer, top);
        corner = new Vector2(outer, 1f);
    }

    // size in pixels, tilt in degrees, centreGap in screen widths, verticalOffset (0 or less) and motion in screen heights.
    public void Place(float w, float h, float size, float tilt, float centreGap, float verticalOffset, Vector2 motion)
    {
        Size = size;
        Angle = tilt;

        // Lowest point at which the whole sleeve cut is below the bottom edge
        float minY = Mathf.Max(h - Rel(cutInner).y, Mathf.Max(h - Rel(cutOuter).y, h - Rel(corner).y));
        float y = minY - Mathf.Min(0f, verticalOffset) * h;

        // Innermost visible point sits at the gap from the centre
        float extreme = 0f;
        bool any = false;
        foreach (var p in innerEdge)
        {
            Vector2 r = Rel(p);
            if (y + r.y >= h) continue; // below the screen, can't be seen
            extreme = !any ? r.x : left ? Mathf.Max(extreme, r.x) : Mathf.Min(extreme, r.x);
            any = true;
        }
        float gap = Mathf.Max(centreGap, Game.Tuning.handMinCenterGap) * w;
        float x = (left ? w / 2f - gap : w / 2f + gap) - extreme;

        Vector2 pos = new Vector2(x, y) + motion * h;
        pos.y = Mathf.Max(pos.y, minY); // motion never lifts the sleeve off the bottom edge
        Pivot = pos;
    }

    // A point of the image (uv, y down) on screen.
    public Vector2 ToScreen(Vector2 uv) => Pivot + Rel(uv);

    public void Draw(Texture texture)
    {
        Matrix4x4 old = GUI.matrix;
        GUIUtility.RotateAroundPivot(Angle, Pivot);
        GUI.DrawTexture(Rect, texture);
        GUI.matrix = old;
    }

    // Offset of an image point from the pivot, in pixels, after the tilt.
    Vector2 Rel(Vector2 uv)
    {
        Vector2 d = (uv - PivotUv) * Size;
        float a = Angle * Mathf.Deg2Rad, c = Mathf.Cos(a), s = Mathf.Sin(a);
        return new Vector2(d.x * c - d.y * s, d.x * s + d.y * c); // clockwise on screen (y points down)
    }
}
