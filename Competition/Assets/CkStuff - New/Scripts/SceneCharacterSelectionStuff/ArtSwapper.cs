using UnityEngine;

public class ArtSwapper : MonoBehaviour
{
    [SerializeField] private CanvasGroup normalCG; // drag ArtNormal CanvasGroup
    [SerializeField] private CanvasGroup pixelCG;  // drag ArtPixel  CanvasGroup
    [SerializeField] private float duration = 0.15f;
    private bool showingPixel = false;

    public void Toggle()
    {
        showingPixel = !showingPixel;
        StopAllCoroutines();
        StartCoroutine(Fade(showingPixel ? normalCG : pixelCG, 0f));
        StartCoroutine(Fade(showingPixel ? pixelCG : normalCG, 1f));
    }

    System.Collections.IEnumerator Fade(CanvasGroup cg, float to)
    {
        float from = cg.alpha, t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }
}
