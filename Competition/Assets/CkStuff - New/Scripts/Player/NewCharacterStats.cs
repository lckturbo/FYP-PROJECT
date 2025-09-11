using UnityEngine;
using static EnemyStats;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/CharacterStats")]
public class NewCharacterStats : MonoBehaviour
{
    [Header("Leveling")]
    public int level;

    [Header("Combat")]
    public float atkRange;
}
