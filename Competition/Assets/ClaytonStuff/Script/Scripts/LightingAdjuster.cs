using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class LightingAdjuster : MonoBehaviour
{
    [Header("Light Reference")]
    public Light2D globalLight;   // Assign your Global Light 2D here

    [Header("UI Slider")]
    public Slider intensitySlider;

    [Header("Intensity Range")]
    public float minIntensity = 0.3f;
    public float maxIntensity = 2.5f;

    private void Start()
    {
        if (globalLight == null)
        {
            Debug.LogWarning("No Light2D assigned!");
            return;
        }

        if (intensitySlider != null)
        {
            // Set slider range
            intensitySlider.minValue = minIntensity;
            intensitySlider.maxValue = maxIntensity;

            // Set initial value
            intensitySlider.value = globalLight.intensity;

            // Add event listener
            intensitySlider.onValueChanged.AddListener(UpdateLightIntensity);
        }
    }

    private void UpdateLightIntensity(float value)
    {
        if (globalLight != null)
            globalLight.intensity = value;
    }
}
