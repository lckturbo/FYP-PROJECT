using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLight : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private Color lightColor = Color.white;
    private Light2D light2D;

    void Start()
    {
        light2D = gameObject.AddComponent<Light2D>();
        light2D.lightType = Light2D.LightType.Point;
        light2D.pointLightOuterRadius = radius;
        light2D.intensity = 1.2f;
        light2D.color = lightColor;
        light2D.shadowsEnabled = true;
    }
}
