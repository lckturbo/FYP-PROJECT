using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/CharacterStats")]
public class NewCharacterStats : BaseStats
{
    [Header("General")] 
    public int level;

    [Header("Combat")]
    public float atkCD;
    public float atkRange;
}
