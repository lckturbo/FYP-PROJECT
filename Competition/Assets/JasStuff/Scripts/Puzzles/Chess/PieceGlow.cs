using UnityEngine;

public class PieceGlow : MonoBehaviour
{
    public bool isWhite;

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
        if (other.CompareTag("Player") && isWhite)
            spriteRenderer.material = highlightMaterial;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isWhite)
            spriteRenderer.material = originalMat;
    }
}
