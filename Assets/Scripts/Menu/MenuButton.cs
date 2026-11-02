using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A plank button: swaps to the glowing plank and grows a little while the mouse is over it.
// Runs on unscaled time, so it also works in the pause menu.
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    const float HoverScale = 1.06f;

    Image image;
    Sprite normal, hover;
    bool over;
    float scale = 1f;

    public TextMeshProUGUI Label { get; private set; }
    public Button Button => GetComponent<Button>();

    public void Setup(Image img, TextMeshProUGUI label, Sprite normalSprite, Sprite hoverSprite)
    {
        image = img;
        Label = label;
        normal = normalSprite;
        hover = hoverSprite;
    }

    public void SetLabel(string text) => Label.text = text;

    public void OnPointerEnter(PointerEventData e) => SetOver(true);
    public void OnPointerExit(PointerEventData e) => SetOver(false);
    public void OnSelect(BaseEventData e) => SetOver(true);
    public void OnDeselect(BaseEventData e) => SetOver(false);

    void OnDisable()
    {
        over = false;
        scale = 1f;
        transform.localScale = Vector3.one;
        if (image != null && normal != null) image.sprite = normal;
    }

    void SetOver(bool value)
    {
        if (value == over || !Button.interactable) return;
        over = value;
        if (normal != null) image.sprite = over && hover != null ? hover : normal;
        if (!normal) image.color = over ? new Color(0.4f, 0.26f, 0.14f, 0.95f) : new Color(0.25f, 0.16f, 0.1f, 0.95f);
        if (over) UiKit.PlayHover();
    }

    void Update()
    {
        scale = Mathf.MoveTowards(scale, over ? HoverScale : 1f, Time.unscaledDeltaTime * 0.8f);
        transform.localScale = Vector3.one * scale;
        Label.color = !Button.interactable ? UiKit.TextDim * 0.8f : over ? UiKit.Gold : UiKit.TextLight;
    }
}
