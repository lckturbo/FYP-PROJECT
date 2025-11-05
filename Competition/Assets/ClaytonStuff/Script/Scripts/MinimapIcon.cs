using UnityEngine;

[DisallowMultipleComponent]
public class MinimapIcon : MonoBehaviour
{
    [Header("Minimap Icon Settings")]
    [SerializeField] private Sprite iconSprite; // the unique minimap icon
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -1f); // keep above player
    [SerializeField] private float iconScale = 10f;

    private GameObject iconObject;

    void Start()
    {
        // Create icon object in minimap layer
        iconObject = new GameObject($"{name}_MinimapIcon");
        iconObject.transform.SetParent(null);
        iconObject.layer = 29;

        var sr = iconObject.AddComponent<SpriteRenderer>();
        sr.sprite = iconSprite;
        sr.sortingLayerName = "MiniMap"; // optional layer just for minimap camera
        sr.sortingOrder = 10;

        iconObject.transform.localScale = Vector3.one * iconScale;
    }

    void LateUpdate()
    {
        if (iconObject == null) return;
        iconObject.transform.position = transform.position + offset;
        iconObject.transform.rotation = Quaternion.identity; // no rotation
    }

    private void OnDestroy()
    {
        if (iconObject != null)
            Destroy(iconObject);
    }
}
