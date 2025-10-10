using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Color originalColor;
    private float timer;

    public void Initialize(int damageAmount, bool isCrit = false)
    {
        if (!damageText) damageText = GetComponentInChildren<TextMeshProUGUI>();

        damageText.text = damageAmount.ToString();
        originalColor = isCrit ? Color.yellow : Color.white;
        damageText.color = originalColor;

        transform.localPosition += offset;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        float t = timer / fadeDuration;
        damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);

        if (t >= 1f)
            Destroy(gameObject);
    }
}
