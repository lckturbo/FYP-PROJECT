using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIButtonPopEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Pop Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 1.2f;
    [SerializeField] private float animSpeed = 8f;

    [Header("Image Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.95f, 0.85f);
    [SerializeField] private Color clickColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private float colorLerpSpeed = 10f;

    [Header("Text Color Settings")]
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color hoverTextColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color clickTextColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Sprite Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite clickSprite;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    private Image image;
    private Color targetImageColor;
    private Color targetTextColor;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        image = GetComponent<Image>();
        if (image != null)
        {
            image.color = normalColor;
            if (normalSprite != null)
                image.sprite = normalSprite;
        }

        if (buttonText != null)
            buttonText.color = normalTextColor;

        targetImageColor = normalColor;
        targetTextColor = normalTextColor;
    }

    private void Update()
    {
        // Smoothly animate scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animSpeed);

        // Smoothly animate colors
        if (image != null)
            image.color = Color.Lerp(image.color, targetImageColor, Time.unscaledDeltaTime * colorLerpSpeed);

        if (buttonText != null)
            buttonText.color = Color.Lerp(buttonText.color, targetTextColor, Time.unscaledDeltaTime * colorLerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
        targetImageColor = hoverColor;
        targetTextColor = hoverTextColor;

        if (image != null && hoverSprite != null)
            image.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
        targetImageColor = normalColor;
        targetTextColor = normalTextColor;

        if (image != null && normalSprite != null)
            image.sprite = normalSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickPop());
    }

    private System.Collections.IEnumerator ClickPop()
    {
        targetScale = originalScale * clickScale;
        targetImageColor = clickColor;
        targetTextColor = clickTextColor;

        if (image != null && clickSprite != null)
            image.sprite = clickSprite;

        yield return new WaitForSecondsRealtime(0.1f);

        targetScale = isHovering ? originalScale * hoverScale : originalScale;
        targetImageColor = isHovering ? hoverColor : normalColor;
        targetTextColor = isHovering ? hoverTextColor : normalTextColor;

        if (image != null)
            image.sprite = isHovering && hoverSprite != null ? hoverSprite : normalSprite;
    }
}
