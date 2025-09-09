using UnityEngine;

[CreateAssetMenu(fileName = "NewBaseStats", menuName = "Stats/BaseStats")]
public class BaseStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth;

    [Header("Combat")]
    public int atkDmg;
}
