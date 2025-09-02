using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class Inventory : MonoBehaviour
{
    public List<InventorySlot> mainInventory = new List<InventorySlot>();
    public List<InventorySlot> subInventory = new List<InventorySlot>();

    public void AddItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory = (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        // If stackable and already in inventory, just add
        if (item.isStackable)
        {
            InventorySlot slot = targetInventory.Find(s => s.item == item);
            if (slot != null)
            {
                slot.quantity += amount;
                return;
            }
        }

        // Otherwise create a new slot
        targetInventory.Add(new InventorySlot(item, amount));
    }

    public void RemoveItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory = (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        InventorySlot slot = targetInventory.Find(s => s.item == item);
        if (slot != null)
        {
            slot.quantity -= amount;
            if (slot.quantity <= 0)
            {
                targetInventory.Remove(slot);
            }
        }
    }

    public bool HasItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory = (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        InventorySlot slot = targetInventory.Find(s => s.item == item);
        return slot != null && slot.quantity >= amount;
    }
}