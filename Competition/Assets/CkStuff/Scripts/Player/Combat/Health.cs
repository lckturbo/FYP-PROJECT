using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int currentHealth;
    public float healRate = 0f;

    float healAccumulator;

    void Start()
    {
        currentHealth = maxHealth;
    }
    public void ApplyStats(CharacterStats stats)
    {
        maxHealth = stats.maxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
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
        Debug.Log("(Player) Health: " + currentHealth);
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        // TODO: death, i-frames, sfx
    }
}
