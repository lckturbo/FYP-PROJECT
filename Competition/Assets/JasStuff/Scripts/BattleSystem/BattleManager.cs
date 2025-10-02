using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    private bool inBattle;
    public string leaderID { get; private set; }
    public EnemyParty enemypartyRef { get; private set; }
    private string enemyPartyID;

    public static event Action<string, bool> OnGlobalBattleEnd;

    public static event Action OnClearAllBuffs;

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
    public void HandleBattleTransition(EnemyParty enemyParty)
    {
        if (PlayerParty.instance) leaderID = PlayerParty.instance.GetLeader().id;

        enemypartyRef = enemyParty;
        enemyPartyID = enemypartyRef.GetID();

        SaveLoadSystem.instance.SaveGame();
        GameManager.instance.ChangeScene("jasBattle");
    }

    public void HandleBattleEnd(bool playerWon)
    {
        inBattle = false;
        OnClearAllBuffs?.Invoke();

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
        {
            Debug.Log("Defeated"); // TODO: GAME OVER UI

            // TODO: go back sample scene (reset every progress) -> when leader died // FOR ALPHA
            SaveLoadSystem.instance.NewGame(true);
            SaveLoadSystem.instance.SaveGame(false);
        }
        OnGlobalBattleEnd?.Invoke(enemyPartyID, playerWon);


        GameManager.instance.ChangeScene("SampleScene");
    }
}
