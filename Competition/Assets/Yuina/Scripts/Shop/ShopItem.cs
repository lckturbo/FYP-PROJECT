using UnityEngine;

public enum ItemType
{
    Food,
    Artifact,
    Unique,     // Items that cannot be held in multiple quantities
    Other
}

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    [Header("Basic Info")]  // 基本情報
    public string itemName;           // アイテム名
    [TextArea] public string description; // 説明文
    public int price;                 // 値段

    [Header("Appearance")]  // 見た目
    public Sprite icon;               // アイコン画像

    [Header("Category")]    // 分類
    public ItemType type;             // アイテムタイプ
}
