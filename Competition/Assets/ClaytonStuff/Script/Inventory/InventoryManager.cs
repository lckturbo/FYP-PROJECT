using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Inventory playerInventory;

    void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<Inventory>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("=== Main Inventory ===");
            foreach (var slot in playerInventory.mainInventory)
            {
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");
            }

            Debug.Log("=== Sub Inventory ===");
            foreach (var slot in playerInventory.subInventory)
            {
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");
            }
        }
    }

    public void AddItemToInventory(Item item, int amount = 1)
    {
        playerInventory.AddItem(item, amount);
        Debug.Log($"Added {amount} {item.itemName} to {(item.category == ItemCategory.Main ? "Main" : "Sub")} Inventory");
    }

    public void RemoveItemFromInventory(Item item, int amount = 1)
    {
        playerInventory.RemoveItem(item, amount);
        Debug.Log($"Removed {amount} {item.itemName}");
    }
}
