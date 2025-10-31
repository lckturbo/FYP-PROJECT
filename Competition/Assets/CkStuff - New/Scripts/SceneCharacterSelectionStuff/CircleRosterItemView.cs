using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CircleRosterItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image portrait;           // child: Portrait
    [SerializeField] private CanvasGroup selectedGlow; // child: SelectedGlow
    [SerializeField] private Button button;

    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

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

    // Hover effects
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaleTween(originalScale * 1.1f, 0.1f); // expand slightly on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleTween(originalScale, 0.1f); // back to normal
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
}
