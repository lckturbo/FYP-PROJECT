using System.Collections.Generic;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject statRowPrefab;

    public void DisplayStats(NewCharacterStats stats)
    {
        if (stats == null) return;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        List<(string label, string value)> statList = new()
        {
            ("Level", stats.level.ToString()),
            ("Health", stats.maxHealth.ToString("0")),
            ("Damage", stats.atkDmg.ToString("0")),
            ("Atk Element", stats.attackElement.ToString()),
            ("Def Element", stats.defenseElement.ToString()),
        };

        foreach (var stat in statList)
        {
            var row = Instantiate(statRowPrefab, container);
            var ui = row.GetComponent<StatRowUI>();
            ui.Bind(stat.label, stat.value);
        }
    }
}
