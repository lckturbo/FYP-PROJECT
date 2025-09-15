using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int startingMoney; // Inspectorで設定
    public int Money { get; private set; }

    public List<ShopItem> ownedItems = new List<ShopItem>();

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
        ownedItems.Add(item);
    }

    public bool HasItem(ShopItem item)
    {
        // 複数所持不可アイテムは名前とタイプで重複チェック
        foreach (var owned in ownedItems)
        {
            if (owned.itemName == item.itemName && owned.type == ItemType.Unique)
                return true;
        }
        return false;
    }

}
