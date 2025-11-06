using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class FoggedTrigger : MonoBehaviour, IDataPersistence
{
    [Header("Identity")]
    [SerializeField] private string fogId = "fog_area_001";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Renderers (auto if empty)")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TilemapRenderer[] tilemapRenderers;

    private Collider2D col;
    private bool cleared;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        if (tilemapRenderers == null || tilemapRenderers.Length == 0)
            tilemapRenderers = GetComponentsInChildren<TilemapRenderer>(includeInactive: true);

        foreach (var r in spriteRenderers)
            if (r != null) r.sortingLayerName = "Fog";

        foreach (var r in tilemapRenderers)
            if (r != null) r.sortingLayerName = "Fog";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!cleared && other.CompareTag("Player"))
            StartCoroutine(FadeAndClear());
    }

    private IEnumerator FadeAndClear()
    {
        cleared = true;
        if (col) col.enabled = false;

        float t = 0f;

        // Cache colors for sprites
        var spriteColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            if (spriteRenderers[i]) spriteColors[i] = spriteRenderers[i].color;

        // Cache materials for tilemaps (they use material color)
        var tilemapColors = new Color[tilemapRenderers.Length];
        for (int i = 0; i < tilemapRenderers.Length; i++)
            if (tilemapRenderers[i]) tilemapColors[i] = tilemapRenderers[i].material.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(t / fadeDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
                if (spriteRenderers[i])
                {
                    var c = spriteColors[i];
                    c.a = alpha;
                    spriteRenderers[i].color = c;
                }

            for (int i = 0; i < tilemapRenderers.Length; i++)
                if (tilemapRenderers[i])
                {
                    var c = tilemapColors[i];
                    c.a = alpha;
                    tilemapRenderers[i].material.color = c;
                }

            yield return null;
        }

        // Disable after fading
        foreach (var r in spriteRenderers)
            if (r) r.enabled = false;
        foreach (var r in tilemapRenderers)
            if (r) r.enabled = false;
    }

    public void LoadData(GameData data)
    {
        if (data.clearedFogIds != null && data.clearedFogIds.Contains(fogId))
        {
            cleared = true;
            if (col) col.enabled = false;

            foreach (var r in spriteRenderers)
                if (r) { var c = r.color; c.a = 0f; r.color = c; r.enabled = false; }

            foreach (var r in tilemapRenderers)
                if (r) { var c = r.material.color; c.a = 0f; r.material.color = c; r.enabled = false; }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (cleared)
        {
            if (data.clearedFogIds == null) data.clearedFogIds = new System.Collections.Generic.List<string>();
            if (!data.clearedFogIds.Contains(fogId))
                data.clearedFogIds.Add(fogId);
        }
    }
}
