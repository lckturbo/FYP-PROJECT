using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    private bool inBattle;
    
    public GameObject playerRef { get; private set; }
    public EnemyParty enemypartyRef { get; private set; }
    private string enemyPartyID;

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

        var bs = FindObjectOfType<BattleSystem>();
        if (bs)
            bs.OnBattleEnd -= HandleBattleEnd;
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
        enemyPartyID = enemypartyRef.GetID();

        GameManager.instance.ChangeScene("jasBattle");
    }

    public void HandleBattleEnd(bool playerWon)
    {
        inBattle = false;
        if (playerWon)
        {
            Debug.Log("Victory"); // TODO: HANDLE XP UI

            if (!string.IsNullOrEmpty(enemyPartyID))
            {
                if (EnemyTracker.instance)
                    EnemyTracker.instance.MarkDefeated(enemyPartyID);
            }
        }
        else
            Debug.Log("Defeated"); // TODO: GO BACK MAIN MENU -> when leader died // FOR ALPHA

        GameManager.instance.ChangeScene("SampleScene");
    }
}
