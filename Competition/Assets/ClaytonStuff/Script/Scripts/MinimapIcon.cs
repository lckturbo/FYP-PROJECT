using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MinimapIcon : MonoBehaviour
{
    [Header("Minimap Icon Settings")]
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -1f);
    [SerializeField] private float iconScale = 10f;
    [SerializeField] private int normalOrder = 10;
    [SerializeField] private int leaderOrder = 20;

    private GameObject iconObject;

    void Start()
    {
        // === Disable entirely if in JasBattle scene ===
        if (SceneManager.GetActiveScene().name == "jasBattle")
        {
            enabled = false;
            return;
        }

        iconObject = new GameObject($"{name}_MinimapIcon");
        iconObject.transform.SetParent(null);
        iconObject.layer = 29;

        var sr = iconObject.AddComponent<SpriteRenderer>();
        sr.sprite = iconSprite;
        sr.sortingLayerName = "MiniMap";
        sr.sortingOrder = normalOrder;

        // --- Identify leader and elevate sorting ---
        var playerParty = PlayerParty.instance;
        if (playerParty != null)
        {
            var leader = playerParty.GetLeader();
            if (leader != null && name.Contains(leader.displayName))
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
