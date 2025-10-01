using UnityEngine;

public class BattleQuest : Quest
{
    private BattleQuestData battleData;
    private bool registered;

    public override void StartQuest()
    {
        battleData = (BattleQuestData)questData;

        // Subscribe to battle end
        var bm = BattleManager.instance;
        if (bm != null)
        {
            var bs = FindObjectOfType<BattleSystem>();
            if (bs != null)
            {
                bs.OnBattleEnd += OnBattleEnd;
                registered = true;
            }
        }
    }

    public override void CheckProgress()
    {
        // Nothing to check every frame, we only care about battle events
    }

    private void OnBattleEnd(bool playerWon)
    {
        if (!playerWon) return;

        // Check if the defeated enemy matches the quest target
        string lastEnemyID = BattleManager.instance?.enemypartyRef?.GetID();
        if (!string.IsNullOrEmpty(lastEnemyID) && lastEnemyID == battleData.targetEnemyPartyID)
        {
            CompleteQuest();
        }
    }

    private void OnDestroy()
    {
        if (registered)
        {
            var bs = FindObjectOfType<BattleSystem>();
            if (bs != null)
                bs.OnBattleEnd -= OnBattleEnd;
        }
    }
}
