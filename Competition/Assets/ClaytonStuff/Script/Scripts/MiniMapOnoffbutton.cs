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

    // Scale values
    private Vector3 activeScale = new Vector3(1.1f, 1.1f, 1f);
    private Vector3 defaultScale = new Vector3(1f, 1f, 1f);

    private void Start()
    {
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

            // Scale active button
            onButton.transform.localScale = activeScale;
            offButton.transform.localScale = defaultScale;
        }
        else
        {
            onButtonImage.sprite = onDefaultSprite;
            offButtonImage.sprite = offActiveSprite;

            // Scale active button
            onButton.transform.localScale = defaultScale;
            offButton.transform.localScale = activeScale;
        }
    }
}
