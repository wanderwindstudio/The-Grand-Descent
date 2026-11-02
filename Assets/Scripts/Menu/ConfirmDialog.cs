using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "Are you sure?" on top of everything else: a dark backdrop that blocks clicks, a question and two buttons.
public static class ConfirmDialog
{
    public static GameObject Show(Transform canvas, string question, string yesLabel, Action onYes, Action onNo = null)
    {
        var backdrop = UiKit.Image("Confirm", canvas, null, new Color(0f, 0f, 0f, 0.7f));
        UiKit.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;

        var panel = UiKit.Panel("Panel", backdrop.transform);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 440f));

        var text = UiKit.Text("Question", panel.transform, question, 44, UiKit.Font.Body, UiKit.TextLight, TextAlignmentOptions.Center);
        UiKit.Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(720f, 200f));

        var go = backdrop.gameObject;
        var yes = UiKit.Button("Yes", panel.transform, yesLabel, new Vector2(300f, 84f), () => { UnityEngine.Object.Destroy(go); onYes?.Invoke(); }, 40f);
        UiKit.Place((RectTransform)yes.transform, new Vector2(0.5f, 0f), new Vector2(-165f, 60f), new Vector2(300f, 84f));
        var no = UiKit.Button("No", panel.transform, "Cancel", new Vector2(300f, 84f), () => { UnityEngine.Object.Destroy(go); onNo?.Invoke(); }, 40f);
        UiKit.Place((RectTransform)no.transform, new Vector2(0.5f, 0f), new Vector2(165f, 60f), new Vector2(300f, 84f));
        no.Button.Select();
        return go;
    }
}
