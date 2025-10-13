using System.Collections;
using UnityEngine;

public class LobbyZoomController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform viewport;    // this object
    [SerializeField] private RectTransform content;     // the big image (CENTER-anchored)

    [Header("Zoom")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomDuration = 0.6f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _anim;
    private Vector2 _homePos;
    private float _homeZoom;

    void Awake()
    {
        if (!viewport) viewport = (RectTransform)transform;

        // Ensure centered anchors (safety)
        content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);

        // Fit the content to the viewport initially (letterbox within)
        _homeZoom = FitToViewport();
        content.localScale = Vector3.one * _homeZoom;

        _homePos = Vector2.zero;
        content.anchoredPosition = _homePos;
    }

    public void FocusOn(RectTransform region, float zoomFactor = 2f)
    {
        zoomFactor = Mathf.Clamp(zoomFactor, minZoom, maxZoom);

        // Directly use the hotspot’s local position (its pivot center)
        Vector2 regionPos = region.localPosition;

        // Move content so that the hotspot is centered in the viewport
        Vector2 targetPos = -regionPos * zoomFactor + (viewport.rect.size * 0.5f);

        // Clamp to avoid showing outside edges
        targetPos = ClampToBounds(targetPos, zoomFactor);

        StartTween(targetPos, zoomFactor);
    }

    public void ResetView() => StartTween(_homePos, _homeZoom);

    private void StartTween(Vector2 targetPos, float targetZoom)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateTo(targetPos, targetZoom));
    }

    private IEnumerator AnimateTo(Vector2 targetPos, float targetZoom)
    {
        Vector2 startPos = content.anchoredPosition;
        float startZoom = content.localScale.x;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / zoomDuration;
            float k = ease.Evaluate(Mathf.Clamp01(t));

            float z = Mathf.Lerp(startZoom, targetZoom, k);
            content.localScale = Vector3.one * z;

            Vector2 p = Vector2.Lerp(startPos, targetPos, k);
            content.anchoredPosition = ClampToBounds(p, z);
            yield return null;
        }
        _anim = null;
    }

    private Vector2 ClampToBounds(Vector2 desiredPos, float zoom)
    {
        Vector2 view = viewport.rect.size;
        Vector2 scaled = content.rect.size * zoom;

        float minX = Mathf.Min(0f, view.x - scaled.x);
        float maxX = Mathf.Max(0f, view.x - scaled.x);
        float minY = Mathf.Min(0f, view.y - scaled.y);
        float maxY = Mathf.Max(0f, view.y - scaled.y);

        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        return desiredPos;
    }

    // Compute a scale that fits the whole image inside the viewport
    private float FitToViewport()
    {
        Vector2 view = viewport.rect.size;
        Vector2 img = content.rect.size; // requires non-stretch rect (Set Native Size)
        if (img.x <= 0f || img.y <= 0f) return 1f;

        float fit = Mathf.Min(view.x / img.x, view.y / img.y);
        return Mathf.Max(minZoom, Mathf.Min(fit, maxZoom));
    }
}
