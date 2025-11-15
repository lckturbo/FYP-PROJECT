using UnityEngine;

public class BossDialogueTrigger : MonoBehaviour
{
    [Header("Boss Dialogue Before Battle")]
    public DialogueData bossDialogue;

    private bool triggered = false;

    private void Start()
    {
        if (!CompareTag("Boss"))
            Debug.LogWarning($"{name} is missing the 'Boss' tag!");
    }

    // Call this when boss takes damage
    public void TriggerBossDialogue()
    {
        if (triggered) return;
        triggered = true;

        Debug.Log("[BossDialogueTrigger] Boss dialogue triggered.");

        DialogueManager.Instance.StartDialogue(
            bossDialogue,
            onEnd: () =>
            {
                Debug.Log("[BossDialogueTrigger] Dialogue finished Entering boss battle.");
                StartBossBattle();
            }
        );
    }

    private void StartBossBattle()
    {
        EnemyParty party = GetComponent<EnemyParty>();
        if (party != null)
        {
            BattleManager.instance.HandleBattleTransition(party);
            return;
        }

        Debug.LogError("[BossDialogueTrigger] Boss has no EnemyParty component!");
    }
}
