using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Pauses the game on level-up and offers random upgrade choices. Click one or press 1/2/3.
public class LevelUpScreen : MonoBehaviour
{
    PlayerLevel level;
    PlayerUpgrades upgrades;
    readonly List<UpgradeType> choices = new List<UpgradeType>();
    bool showing;
    GUIStyle titleStyle, cardStyle;

    void Start()
    {
        level = Game.Player.GetComponent<PlayerLevel>();
        upgrades = Game.Player.GetComponent<PlayerUpgrades>();
    }

    void Update()
    {
        if (Game.PlayerHealth.IsDead || RunManager.Instance.Current != RunManager.State.Playing) return;

        if (!showing && level.PendingLevelUps > 0) Open();
        if (!showing) return;

        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) Pick(0);
        else if (kb.digit2Key.wasPressedThisFrame) Pick(1);
        else if (kb.digit3Key.wasPressedThisFrame) Pick(2);
    }

    void Open()
    {
        showing = true;
        Time.timeScale = 0f;
        Game.InputBlocked = true;
        PlayerLook.UnlockCursor();
        RollChoices();
    }

    void RollChoices()
    {
        var pool = new List<UpgradeType>(PlayerUpgrades.All);
        choices.Clear();
        int count = Mathf.Min(Game.Tuning.upgradeChoices, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            choices.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    void Pick(int index)
    {
        if (index >= choices.Count) return;
        upgrades.Apply(choices[index]);
        Hud.ShowMessage(PlayerUpgrades.Name(choices[index]) + "!");
        level.ConsumeLevelUp();

        if (level.PendingLevelUps > 0) { RollChoices(); return; }

        showing = false;
        Time.timeScale = 1f;
        Game.InputBlocked = false;
        PlayerLook.LockCursor();
    }

    void OnGUI()
    {
        if (!showing) return;
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            cardStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, wordWrap = true, richText = true, alignment = TextAnchor.MiddleCenter };
        }

        float w = Screen.width, h = Screen.height;
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string extra = level.PendingLevelUps > 1 ? $"   (+{level.PendingLevelUps - 1} more)" : "";
        GUI.Label(new Rect(0, h * 0.2f, w, 60), $"LEVEL {level.Level - level.PendingLevelUps + 1}!{extra}", titleStyle);

        const float cardW = 260f, cardH = 170f, gap = 30f;
        float totalW = choices.Count * cardW + (choices.Count - 1) * gap;
        float x = (w - totalW) / 2f, y = h * 0.4f;
        for (int i = 0; i < choices.Count; i++)
        {
            var u = choices[i];
            int owned = upgrades.Stacks(u);
            string text = $"[{i + 1}]\n<b>{PlayerUpgrades.Name(u)}</b>\n\n{PlayerUpgrades.Description(u)}" + (owned > 0 ? $"\n\nOwned: {owned}" : "");
            if (GUI.Button(new Rect(x + i * (cardW + gap), y, cardW, cardH), text, cardStyle)) Pick(i);
        }
    }
}
