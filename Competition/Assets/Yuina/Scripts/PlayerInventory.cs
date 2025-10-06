using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int startingMoney;
    public int Money { get; private set; }

    public List<ShopItem> ownedItems = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Money = startingMoney;
    }

    public bool TrySpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    public void AddItem(ShopItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add null item to inventory!");
            return;
        }

        // Check if item already exists
        bool alreadyOwned = ownedItems.Exists(owned => owned.itemName == item.itemName);

        if (alreadyOwned)
        {
            Debug.LogWarning($"Item '{item.itemName}' already in inventory! Not adding again.");
            return;
        }

        ownedItems.Add(item);
        Debug.Log($"Added new item: {item.itemName}");
    }


    // Duplicate Check for Items with Multiple Possession Restrictions
    public bool HasItem(ShopItem item)
    {
        if (item.type != ItemType.Unique)
            return false; // non-unique items can stack or repeat

        return ownedItems.Exists(owned => owned.itemName == item.itemName);
    }


}
