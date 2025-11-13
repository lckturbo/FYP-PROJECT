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
        List<PlayerLevelApplier> partyInstances = PlayerParty.instance.GetRuntimeParty();

        if (partyInstances.Count > 0) leaderPanel.SetData(partyInstances[0]);
        if (partyInstances.Count > 1) ally1Panel.SetData(partyInstances[1]);
        if (partyInstances.Count > 2) ally2Panel.SetData(partyInstances[2]);
    }
}
