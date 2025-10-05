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
                Debug.Log($"Target {defeatedPartyID} completed! ({GetProgressText()})");
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

    //  New: Report progress like "1/2 defeated"
    public override string GetProgressText()
    {
        if (completedTargets == null || completedTargets.Length == 0)
            return "No targets";

        int defeatedCount = 0;
        foreach (bool done in completedTargets)
        {
            if (done) defeatedCount++;
        }

        return $"{defeatedCount}/{completedTargets.Length} defeated";
    }

    private void OnDestroy()
    {
        BattleManager.OnGlobalBattleEnd -= HandleGlobalBattleEnd;
    }
}
