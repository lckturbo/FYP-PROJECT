using UnityEngine;

public enum ItemCategory
{
    Main,
    Sub
}
public enum ItemTypes
{
    Food,
    Artifact,
    Unique,     // Items that cannot be held in multiple quantities
    Other
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public ItemCategory category; // defines whether item goes to Main or Sub inventory
    public int healing;

    [Header("World/Visuals")]
    public GameObject worldPrefab; // prefab to show in hand or drop

    [Header("Combat")]
    public bool isWeapon; //  Only true items can attack

    [Header("Projectile")]
    public bool isBow;

    [Header("Category")]    // 分類
    public ItemTypes type;             // アイテムタイプ

    [TextArea] public string description; // 説明文
    public int price;                 // 値段

    [Header("Buff Settings")]
    public bool isBuff;
    public int attackBuffAmount;   // flat atk increase
    public float buffDuration;     // seconds

}
