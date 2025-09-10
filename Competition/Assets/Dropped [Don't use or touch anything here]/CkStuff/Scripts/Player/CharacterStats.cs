using UnityEngine;

public enum ElementType { None, Fire, Water, Grass, Dark, Light }

[CreateAssetMenu(menuName = "Character/Stats", fileName = "CharacterStats")]
public class CharacterStats : ScriptableObject
{
    [Header("General")]
    public string characterName = "New Character";
    public int level = 1;

    [Header("Health & Defense")]
    public int maxHealth = 100;
    public int defense = 10;

    [Header("Combat")]
    public int baseDamage = 20;

    public float attackSpeed = 1.0f;

    [Range(0f, 1f)] public float critRate = 0.1f;
    public float critMultiplier = 1.5f;

    [Header("Elemental")]
    public ElementType element = ElementType.None;
}
