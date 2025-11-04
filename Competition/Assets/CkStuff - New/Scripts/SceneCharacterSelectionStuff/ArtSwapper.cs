using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArtSwapper : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup normalCG; // ArtNormal CanvasGroup
    [SerializeField] private CanvasGroup pixelCG;  // ArtPixel CanvasGroup
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("Glitch Settings")]
    [SerializeField] private int glitchFrames = 3;         // Number of quick glitch flashes
    [SerializeField] private float glitchDuration = 0.05f; // Duration of each flash
    [SerializeField] private float glitchOffset = 5f;      // Pixel offset for shake

    [Header("Art Images")]
    [SerializeField] private Image artNormal;
    [SerializeField] private Image artPixel;

    [Header("Auto Swap")]
    [SerializeField] private bool autoSwap = true;         // Enable automatic swap
    [SerializeField] private float swapInterval = 2f;     // Time between auto swaps

    private bool showingPixel = false;

    private void Start()
    {
        if (autoSwap)
            StartCoroutine(AutoSwapLoop());
    }

    public void Toggle()
    {
        showingPixel = !showingPixel;
        StopAllCoroutines();
        StartCoroutine(SwapWithGlitch());

        // Restart auto-swap if enabled
        if (autoSwap)
            StartCoroutine(AutoSwapLoop());
    }

    private IEnumerator AutoSwapLoop()
    {
        while (autoSwap)
        {
            yield return new WaitForSecondsRealtime(swapInterval);
            Toggle();
        }
    }

    private IEnumerator SwapWithGlitch()
    {
        CanvasGroup fromCG = showingPixel ? normalCG : pixelCG;
        CanvasGroup toCG = showingPixel ? pixelCG : normalCG;

        Vector3 normalPos = artNormal.rectTransform.localPosition;
        Vector3 pixelPos = artPixel.rectTransform.localPosition;

        // Quick glitch flickers with position offsets and alpha
        for (int i = 0; i < glitchFrames; i++)
        {
            fromCG.alpha = Random.value;
            toCG.alpha = Random.value;

            if (artNormal && artPixel)
            {
                artNormal.rectTransform.localPosition = normalPos + new Vector3(Random.Range(-glitchOffset, glitchOffset), Random.Range(-glitchOffset, glitchOffset), 0);
                artPixel.rectTransform.localPosition = pixelPos + new Vector3(Random.Range(-glitchOffset, glitchOffset), Random.Range(-glitchOffset, glitchOffset), 0);

                artNormal.color = new Color(1, 1, 1, Random.Range(0.5f, 1f));
                artPixel.color = new Color(1, 1, 1, Random.Range(0.5f, 1f));
            }

            yield return new WaitForSecondsRealtime(glitchDuration);
        }

        // Reset positions and colors before smooth fade
        if (artNormal && artPixel)
        {
            artNormal.rectTransform.localPosition = normalPos;
            artPixel.rectTransform.localPosition = pixelPos;
            artNormal.color = Color.white;
            artPixel.color = Color.white;
        }

        // Smooth fade between canvas groups
        StartCoroutine(Fade(fromCG, 0f));
        yield return Fade(toCG, 1f);
    }

    private IEnumerator Fade(CanvasGroup cg, float to)
    {
        float from = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }
}
