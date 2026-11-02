using System.Collections.Generic;
using System.Text.RegularExpressions;

// Reads the intro script: one line per row, "scene | speaker | portrait | line".
// A speaker or portrait of "—" (or "-") means none: narration has neither.
public static class IntroScript
{
    public class Line
    {
        public string scene, speaker, portrait, text;
        public bool IsNarration => speaker == null;
        // All capitals with full stops ("BANG. BANG. BANG."): a small screen shake.
        public bool IsLoud => Regex.IsMatch(text, "[A-Z]") && !Regex.IsMatch(text, "[a-z]") && text.Contains(".");
    }

    public static List<Line> Parse(string source)
    {
        var lines = new List<Line>();
        if (string.IsNullOrEmpty(source)) return lines;
        int number = 0;
        foreach (string raw in source.Split('\n'))
        {
            number++;
            string row = raw.Trim();
            if (row.Length == 0 || row.StartsWith("#")) continue;
            string[] parts = row.Split(new[] { '|' }, 4);
            if (parts.Length < 4)
            {
                UnityEngine.Debug.LogWarning($"Intro script line {number} needs 4 parts (scene | speaker | portrait | line): {row}");
                continue;
            }
            lines.Add(new Line
            {
                scene = parts[0].Trim(),
                speaker = None(parts[1]),
                portrait = None(parts[2]),
                text = parts[3].Trim(),
            });
        }
        return lines;
    }

    static string None(string s)
    {
        s = s.Trim();
        return s.Length == 0 || s == "—" || s == "-" || s == "–" ? null : s;
    }
}
