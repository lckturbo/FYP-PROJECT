using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Attachment Point")]
    [SerializeField] private Transform handPoint;

    [Header("Starting Equipment")]
    [SerializeField] private Item startingItem; // assign your Sword item here in Inspector

    private GameObject currentHeld;
    private Item equippedItem;
    private NewPlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
    }

    private void Start()
    {
        //  Check if this character is the leader
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

        //  Don't spawn if player is in battle
        if (BattleManager.instance != null && BattleManager.instance.inBattle)
        {
            Debug.Log("In battle — skipping starting weapon spawn.");
            return;
        }

        //  Only add if player doesn't already have it
        if (InventoryManager.Instance != null)
        {
            if (!InventoryManager.Instance.HasItem(startingItem))
            {
                InventoryManager.Instance.AddItemToInventory(startingItem, 1);
                DisplayItem(startingItem);
                Debug.Log($"Spawned and added starting item: {startingItem.itemName}");
            }
            else
            {
                Debug.Log($"Player already owns '{startingItem.itemName}', not spawning again.");
            }
        }
        else
        {
            Debug.LogWarning("No InventoryManager found when trying to add starting item!");
        }
    }

    private void Update()
    {
        if (currentHeld != null && playerMovement != null)
        {
            // Flip logic (optional)
        }
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
    }

    public void RemoveItem()
    {
        if (currentHeld != null)
        {
            Destroy(currentHeld);
            currentHeld = null;
        }

        equippedItem = null;
    }

    public void HideItem()
    {
        if (currentHeld != null)
        {
            currentHeld.SetActive(false);
        }
        equippedItem = null;
    }

    public Item GetEquippedItem()
    {
        return equippedItem;
    }
}
