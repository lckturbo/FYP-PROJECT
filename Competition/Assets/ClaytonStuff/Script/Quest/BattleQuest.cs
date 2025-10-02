using UnityEngine;

public class BattleQuest : Quest
{
    private BattleQuestData battleData;
    private bool[] completedTargets;

    public override void StartQuest()
    {
        battleData = (BattleQuestData)questData;

        if (battleData.targetEnemyPartyIDs != null)
            completedTargets = new bool[battleData.targetEnemyPartyIDs.Length];

        BattleManager.OnGlobalBattleEnd += HandleGlobalBattleEnd;
    }

    public override void CheckProgress()
    {
        // handled by events
    }

    private void HandleGlobalBattleEnd(string defeatedPartyID, bool playerWon)
    {
        if (!playerWon) return;
        if (battleData.targetEnemyPartyIDs == null) return;

        for (int i = 0; i < battleData.targetEnemyPartyIDs.Length; i++)
        {
            if (!completedTargets[i] && battleData.targetEnemyPartyIDs[i] == defeatedPartyID)
            {
                completedTargets[i] = true;
                Debug.Log($"Target {defeatedPartyID} completed!");
            }
        }

        // Complete quest if all targets defeated
        bool allCompleted = true;
        for (int i = 0; i < completedTargets.Length; i++)
        {
            if (!completedTargets[i])
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            CompleteQuest();
        }
    }

    private void OnDestroy()
    {
        BattleManager.OnGlobalBattleEnd -= HandleGlobalBattleEnd;
    }
}
