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

    [Header("Currency")]
    public int Money = 100; // starting money, adjust as needed

    public void AddItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory =
            (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        if (item.isStackable)
        {
            InventorySlot slot = targetInventory.Find(s => s.item == item);
            if (slot != null)
            {
                slot.quantity += amount;
                return;
            }
        }

        for (int i = 0; i < amount; i++)
        {
            targetInventory.Add(new InventorySlot(item, item.isStackable ? amount : 1));
            if (item.isStackable) break;
        }
    }

    public void RemoveItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory =
            (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        InventorySlot slot = targetInventory.Find(s => s.item == item);
        if (slot != null)
        {
            slot.quantity -= amount;
            if (slot.quantity <= 0)
                targetInventory.Remove(slot);
        }
    }

    public bool HasItem(Item item, int amount = 1)
    {
        List<InventorySlot> targetInventory =
            (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        InventorySlot slot = targetInventory.Find(s => s.item == item);
        return slot != null && slot.quantity >= amount;
    }

    // === Currency Helpers ===
    public bool TrySpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }
}
