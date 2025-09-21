using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public GameObject playerRef { get; private set; }
    public EnemyParty enemypartyRef { get; private set; }

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        enemy.OnAttackPlayer += HandleBattleTransition;
    }

    public void UnRegisterEnemy(EnemyBase enemy)
    {
        enemy.OnAttackPlayer -= HandleBattleTransition;
    }

    public void HandleBattleTransition(GameObject player, EnemyParty enemyParty)
    {
        playerRef = player;
        enemypartyRef = enemyParty;

        // load scene
        GameManager.instance.ChangeScene("jasBattle");
    }
}
