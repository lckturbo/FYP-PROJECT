using TMPro;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
    [Header("Hint Settings")]
    [SerializeField] private Canvas hintCanvas;
    [SerializeField] private TMP_Text hintText;

    [SerializeField] private float fadeSpeed;
    private bool hasTriggered = false;
    private Coroutine fadeRoutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (hintCanvas)
        {
            canvasGroup = hintCanvas.GetComponent<CanvasGroup>();
            if (!canvasGroup)
                canvasGroup = hintCanvas.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hintCanvas || !collision.CompareTag("Player")) return;
        if (hasTriggered) return;

        Debug.Log("ENTER tutorial zone: " + gameObject.name);
        StartFade(1f);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!hintCanvas || !collision.CompareTag("Player")) return;
        StartCoroutine(CheckFadeOut());
    }


    private void StartFade(float target)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCanvas(target));
    }

    private System.Collections.IEnumerator CheckFadeOut()
    {
        yield return new WaitForSeconds(5.0f);
        StartFade(0f);

    }
    private System.Collections.IEnumerator FadeCanvas(float target)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, target))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
