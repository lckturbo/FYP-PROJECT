// Example script: makes chest glow when player is near
using UnityEngine;

public class ChestGlow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material highlightMaterial;
    private Material originalMat;

    void Start()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        originalMat = spriteRenderer.material;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            spriteRenderer.material = highlightMaterial;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            spriteRenderer.material = originalMat;
    }
}
