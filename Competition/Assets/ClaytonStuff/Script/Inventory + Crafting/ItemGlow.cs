using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemGlowAura : MonoBehaviour
{
    [Header("References")]
    public Item item; // The Item ScriptableObject this glow is based on

    [Header("Glow Settings")]
    public float glowSize = 1.1f;          // How large the glow appears relative to the object
    [Range(0f, 1f)] public float baseAlpha = 0.35f;  // Base transparency
    [Range(0f, 1f)] public float pulseAlpha = 0.25f; // How much alpha oscillates
    public float pulseSpeed = 2f;          // How fast it pulses

    private SpriteRenderer mainRenderer;
    private GameObject glowObject;
    private SpriteRenderer glowRenderer;
    private Color glowColor;

    private void Awake()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        CreateGlowObject();
    }

    private void Update()
    {
        if (glowRenderer == null) return;

        // Pulse transparency
        float alphaPulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        float currentAlpha = baseAlpha + alphaPulse * pulseAlpha;

        Color c = glowColor;
        c.a = currentAlpha;
        glowRenderer.color = c;

        // Keep position and rotation synced
        glowObject.transform.position = transform.position;
        glowObject.transform.rotation = transform.rotation;
    }

    private void CreateGlowObject()
    {
        if (item == null)
        {
            Debug.LogWarning($"{name}: No Item assigned, defaulting white glow.");
        }

        // Create a child for glow
        glowObject = new GameObject("GlowAura");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localScale = Vector3.one * glowSize;

        glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = mainRenderer.sprite;
        glowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        glowRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        glowRenderer.sortingOrder = mainRenderer.sortingOrder - 1; // behind main sprite

        glowColor = item != null ? GetColorByRarity(item.type) : new Color(1f, 1f, 1f, baseAlpha);
        glowRenderer.color = glowColor;
    }

    private Color GetColorByRarity(ItemTypes type)
    {
        return type switch
        {
            ItemTypes.Common => new Color(0f, 1f, 0f, baseAlpha),        // Green
            ItemTypes.Uncommon => new Color(1f, 1f, 0f, baseAlpha),      // Yellow
            ItemTypes.Rare => new Color(0f, 0.5f, 1f, baseAlpha),        // Blue
            ItemTypes.Epic => new Color(0.7f, 0f, 1f, baseAlpha),        // Purple
            ItemTypes.Leagendary => new Color(1f, 0.84f, 0f, baseAlpha), // Gold
            _ => new Color(1f, 1f, 1f, baseAlpha)
        };
    }

    private void OnValidate()
    {
        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();
    }
}
