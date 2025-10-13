using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool inBattle;
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

        //NewPlayerMovement player = GameObject.FindGameObjectWithTag("Player").GetComponent<NewPlayerMovement>();
        //if (player != null)
        //{
        //    player.SaveEncounterPosition();
        //}

        SaveLoadSystem.instance.SaveGame();
        GameManager.instance.ChangeScene("jasBattle");
    }

    public void HandleBattleEnd(bool playerWon)
    {
        inBattle = false;

        OnClearAllBuffs?.Invoke();
        var data = SaveLoadSystem.instance.GetGameData();

        if (playerWon)
        {
            Debug.Log("Victory");

            if (!string.IsNullOrEmpty(enemyPartyID))
            {
                if (EnemyTracker.instance)
                    EnemyTracker.instance.MarkDefeated(enemyPartyID);
            }

            if (InventoryManager.Instance != null && InventoryManager.Instance.PlayerInventory != null)
            {
                InventoryManager.Instance.PlayerInventory.AddMoney(1000);
                Debug.Log($"[Battle Reward] +100 Money. Total: {InventoryManager.Instance.PlayerInventory.Money}");
            }
            else
            {
                Debug.LogWarning("No InventoryManager or PlayerInventory found when trying to reward money!");
            }

            SaveLoadSystem.instance.SaveGame(false, true);
        }
        else
        {
            Debug.Log("Defeated"); // TODO: GAME OVER UI

            if (QuestManager.Instance != null)
                QuestManager.Instance.ResetAllQuests();

            SaveLoadSystem.instance.NewGame(true);
            SaveLoadSystem.instance.SaveGame(false, false);
        }
        OnGlobalBattleEnd?.Invoke(enemyPartyID, playerWon);


        GameManager.instance.ChangeScene("SampleScene");
    }
}
