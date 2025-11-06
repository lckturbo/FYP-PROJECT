using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoggedTrigger : MonoBehaviour, IDataPersistence
{
    [Header("Identity")]
    [SerializeField] private string fogId = "fog_area_001";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Renderers (auto if empty)")]
    [SerializeField] private SpriteRenderer[] renderers; // add your fog sprites here; auto-fills if empty

    private Collider2D col;
    private bool cleared;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        // Make sure these render only on minimap. Choose one:
        // 1) Unity layer for minimap camera culling:
        // gameObject.layer = LayerMask.NameToLayer("Fog");
        // 2) Sorting layer used by minimap camera:
        foreach (var r in renderers)
        {
            if (r != null) r.sortingLayerName = "Fog";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only clear when the player main body enters
        if (!cleared && other.CompareTag("Player"))
            StartCoroutine(FadeAndClear());
    }

    private IEnumerator FadeAndClear()
    {
        cleared = true;
        if (col) col.enabled = false;

        float t = 0f;
        // cache original colors
        var colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i]) colors[i] = renderers[i].color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeDuration);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i]) { var c = colors[i]; c.a = c.a * a; renderers[i].color = c; }
            yield return null;
        }

        // fully cleared
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i]) renderers[i].enabled = false;

        // optional: destroy to free up
        // Destroy(gameObject);
    }

    // ========= Save/Load =========
    public void LoadData(GameData data)
    {
        // If you haven’t added fog persistence to GameData yet, see section 2 below.
        if (data.clearedFogIds != null && data.clearedFogIds.Contains(fogId))
        {
            cleared = true;
            if (col) col.enabled = false;
            foreach (var r in renderers) if (r) { var c = r.color; c.a = 0f; r.color = c; r.enabled = false; }
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
