using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightActivator : MonoBehaviour
{
    [SerializeField] private GameObject lightObject; // object holding Light2D

    private void Awake()
    {
        // Auto-assign the Light2D child if missing
        if (lightObject == null)
        {
            Light2D foundLight = GetComponentInChildren<Light2D>(true);
            if (foundLight != null)
            {
                lightObject = foundLight.gameObject;
            }
            else
            {
                Debug.LogWarning($"[LightActivator] No Light2D child found on {name}");
            }
        }

        // Start OFF
        if (lightObject != null)
            lightObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (lightObject == null) return;
        if (other.CompareTag("Player"))
            lightObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (lightObject == null) return;
        if (other.CompareTag("Player"))
            lightObject.SetActive(false);
    }
}
