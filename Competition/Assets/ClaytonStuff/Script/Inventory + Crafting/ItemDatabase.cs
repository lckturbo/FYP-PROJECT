using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems = new List<Item>();

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
            return _instance;
        }
    }

    public static Item GetItemByID(string id)
    {
        if (Instance == null) return null;
        return Instance.allItems.Find(i => i.itemName == id);
    }
}
