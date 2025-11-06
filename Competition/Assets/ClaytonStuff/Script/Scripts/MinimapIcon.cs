using UnityEngine;

[DisallowMultipleComponent]
public class MinimapIcon : MonoBehaviour
{
    [Header("Minimap Icon Settings")]
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -1f);
    [SerializeField] private float iconScale = 10f;
    [SerializeField] private int normalOrder = 10;
    [SerializeField] private int leaderOrder = 20; // higher = on top

    private GameObject iconObject;

    void Start()
    {
        iconObject = new GameObject($"{name}_MinimapIcon");
        iconObject.transform.SetParent(null);
        iconObject.layer = 29;

        var sr = iconObject.AddComponent<SpriteRenderer>();
        sr.sprite = iconSprite;
        sr.sortingLayerName = "MiniMap";
        sr.sortingOrder = normalOrder;

        // === Check if this belongs to the current leader ===
        var playerParty = PlayerParty.instance;
        if (playerParty != null)
        {
            var leader = playerParty.GetLeader();
            var held = GetComponentInChildren<PlayerHeldItem>();
            if (held != null && leader != null && held.name.Contains(leader.displayName))
            {
                sr.sortingOrder = leaderOrder;
                Debug.Log($"[MinimapIcon] {name} identified as leader, elevated sorting order.");
            }
        }

        iconObject.transform.localScale = Vector3.one * iconScale;
    }

    void LateUpdate()
    {
        if (iconObject == null) return;
        iconObject.transform.position = transform.position + offset;
        iconObject.transform.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (iconObject != null)
            Destroy(iconObject);
    }
}
