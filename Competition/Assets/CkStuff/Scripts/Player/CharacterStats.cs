using UnityEngine;

[CreateAssetMenu(menuName = "Character/Stats", fileName = "CharacterStats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 0;
}
