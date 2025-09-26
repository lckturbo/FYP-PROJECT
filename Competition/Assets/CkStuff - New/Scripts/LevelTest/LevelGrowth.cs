using UnityEngine;

[CreateAssetMenu(fileName = "LevelGrowth", menuName = "Stats/Level Growth")]
public class LevelGrowth : ScriptableObject
{
    [Header("Additive per level (applied each level-up)")]
    public int maxHealthAdd;
    public int atkDmgAdd;
    public float defenseAdd;
    public float speedAdd;
    public float critRateAdd;
    public float critDmgAdd;

    [Header("Multipliers per level (applied cumulatively)")]
    public float maxHealthMul;
    public float atkDmgMul;
    public float speedMul;
}
