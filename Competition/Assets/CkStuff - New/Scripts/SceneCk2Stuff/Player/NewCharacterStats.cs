using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/CharacterStats")]
public class NewCharacterStats : BaseStats
{
    [Header("General")] 
    public int level;

    [Header("Combat")]
    public float atkCD; //this is just to trigger encounter
    public float atkRange; //this is just to trigger encounter
}
