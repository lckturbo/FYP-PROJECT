using UnityEngine;
using UnityEngine.UI;

public class MiniMapOnoffbutton : MonoBehaviour
{
    [Header("Minimap Object")]
    public GameObject miniMap;

    [Header("Buttons")]
    public Button onButton;
    public Button offButton;

    [Header("Button Images")]
    public Image onButtonImage;
    public Image offButtonImage;

    [Header("Sprites")]
    public Sprite onActiveSprite;
    public Sprite onDefaultSprite;

    public Sprite offActiveSprite;
    public Sprite offDefaultSprite;

    private bool isMinimapOn = true;

    private void Start()
    {
        // Assign button events
        onButton.onClick.AddListener(TurnOnMiniMap);
        offButton.onClick.AddListener(TurnOffMiniMap);

        UpdateButtonVisual();
    }

    private void TurnOnMiniMap()
    {
        isMinimapOn = true;
        miniMap.SetActive(true);
        UpdateButtonVisual();
    }

    private void TurnOffMiniMap()
    {
        isMinimapOn = false;
        miniMap.SetActive(false);
        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (isMinimapOn)
        {
            onButtonImage.sprite = onActiveSprite;
            offButtonImage.sprite = offDefaultSprite;
        }
        else
        {
            onButtonImage.sprite = onDefaultSprite;
            offButtonImage.sprite = offActiveSprite;
        }
    }
}
