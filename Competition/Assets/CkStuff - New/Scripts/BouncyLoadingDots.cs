using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouncyLoadingDots : MonoBehaviour
{
    [Header("Dot Prefab")]
    [SerializeField] private RectTransform dotPrefab;

    [Header("Layout")]
    [SerializeField] private int count = 3;
    [SerializeField] private float spacing = 48f;

    [Header("Bounce")]
    [SerializeField] private float height = 28f;
    [SerializeField] private float baseDuration = 0.55f;
    [SerializeField] private float phaseOffset = 0.20f;
    [SerializeField] private bool easeWithProgress = false;

    [Header("Stop Logic")]
    [SerializeField] private bool stopWhenLoaded = true;

    [Header("Squash/Stretch")]
    [SerializeField] private float squashAmount = 0.33f;
    [SerializeField] private float minScaleY = 0.6f;

    private readonly List<RectTransform> _dots = new();
    private readonly List<Image> _shadows = new();
    private bool _playing;

    void OnEnable()
    {
        Build();
        _playing = true;
        StartCoroutine(Animate());
    }

    void OnDisable()
    {
        _playing = false;
        StopAllCoroutines();
    }

    private void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        _dots.Clear();
        _shadows.Clear();

        if (!dotPrefab) { Debug.LogWarning("BouncyLoadingDots: dotPrefab not set."); return; }

        for (int i = 0; i < count; i++)
        {
            var dot = Instantiate(dotPrefab, transform);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2((i - (count - 1) * 0.5f) * spacing, 0f);
            rt.localScale = Vector3.one;
            _dots.Add(rt);
        }
    }

    private IEnumerator Animate()
    {
        float t = 0f;

        while (_playing)
        {
            if (stopWhenLoaded && typeof(ASyncManager).IsClass)
            {
                if (ASyncManager.instance && !ASyncManager.IsLoading)
                    yield break;
            }

            t += Time.unscaledDeltaTime;

            float dur = baseDuration;
            if (easeWithProgress && ASyncManager.instance)
            {
                float prog = ASyncManager.DisplayedProgress;
                dur = Mathf.Lerp(baseDuration * 0.75f, baseDuration * 1.25f, prog);
            }

            for (int i = 0; i < _dots.Count; i++)
            {
                float phase = Mathf.Repeat((t - i * phaseOffset) / dur, 1f);

                float y01 = Mathf.Sin(Mathf.PI * phase);
                float y = y01 * height;

                float contact = 1f - y01;
                float sx = 1f + squashAmount * contact;
                float sy = Mathf.Max(1f - squashAmount * contact, minScaleY);

                var rt = _dots[i];
                var basePos = new Vector2(rt.anchoredPosition.x, 0f);
                rt.anchoredPosition = basePos + Vector2.up * y;
                rt.localScale = new Vector3(sx, sy, 1f);
            }

            yield return null;
        }
    }
}
