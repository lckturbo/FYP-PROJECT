using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Pop Effect")]
    [SerializeField] private float popScale = 1.4f;
    [SerializeField] private float popDuration = 0.15f;

    private Color originalColor;
    private float timer;
    private Vector3 originalScale;
    private bool popped;

    public void Initialize(int damageAmount, bool isCrit = false)
    {
        if (!damageText) damageText = GetComponentInChildren<TextMeshProUGUI>();

        // Add "Crit" marker if critical hit
        if (isCrit)
        {
            damageText.text = $"{damageAmount} Crit!";
            damageText.color = new Color(1f, 0.9f, 0.1f); // gold-yellow for crit
        }
        else
        {
            damageText.text = damageAmount.ToString();
            damageText.color = Color.white;
        }

        originalColor = damageText.color;
        transform.localPosition += offset;
        originalScale = transform.localScale;
        timer = 0f;
        popped = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Pop expand effect
        if (!popped)
        {
            float t = Mathf.Clamp01(timer / popDuration);
            transform.localScale = Vector3.Lerp(originalScale * popScale, originalScale, t);
            if (t >= 1f) popped = true;
        }

        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        float fadeT = timer / fadeDuration;
        damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - fadeT);

        if (fadeT >= 1f)
            Destroy(gameObject);
    }
}
