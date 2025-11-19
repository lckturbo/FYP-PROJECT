using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightActivator : MonoBehaviour
{
    [SerializeField] private Light2D light2D;  // auto-filled
    [SerializeField] private float fadeSpeed = 3f;

    private bool targetState = false;
    private float maxIntensity;

    private void Awake()
    {
        // Auto-assign Light2D if missing
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
            if (light2D == null)
                light2D = GetComponentInChildren<Light2D>();

            if (light2D == null)
                Debug.LogWarning($"[LightActivator] No Light2D found on {name} or its children.");
        }

        if (light2D != null)
        {
            maxIntensity = light2D.intensity;
            light2D.intensity = 0f; // turn off at start
        }
    }

    private void Update()
    {
        if (light2D == null) return;

        float target = targetState ? maxIntensity : 0f;
        light2D.intensity = Mathf.MoveTowards(light2D.intensity, target, fadeSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            targetState = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            targetState = false;
    }
}
