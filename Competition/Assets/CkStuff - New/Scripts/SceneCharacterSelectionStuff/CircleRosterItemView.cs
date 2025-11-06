using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CircleRosterItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image portrait;           // child: Portrait
    [SerializeField] private CanvasGroup selectedGlow; // child: SelectedGlow
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private Vector3 selectedOffset = new Vector3(0f, 15f, 0f);
    [SerializeField] private float moveDuration = 0.15f;

    private Vector3 originalPos;
    private Coroutine moveRoutine;

    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private NewCharacterDefinition currentDef;
    private bool isSelected = false;
    private bool isHovered = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPos = rectTransform.localPosition;

        Debug.Log(rectTransform.localPosition);
    }


    public void Bind(NewCharacterDefinition def, System.Action onClick)
    {
        currentDef = def;
        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());

        // Default to portrait1
        if (portrait) portrait.sprite = def.portrait ? def.portrait : def.normalArt;
        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        isSelected = on;
        if (selectedGlow) selectedGlow.alpha = on ? 1f : 0f;

        // Move position based on selection state
        Vector3 targetPos = on ? originalPos + selectedOffset : originalPos;
        StartMoveTween(targetPos, moveDuration);

        // Update portrait
        if (portrait && currentDef != null)
        {
            portrait.sprite = on
                ? (currentDef.portrait2 ? currentDef.portrait2 : currentDef.portrait)
                : (currentDef.portrait ? currentDef.portrait : currentDef.normalArt);
        }
    }

    private void StartMoveTween(Vector3 target, float duration)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveTween(target, duration));
    }

    private IEnumerator MoveTween(Vector3 target, float duration)
    {
        Vector3 start = transform.localPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }
        transform.localPosition = target;
    }


    // Hover effects
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StartScaleTween(originalScale * 1.1f, 0.1f); // expand slightly on hover
        UpdatePortraitHoverState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StartScaleTween(originalScale, 0.1f); // back to normal
        UpdatePortraitHoverState();
    }

    // Click effect
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(ClickExpandEffect());
    }

    private IEnumerator ClickExpandEffect()
    {
        StartScaleTween(originalScale * 1.25f, 0.08f); // expand a bit more
        yield return new WaitForSeconds(0.1f);
        StartScaleTween(originalScale * 1.1f, 0.08f); // return to hover size
    }

    private void StartScaleTween(Vector3 targetScale, float duration)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTween(targetScale, duration));
    }

    private IEnumerator ScaleTween(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    private void UpdatePortraitHoverState()
    {
        if (!portrait || currentDef == null) return;

        // If selected, always show portrait2
        if (isSelected)
        {
            portrait.sprite = currentDef.portrait2 ? currentDef.portrait2 : currentDef.portrait;
        }
        else if (isHovered)
        {
            // Show portrait2 when hovering
            portrait.sprite = currentDef.portrait2 ? currentDef.portrait2 : currentDef.portrait;
        }
        else
        {
            // Otherwise revert to normal
            portrait.sprite = currentDef.portrait ? currentDef.portrait : currentDef.normalArt;
        }
    }
}
