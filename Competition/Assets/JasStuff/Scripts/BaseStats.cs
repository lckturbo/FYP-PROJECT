using UnityEngine;

public enum NewElementType { None, Fire, Water, Grass, Dark, Light }

[CreateAssetMenu(fileName = "NewBaseStats", menuName = "Stats/BaseStats")]
public class BaseStats : ScriptableObject
{
    [Header("Movement")]
    public float Speed;

    [Header("Health & Defense")]
    public int maxHealth; //your current one @jas

    public float attackreduction; //aka flat defense

    [Header("Combat")]
    public int atkDmg; //your current one @jas

    public float actionvaluespeed;

    [Range(0f, 1f)] public float critRate;
    public float critDamage;

    [Header("Elemental")]
    public NewElementType attackElement = NewElementType.None;
    public NewElementType defenseElement = NewElementType.None;

    // 1.0 = neutral, <1 = resists (less damage), >1 = weak (more damage)
    [Range(0f, 2f)] public float fireRes;
    [Range(0f, 2f)] public float waterRes;
    [Range(0f, 2f)] public float grassRes;
    [Range(0f, 2f)] public float darkRes;
    [Range(0f, 2f)] public float lightRes;

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
