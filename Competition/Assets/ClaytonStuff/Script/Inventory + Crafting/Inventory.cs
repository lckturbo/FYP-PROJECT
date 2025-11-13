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

public class Inventory : MonoBehaviour, IDataPersistence
{
    public List<InventorySlot> mainInventory = new List<InventorySlot>();
    public List<InventorySlot> subInventory = new List<InventorySlot>();

    public static Inventory Instance { get; private set; }

    [Header("Limits")]
    public int mainInventoryLimit = 6;  // max slots for main

    [Header("Currency")]
    public int Money = 100; // starting money, adjust as needed

    public bool CanAddItem(Item item)
    {
        List<InventorySlot> targetInventory =
            (item.category == ItemCategory.Main) ? mainInventory : subInventory;

        //  Prevent more than one sword or bow in main inventory
        if (item.category == ItemCategory.Main)
        {
            bool alreadyHasSword = mainInventory.Exists(s => s.item != null && s.item.isWeapon && !s.item.isBow);
            bool alreadyHasBow = mainInventory.Exists(s => s.item != null && s.item.isBow);

            if (item.isWeapon && !item.isBow && alreadyHasSword)
            {
                Debug.LogWarning("Main inventory already has a sword! Cannot add another.");
                return false;
            }

            if (item.isBow && alreadyHasBow)
            {
                Debug.LogWarning("Main inventory already has a bow! Cannot add another.");
                return false;
            }
        }

        // If item is stackable and already exists  can add more
        if (item.isStackable)
        {
            InventorySlot slot = targetInventory.Find(s => s.item == item);
            if (slot != null) return true;
        }

        // Otherwise, check slot capacity
        if (item.category == ItemCategory.Main)
            return mainInventory.Count < mainInventoryLimit;

        return true;
    }


    public void AddItem(Item item, int amount = 1)
    {
        if (!CanAddItem(item))
        {
            Debug.LogWarning($"Cannot add {item.itemName}, inventory is full!");
            return;
        }

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

    public void ClearInventory()
    {
        mainInventory.Clear();
        subInventory.Clear();
        Money = 0;

        Debug.Log("[Inventory] Inventory cleared for New Game.");
    }


    public void LoadData(GameData data)
    {
        mainInventory.Clear();
        subInventory.Clear();

        foreach (var entry in data.mainInventoryData)
        {
            Item loadedItem = Resources.Load<Item>($"Items/{entry.itemName}");
            if (loadedItem != null)
                mainInventory.Add(new InventorySlot(loadedItem, entry.quantity));
        }

        foreach (var entry in data.subInventoryData)
        {
            Item loadedItem = Resources.Load<Item>($"Items/{entry.itemName}");
            if (loadedItem != null)
                subInventory.Add(new InventorySlot(loadedItem, entry.quantity));
        }

        Money = data.money;
    }

    public void SaveData(ref GameData data)
    {
        data.mainInventoryData.Clear();
        data.subInventoryData.Clear();

        foreach (var slot in mainInventory)
        {
            if (slot.item != null)
                data.mainInventoryData.Add(new GameData.InventoryItemData(slot.item.itemName, slot.quantity));
        }

        foreach (var slot in subInventory)
        {
            if (slot.item != null)
                data.subInventoryData.Add(new GameData.InventoryItemData(slot.item.itemName, slot.quantity));
        }

        data.money = Money;
    }
}
