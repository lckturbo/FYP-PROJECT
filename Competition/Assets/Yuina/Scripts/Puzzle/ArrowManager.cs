using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArrowManager : MonoBehaviour
{
    [SerializeField] private RectTransform arrowParent; // Parent object within the Canvas
    [SerializeField] private GameObject arrowPrefab;    // UI Arrow Prefab
    [SerializeField] private float offsetX = 50f;       // Spacing between items (in px, since it's UI)

    private List<GameObject> arrows = new();

    public void AddArrow(Vector2Int dir)
    {
        // Arrow UI Generation
        GameObject arrow = Instantiate(arrowPrefab, arrowParent);

        // Alignment: Horizontal shift
        RectTransform rt = arrow.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(offsetX * arrows.Count, 0);

        // Rotation
        float angle = 0f;
        if (dir == Vector2Int.right) angle = -90f;
        if (dir == Vector2Int.left) angle = 90f;
        if (dir == Vector2Int.down) angle = 180f;

        rt.localRotation = Quaternion.Euler(0, 0, angle);

        // Save to list
        arrows.Add(arrow);
    }

    // For resetting the arrow
    public void ClearArrows()
    {
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows.Clear();
    }
}
