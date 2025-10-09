using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemGlow : MonoBehaviour
{
    [Header("References")]
    public Item item;  // Reference to the item ScriptableObject

    [Header("Glow Settings")]
    public float glowIntensity = 1.5f;
    public float pulseSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private Material glowMaterial;
    private Color baseColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Use the sprite's default material as a base
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        spriteRenderer.material = glowMaterial;

        baseColor = spriteRenderer.color;
        ApplyGlowColor();
    }

    private void Update()
    {
        // Optional: pulsating effect for glow
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * 0.3f;
        spriteRenderer.material.SetColor("_Color", baseColor * (1 + pulse * glowIntensity));
    }

    private void ApplyGlowColor()
    {
        if (item == null)
        {
            Debug.LogWarning($"[{name}] No Item assigned for glow!");
            return;
        }

        // Pick color based on rarity
        Color glowColor = item.type switch
        {
            ItemTypes.Common => new Color(0f, 1f, 0f),      // Green
            ItemTypes.Uncommon => new Color(1f, 1f, 0f),    // Yellow
            ItemTypes.Rare => new Color(0f, 0.5f, 1f),      // Blue
            ItemTypes.Epic => new Color(0.7f, 0f, 1f),      // Purple
            ItemTypes.Leagendary => new Color(1f, 0.84f, 0f), // Gold
            _ => Color.white
        };

        baseColor = glowColor;
        spriteRenderer.material.SetColor("_Color", baseColor);
    }
}
