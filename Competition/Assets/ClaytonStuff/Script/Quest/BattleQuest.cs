using UnityEngine;

public class BattleQuest : Quest
{
    private BattleQuestData battleData;
    private int killCount = 0;

    // For popup after returning from battle scene
    public static bool ShouldShowBattlePopupAfterReload { get; private set; } = false;

    public override void StartQuest()
    {
        battleData = (BattleQuestData)questData;

        BattleManager.OnGlobalBattleEnd += HandleGlobalBattleEnd;
    }

    public override void CheckProgress()
    {
        // handled by events
    }

    private void HandleGlobalBattleEnd(string defeatedPartyID, bool playerWon)
    {
        if (!playerWon) return;

        // Each battle = one kill (or count party size)
        killCount++;

        Debug.Log($"Battle Quest Kill Count: {killCount}/{battleData.requiredKills}");

        // Completed?
        if (killCount >= battleData.requiredKills)
        {
            CompleteQuest();
            ShouldShowBattlePopupAfterReload = true;
        }
    }

    public override string GetProgressText()
    {
        return $"{killCount}/{battleData.requiredKills} defeated";
    }

    private void OnDestroy()
    {
        BattleManager.OnGlobalBattleEnd -= HandleGlobalBattleEnd;
    }

    public static void ClearBattlePopupFlag()
    {
        ShouldShowBattlePopupAfterReload = false;
    }
}
