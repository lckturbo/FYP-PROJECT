using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss Dialogue Before Battle")]
    [SerializeField] private DialogueData bossDialogue;

    [Header("Party Reference")]
    [SerializeField] private EnemyParty enemyParty;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        Debug.Log("[BossTrigger] Player entered boss area Triggering boss dialogue.");

        // Disable movement here automatically in DialogueManager
        DialogueManager.Instance.StartDialogue(
            bossDialogue,
            onEnd: StartBossBattle   // <- callback when dialogue ends
        );
    }

    private void StartBossBattle()
    {
        Debug.Log("[BossTrigger] Dialogue finished Starting boss battle.");

        if (enemyParty == null)
        {
            Debug.LogError("[BossTrigger] Missing EnemyParty reference!");
            return;
        }

        EnemyBase baseEnemy = enemyParty.GetComponent<EnemyBase>();
        if (baseEnemy != null)
            BattleManager.instance.RegisterEnemy(baseEnemy);

        BattleManager.instance.SetBattleMode(true);
        BattleManager.instance.HandleBattleTransition(enemyParty);
    }
}
