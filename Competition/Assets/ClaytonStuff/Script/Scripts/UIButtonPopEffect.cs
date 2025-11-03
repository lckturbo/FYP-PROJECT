using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonPopEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Pop Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 1.2f;
    [SerializeField] private float animSpeed = 8f;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.95f, 0.85f); // slight warm tint
    [SerializeField] private Color clickColor = new Color(1f, 0.9f, 0.7f);  // stronger tint
    [SerializeField] private float colorLerpSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    private Image image;
    private Color targetColor;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        image = GetComponent<Image>();
        if (image != null)
            image.color = normalColor;

        targetColor = normalColor;
    }

    private void Update()
    {
        // Smoothly scale and color
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animSpeed);

        if (image != null)
            image.color = Color.Lerp(image.color, targetColor, Time.unscaledDeltaTime * colorLerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
        targetColor = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickPop());
    }

    private System.Collections.IEnumerator ClickPop()
    {
        targetScale = originalScale * clickScale;
        targetColor = clickColor;
        yield return new WaitForSecondsRealtime(0.1f);
        targetScale = isHovering ? originalScale * hoverScale : originalScale;
        targetColor = isHovering ? hoverColor : normalColor;
    }
}
