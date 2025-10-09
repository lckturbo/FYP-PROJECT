using UnityEngine;
using TMPro;

public class RainbowText : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 1f;
    public bool useUnscaledTime = false;

    private TMP_Text tmpText;
    private float hue = 0f;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        float delta = (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * speed;
        hue += delta;
        if (hue > 1f) hue -= 1f;

        Color rainbow = Color.HSVToRGB(hue, 1f, 1f);

        tmpText.color = rainbow;
    }
}
