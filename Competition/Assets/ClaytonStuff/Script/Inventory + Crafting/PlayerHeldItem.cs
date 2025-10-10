using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Attachment Point")]
    [SerializeField] public Transform handPoint;

    [Header("Starting Equipment")]
    [SerializeField] private Item startingItem;

    [Header("Visual Settings")]
    [Tooltip("Base scale for the held item.")]
    [SerializeField] private Vector2 baseItemScale = new Vector2(1f, 1f);

    [Header("Direction Offsets")]
    [SerializeField] private Vector3 rightOffset = new Vector3(0.3f, 0f, 0f);
    [SerializeField] private Vector3 leftOffset = new Vector3(-0.3f, 0f, 0f);
    [SerializeField] private Vector3 upOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField] private Vector3 downOffset = new Vector3(0f, -0.3f, 0f);

    [Header("Rotation Angles")]
    [SerializeField] private float upAngle = 90f;
    [SerializeField] private float downAngle = -90f;
    [SerializeField] private float rightAngle = 0f;
    [SerializeField] private float leftAngle = 0f;

    private GameObject currentHeld;
    private Item equippedItem;
    private Animator animator;
    private bool facingRight = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        var combatant = GetComponent<Combatant>();
        if (combatant != null && !combatant.isLeader)
        {
            if (handPoint != null)
                Destroy(handPoint.gameObject);

            Debug.Log($"{name} is not the leader — disabling handPoint and skipping item spawn.");
            return;
        }

        if (handPoint != null)
            handPoint.gameObject.SetActive(true);

        if (startingItem == null)
            return;

        if (BattleManager.instance != null && BattleManager.instance.inBattle)
        {
            Debug.Log("In battle — skipping starting weapon spawn.");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            if (!InventoryManager.Instance.HasItem(startingItem))
            {
                InventoryManager.Instance.AddItemToInventory(startingItem, 1);
                DisplayItem(startingItem);
                Debug.Log($"Spawned and added starting item: {startingItem.itemName}");
            }
        }
        else
        {
            Debug.LogWarning("No InventoryManager found when trying to add starting item!");
        }
    }

    private void Update()
    {
        if (currentHeld == null || animator == null) return;

        float moveX = animator.GetFloat("moveX");
        float moveY = animator.GetFloat("moveY");

        // Determine direction — same as PlayerAttack
        Vector3 offset = Vector3.zero;
        float angle = 0f;

        if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
        {
            if (moveX > 0)
            {
                // Right
                offset = rightOffset;
                angle = rightAngle;
                facingRight = true;
            }
            else
            {
                // Left
                offset = leftOffset;
                angle = leftAngle;
                facingRight = false;
            }
        }
        else if (Mathf.Abs(moveY) > 0.01f)
        {
            if (moveY > 0)
            {
                // Up
                offset = upOffset;
                angle = upAngle;
            }
            else
            {
                // Down
                offset = downOffset;
                angle = downAngle;
            }
        }

        // Apply local position and rotation
        currentHeld.transform.localPosition = offset;
        currentHeld.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Flip horizontally for left/right
        Vector3 scale = baseItemScale;
        scale.x *= facingRight ? 1f : -1f;
        currentHeld.transform.localScale = scale;
    }

    public void DisplayItem(Item item)
    {
        if (item == null || item.worldPrefab == null)
        {
            HideItem();
            return;
        }

        if (currentHeld != null)
        {
            Destroy(currentHeld);
            currentHeld = null;
        }

        equippedItem = item;
        currentHeld = Instantiate(item.worldPrefab, handPoint.position, Quaternion.identity, handPoint);
        currentHeld.transform.localPosition = Vector3.zero;
        currentHeld.transform.localRotation = Quaternion.identity;
        currentHeld.transform.localScale = baseItemScale;
    }

    public void RemoveItem()
    {
        if (currentHeld != null)
            Destroy(currentHeld);

        currentHeld = null;
        equippedItem = null;
    }

    public void HideItem()
    {
        if (currentHeld != null)
            currentHeld.SetActive(false);

        equippedItem = null;
    }

    public Item GetEquippedItem() => equippedItem;
}
