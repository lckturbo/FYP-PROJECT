using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    private Light2D light2D;
    private float baseIntensity;

    void Awake()
    {
        light2D = GetComponent<Light2D>();
        baseIntensity = light2D.intensity;
    }

    void Update()
    {
        light2D.intensity = baseIntensity + Mathf.Sin(Time.time * 20f) * 0.1f;
    }
}
