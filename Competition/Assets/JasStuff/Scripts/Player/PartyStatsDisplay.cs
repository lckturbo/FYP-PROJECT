using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyStatsDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bgImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Transform statsContainer;
    [SerializeField] private StatsDisplay statsDisplay;
    private PlayerLevelApplier _applier;

    public void SetData(PlayerLevelApplier applier)
    {
        if (!applier || applier.definition == null) return;

        if (_applier != null)
            _applier.OnStatsUpdated -= RefreshStats;

        _applier = applier;
        _applier.OnStatsUpdated += RefreshStats;

        if (bgImage) bgImage.color = applier.definition.uiColor;
        if (nameText) nameText.text = applier.definition.displayName.ToUpper();

        RefreshStats(applier.runtimeStats);
    }

    private void OnDisable()
    {
        if (_applier != null)
            _applier.OnStatsUpdated -= RefreshStats;
    }

    private void RefreshStats(NewCharacterStats stats)
    {
        if (statsDisplay && stats != null)
            statsDisplay.DisplayStats(stats);
    }
}