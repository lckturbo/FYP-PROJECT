using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private EnemyParty enemyParty; 

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        Debug.Log("[BossTrigger] Player entered boss area. Starting battle...");

        BattleManager.instance.SetBattleMode(true);

        BattleManager.instance.RegisterEnemy(enemyParty.GetComponent<EnemyBase>());
        BattleManager.instance.HandleBattleTransition(enemyParty);
    }
}
