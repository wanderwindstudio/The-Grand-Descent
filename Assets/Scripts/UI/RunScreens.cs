using UnityEngine;

// The start screen and the end-of-run summary.
public class RunScreens : MonoBehaviour
{
    GUIStyle title, body;

    void OnGUI()
    {
        var run = RunManager.Instance;
        if (run.Current == RunManager.State.Playing) return;
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 48, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            body = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.UpperCenter, richText = true };
        }

        float w = Screen.width, h = Screen.height;
        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (run.Current == RunManager.State.StartScreen) DrawStart(w, h);
        else DrawSummary(run, w, h);
    }

    void DrawStart(float w, float h)
    {
        GUI.Label(new Rect(0, h * 0.25f, w, 70), "THE GRAND DESCENT", title);
        GUI.Label(new Rect(0, h * 0.45f, w, 120),
            $"Banked gold: {MetaProgress.BankedGold}   —   Runs: {MetaProgress.TotalRuns}\n\n<b>Press Enter to descend.</b>", body);
    }

    void DrawSummary(RunManager run, float w, float h)
    {
        GUI.Label(new Rect(0, h * 0.12f, w, 70), run.Extracted ? "EXTRACTED" : run.DiedToCollapse ? "CRUSHED BY THE COLLAPSE" : "YOU DIED", title);

        int minutes = Mathf.FloorToInt(run.RunDuration / 60f);
        int seconds = Mathf.FloorToInt(run.RunDuration % 60f);
        string bankNote = run.Extracted ? "all of it" : $"{Game.Tuning.deathBankFraction * 100f:0}% consolation";
        string text =
            $"Floor reached: {Game.Floor}\n" +
            $"Time: {minutes}:{seconds:00}\n" +
            $"Kills: {RunStats.Kills}\n" +
            $"Level reached: {run.LevelReached}\n" +
            $"Gold carried: {run.CarriedGold}\n" +
            $"Gold banked: {run.GoldBankedThisRun}  ({bankNote})\n" +
            $"Time above {Game.Tuning.buzzHighThreshold * 100f:0}% Buzz: {RunStats.SecondsAboveBuzzThreshold:0}s\n" +
            $"Transcendences: {RunStats.Transcendences}\n\n" +
            $"Total banked gold: {MetaProgress.BankedGold}   —   Runs: {MetaProgress.TotalRuns}\n\n" +
            "<b>Press Enter to continue.</b>";
        GUI.Label(new Rect(0, h * 0.25f, w, h * 0.7f), text, body);
    }
}
