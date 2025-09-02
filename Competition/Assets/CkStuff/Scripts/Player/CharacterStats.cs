using UnityEngine;

[CreateAssetMenu(menuName = "Character/Stats", fileName = "CharacterStats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 8;
    public float healRate = 0f; // per second (Producer > 0)

    [Header("Stamina (optional)")]
    public float staminaMax = 100f;
    public float staminaRegen = 20f;
}
