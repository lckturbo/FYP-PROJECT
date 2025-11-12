using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyStatsPanel : MonoBehaviour
{
    [SerializeField] private PartyStatsDisplay leaderPanel;
    [SerializeField] private PartyStatsDisplay ally1Panel;
    [SerializeField] private PartyStatsDisplay ally2Panel;

    private void Start()
    {
        UpdatePartyDisplay();
    }

    public void UpdatePartyDisplay()
    {
        var party = PlayerParty.instance.GetFullParty();

        if (party.Count > 0) leaderPanel.SetData(party[0]);
        if (party.Count > 1) ally1Panel.SetData(party[1]);
        if (party.Count > 2) ally2Panel.SetData(party[2]);
    }
}
