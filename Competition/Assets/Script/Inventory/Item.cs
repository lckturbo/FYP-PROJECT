using UnityEngine;

public enum ItemCategory
{
    Main,
    Sub
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public ItemCategory category; //  defines whether item goes to Main or Sub inventory
}
