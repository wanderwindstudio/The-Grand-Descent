using System.IO;
using UnityEditor;
using UnityEngine;

// Menu: Grand Descent > Prepare Menu Art.
// Many menu images have wide transparent margins. This writes trimmed copies (cropped to the visible artwork,
// scaled down to at most 1024 px) into Assets/Art/Generated/UI and sets their 9-slice borders, so frames and
// buttons stretch without bending their corners. The source PNGs are never changed.
// Run it again after replacing any of the source images.
public static class MenuArtPrep
{
    const string SourceFolder = "Assets/Art/UI/Menu/";
    public const string OutputFolder = "Assets/Art/Generated/UI/";
    const int MaxSize = 1024;

    // name, 9-slice border on the trimmed source image in pixels (left, bottom, right, top); zero = no slicing.
    static readonly (string name, Vector4 border)[] Pieces =
    {
        ("ui_button_normal_01", new Vector4(230, 200, 230, 200)),   // iron corner fittings
        ("ui_button_hover_01", new Vector4(230, 200, 230, 200)),
        ("ui_panel_frame_01", new Vector4(215, 215, 215, 215)),     // whole iron corner plates
        ("ui_saveSlot_frame_01", new Vector4(200, 130, 230, 90)),   // ticket notches, torn top and bottom
        ("ui_slider_track_01", new Vector4(90, 0, 90, 0)),          // rounded groove ends
        ("ui_slider_knob_01", Vector4.zero),
        ("ui_menu_logo_01", Vector4.zero),
    };

    public static string OutputPath(string name) => OutputFolder + name + "_trim.png";

    [MenuItem("Grand Descent/Prepare Menu Art")]
    public static void PrepareAll()
    {
        Directory.CreateDirectory(OutputFolder);
        foreach (var (name, border) in Pieces)
        {
            string source = SourceFolder + name + ".png";
            if (!File.Exists(source)) { Debug.LogWarning("Menu art missing: " + source); continue; }
            Trim(source, OutputPath(name), border);
        }
        AssetDatabase.Refresh();
        Debug.Log("Menu art prepared in " + OutputFolder);
    }

    static void Trim(string source, string output, Vector4 border)
    {
        var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        src.LoadImage(File.ReadAllBytes(source));
        var px = src.GetPixels32();
        int w = src.width, h = src.height;

        // Bounds of everything that isn't (almost) fully transparent.
        int minX = w, minY = h, maxX = -1, maxY = -1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (px[y * w + x].a > 8)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
        if (maxX < 0) { Debug.LogWarning(source + " is fully transparent"); return; }
        int cw = maxX - minX + 1, ch = maxY - minY + 1;

        // Scale down with a box filter (each output pixel averages the source pixels it covers).
        float scale = Mathf.Min(1f, (float)MaxSize / Mathf.Max(cw, ch));
        int ow = Mathf.Max(1, Mathf.RoundToInt(cw * scale)), oh = Mathf.Max(1, Mathf.RoundToInt(ch * scale));
        var outPx = new Color32[ow * oh];
        float sx = (float)cw / ow, sy = (float)ch / oh;
        for (int y = 0; y < oh; y++)
            for (int x = 0; x < ow; x++)
            {
                int x0 = minX + Mathf.FloorToInt(x * sx), x1 = minX + Mathf.Max(Mathf.FloorToInt(x * sx) + 1, Mathf.FloorToInt((x + 1) * sx));
                int y0 = minY + Mathf.FloorToInt(y * sy), y1 = minY + Mathf.Max(Mathf.FloorToInt(y * sy) + 1, Mathf.FloorToInt((y + 1) * sy));
                float r = 0, g = 0, b = 0, a = 0; int n = 0;
                for (int yy = y0; yy < y1 && yy <= maxY; yy++)
                    for (int xx = x0; xx < x1 && xx <= maxX; xx++)
                    {
                        var c = px[yy * w + xx];
                        float ca = c.a / 255f;
                        r += c.r * ca; g += c.g * ca; b += c.b * ca; a += ca; n++;
                    }
                outPx[y * ow + x] = a > 0f
                    ? new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), (byte)(255f * a / n))
                    : new Color32(0, 0, 0, 0);
            }

        var tex = new Texture2D(ow, oh, TextureFormat.RGBA32, false);
        tex.SetPixels32(outPx);
        File.WriteAllBytes(output, tex.EncodeToPNG());
        Object.DestroyImmediate(src);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(output, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(output);
        importer.spriteBorder = border * scale;
        importer.SaveAndReimport();
        Debug.Log($"{Path.GetFileName(output)}: trimmed to {cw}x{ch}, saved at {ow}x{oh}, border {border * scale}");
    }
}
