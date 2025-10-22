using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    [SerializeField] private NewCharacterStats playerStats;
    [SerializeField] private RectTransform container; 
    [SerializeField] private GameObject statRowPrefab;

    public void DisplayStats()
    {
        if (!playerStats)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player)
            {
                var movement = player.GetComponent<NewPlayerMovement>();
                if (movement)
                    playerStats = movement.GetStats();
            }
        }

        if (!playerStats) return;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        List<(string label, string value)> statList = new()
        {
            ("Level", playerStats.level.ToString()),
            ("Health", playerStats.maxHealth.ToString("0")),
            ("Damage", playerStats.atkDmg.ToString("0")),
            ("Atk Element", playerStats.attackElement.ToString()),
            ("Def Element", playerStats.defenseElement.ToString()),
        };

        foreach (var stat in statList)
        {
            GameObject row = Instantiate(statRowPrefab, container);
            StatRowUI ui = row.GetComponent<StatRowUI>();
            ui.Bind(stat.label, stat.value);
        }
    }
}
