using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/CharacterStats")]
public class NewCharacterStats : BaseStats
{
    [Header("General")] 
    public string characterName = "Name";
    public int level;

    [Header("Combat")]
    public float atkCD; //this is just to trigger encounter
    public float atkRange; //this is just to trigger encounter
}
