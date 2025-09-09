using UnityEngine;

public enum NewElementType { None, Fire, Water, Grass, Dark, Light }

[CreateAssetMenu(menuName = "NewCharacter/Stats", fileName = "NewCharacterStats")]
public class NewCharacterStats : ScriptableObject
{
    [Header("General")]
    public string characterName = "New Character";
    public int level = 1;

    [Header("Health & Defense")]
    public int maxHealth = 100;
    public int defense = 10;          // flat reduction (you can switch to % later if you want)

    [Header("Combat")]
    public int baseDamage = 20;
    public float attackSpeed = 1.0f;
    [Range(0f, 1f)] public float critRate = 0.1f;     // 0.10 = 10% chance
    public float critDamage = 1.5f;                    // 1.5 = 150% total dmg on crit

    [Header("Elemental (type + resistances)")]
    public NewElementType element = NewElementType.None;

    // 1.0 = neutral, <1 = resists (takes less), >1 = weak (takes more)
    [Range(0f, 2f)] public float fireRes = 1f;
    [Range(0f, 2f)] public float waterRes = 1f;
    [Range(0f, 2f)] public float grassRes = 1f;
    [Range(0f, 2f)] public float darkRes = 1f;
    [Range(0f, 2f)] public float lightRes = 1f;

    public float GetResistance(NewElementType incoming)
    {
        switch (incoming)
        {
            case NewElementType.Fire: return fireRes;
            case NewElementType.Water: return waterRes;
            case NewElementType.Grass: return grassRes;
            case NewElementType.Dark: return darkRes;
            case NewElementType.Light: return lightRes;
            default: return 1f;
        }
    }
}
