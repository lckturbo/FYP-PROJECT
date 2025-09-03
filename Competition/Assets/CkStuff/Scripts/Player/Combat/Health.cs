using UnityEngine;

public class Health : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;
    public float healRate = 0f;

    float healAccumulator;

    public void ApplyStats(CharacterStats stats)
    {
        maxHealth = stats.maxHealth;
        //currentHealth = Mathf.Min(currentHealth, maxHealth); 
        currentHealth = maxHealth;
        healRate = stats.healRate;
    }

    void Update()
    {
        if (healRate > 0f && currentHealth < maxHealth)
        {
            healAccumulator += healRate * Time.deltaTime;
            if (healAccumulator >= 1f)
            {
                int heal = Mathf.FloorToInt(healAccumulator);
                healAccumulator -= heal;
                currentHealth = Mathf.Min(maxHealth, currentHealth + heal);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        // TODO: death, i-frames, sfx
    }
}
