using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrioLoadingIcons : MonoBehaviour
{
    [Header("Character Prefabs (assign 3)")]
    [SerializeField] private RectTransform[] characterPrefabs;

    [Header("Layout")]
    [SerializeField] private float spacing = 34f;

    [Header("Stop Logic")]
    [SerializeField] private bool hideWhenLoaded = true;

    private readonly List<RectTransform> _spawned = new();

    private void OnEnable()
    {
        Build();

        if (hideWhenLoaded)
            StartCoroutine(WatchLoading());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Build()
    {
        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        _spawned.Clear();

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogWarning("TrioLoadingIcons: No characterPrefabs assigned.");
            return;
        }

        int count = characterPrefabs.Length;

        for (int i = 0; i < count; i++)
        {
            RectTransform prefab = characterPrefabs[i];
            if (!prefab) continue;

            RectTransform rt = Instantiate(prefab, transform);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            // Center them horizontally, spaced out
            float x = (i - (count - 1) * 0.5f) * spacing;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.localScale = Vector3.one;

            _spawned.Add(rt);
        }
    }

    private IEnumerator WatchLoading()
    {
        // Same style of stop logic as your old BouncyLoadingDots
        while (ASyncManager.instance && ASyncManager.IsLoading)
        {
            yield return null;
        }

        // Loading done -> hide this object (and therefore the trio)
        gameObject.SetActive(false);
    }
}
