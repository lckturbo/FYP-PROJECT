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

    public void SetData(NewCharacterDefinition character)
    {
        if (!character) return;

        if (bgImage) bgImage.color = character.uiColor;
        if (nameText) nameText.text = character.displayName.ToUpper();

        if (statsDisplay)
            statsDisplay.DisplayStats(character.stats);
    }
}