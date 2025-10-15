using UnityEngine;

public class BattleQuest : Quest
{
    private BattleQuestData battleData;
    private bool[] completedTargets;

    //  Static flag for scene reload popup control
    public static bool ShouldShowBattlePopupAfterReload { get; private set; } = false;

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

            //  Mark for popup on next scene load
            ShouldShowBattlePopupAfterReload = true;
        }
    }

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

    //  Called after popup has shown, to reset flag
    public static void ClearBattlePopupFlag()
    {
        ShouldShowBattlePopupAfterReload = false;
    }
}
