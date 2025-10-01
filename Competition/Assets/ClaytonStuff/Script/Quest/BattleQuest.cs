using UnityEngine;

public class BattleQuest : Quest
{
    private BattleQuestData battleData;

    public override void StartQuest()
    {
        battleData = (BattleQuestData)questData;

        //  Subscribe to global event
        BattleManager.OnGlobalBattleEnd += HandleGlobalBattleEnd;
    }

    public override void CheckProgress()
    {
        // No polling needed, handled by events
    }

    private void HandleGlobalBattleEnd(string defeatedPartyID, bool playerWon)
    {
        if (!playerWon) return;

        //  Check if this is the right enemy party
        if (!string.IsNullOrEmpty(defeatedPartyID) && defeatedPartyID == battleData.targetEnemyPartyID)
        {
            CompleteQuest();
        }
    }

    private void OnDestroy()
    {
        //  Always unsubscribe to prevent leaks
        BattleManager.OnGlobalBattleEnd -= HandleGlobalBattleEnd;
    }
}
