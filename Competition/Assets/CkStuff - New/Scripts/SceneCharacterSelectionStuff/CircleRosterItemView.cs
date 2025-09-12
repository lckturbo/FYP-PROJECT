using UnityEngine;
using UnityEngine.UI;

public class CircleRosterItemView : MonoBehaviour
{
    [SerializeField] private Image portrait;           // child: Portrait
    [SerializeField] private CanvasGroup selectedGlow; // child: SelectedGlow
    [SerializeField] private Button button;

    public void Bind(NewCharacterDefinition def, System.Action onClick)
    {
        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());

        if (portrait) portrait.sprite = def.portrait ? def.portrait : def.normalArt;
        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        if (selectedGlow) selectedGlow.alpha = on ? 1f : 0f; // no SetActive
    }
}
