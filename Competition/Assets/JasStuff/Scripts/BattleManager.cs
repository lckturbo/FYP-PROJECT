using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    private bool inBattle;
    
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

    public void SetBattleMode(bool v)
    {
        inBattle = v;
    }

    public bool GetBattleMode()
    {
        return inBattle;
    }

    public void HandleBattleTransition(GameObject player, EnemyParty enemyParty)
    {
        playerRef = player;
        enemypartyRef = enemyParty;

        // load scene
        GameManager.instance.ChangeScene("jasBattle");
    }
}
