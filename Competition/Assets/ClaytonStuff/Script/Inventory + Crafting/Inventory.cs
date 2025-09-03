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
        List<InventorySlot> targetInventory =
            (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        if (item.isStackable)
        {
            // Try to find an existing stack
            InventorySlot slot = targetInventory.Find(s => s.item == item);
            if (slot != null)
            {
                slot.quantity += amount;
                return;
            }
        }

        // For non-stackables OR new stack, add new slot(s)
        for (int i = 0; i < amount; i++)
        {
            targetInventory.Add(new InventorySlot(item, item.isStackable ? amount : 1));
            if (item.isStackable) break; // already added whole stack
        }
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