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
    [Header("Basic Info")]  // ��{���
    public string itemName;           // �A�C�e����
    [TextArea] public string description; // ������
    public int price;                 // �l�i

    [Header("Appearance")]  // ������
    public Sprite icon;               // �A�C�R���摜

    [Header("Category")]    // ����
    public ItemType type;             // �A�C�e���^�C�v
}
