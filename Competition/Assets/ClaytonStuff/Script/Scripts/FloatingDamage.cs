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

    public void Initialize(int damageAmount, bool isCrit = false, NewElementType elementType = NewElementType.None)
    {
        if (!damageText) damageText = GetComponentInChildren<TextMeshProUGUI>();

        string elementName = "";
        switch (elementType)
        {
            case NewElementType.Fire:
                elementName = "Fire";
                originalColor = new Color(1f, 0.35f, 0.25f); // red
                break;
            case NewElementType.Water:
                elementName = "Water";
                originalColor = new Color(0.3f, 0.6f, 1f); // blue
                break;
            case NewElementType.Grass:
                elementName = "Grass";
                originalColor = new Color(0.35f, 1f, 0.35f); // green
                break;
            case NewElementType.Light:
                elementName = "Light";
                originalColor = new Color(1f, 0.95f, 0.5f); // yellow
                break;
            case NewElementType.Dark:
                elementName = "Dark";
                originalColor = new Color(0.7f, 0.3f, 1f); // purple
                break;
            default:
                elementName = "";
                originalColor = Color.white;
                break;
        }

        // Text display rules
        if (isCrit)
        {
            damageText.text = $"{damageAmount} CRIT!";
            originalColor = new Color(1f, 0.9f, 0.1f); // gold for crits
        }
        else
        {
            // Show element name if it exists
            damageText.text = string.IsNullOrEmpty(elementName)
                ? $"{damageAmount}"
                : $"{damageAmount} {elementName}";
        }

        damageText.color = originalColor;

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
