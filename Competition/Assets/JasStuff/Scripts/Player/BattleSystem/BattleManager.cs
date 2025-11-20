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

    private bool isBossBattle = false;
    public void SetBossBattle(bool isBossBattle)
    {
        this.isBossBattle = isBossBattle;
    }
    public bool IsBossBattle => isBossBattle;

    [SerializeField] private DialogueData dialogue;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }
    //private void Update()
    //{
    //    if (!inBattle) return;

    //    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
    //    {
    //        Debug.Log("[CHEAT] Forced WIN");
    //        HandleBattleEnd(true);
    //    }

    //    // CHEAT: Instant Lose (Shift + L)
    //    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
    //    {
    //        Debug.Log("[CHEAT] Forced LOSE");
    //        HandleBattleEnd(false);
    //    }
    //}

    public void RegisterEnemy(EnemyBase enemy)
    {
        enemy.OnAttackPlayer += HandleBattleTransition;

        if (enemy.GetEnemyStats().type == EnemyStats.EnemyTypes.MiniBoss)
        {
            Debug.Log("this is a boss battle");
            isBossBattle = true;
        }
        else
            isBossBattle = false;
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

        var enemies = enemyParty.GetEnemies();
        foreach (var enemyObj in enemies)
        {
            var eb = enemyObj.GetComponent<EnemyBase>();
            if (eb != null && eb.GetEnemyStats().type == EnemyStats.EnemyTypes.MiniBoss)
            {
                isBossBattle = true;
                Debug.Log("[BattleManager] Boss battle detected!");
                break;
            }
        }

        SaveLoadSystem.instance.SaveGame();
        if (CheckpointManager.instance != null) CheckpointManager.instance.ClearCheckpoints();
        Debug.Log("changing to battle");
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
            Debug.Log("Defeated");

            if (data != null)
                data.hasSavedPosition = false;

            if (QuestManager.Instance != null)
                QuestManager.Instance.ResetAllQuests();

            SaveLoadSystem.instance.SaveGame(false, false);
        }
        OnGlobalBattleEnd?.Invoke(enemyPartyID, playerWon);

        if (IsBossBattle && playerWon)
        {
            Debug.Log("[BattleManager] Boss defeated, playing final dialogue.");
            isBossBattle = false;
            DialogueManager.Instance.StartDialogue(dialogue, OnFinalDialogueFinished);
            return; 
        }
        else
        {
            isBossBattle = false;
            GameManager.instance.ChangeScene("SampleScene");
        }

    }
    private void OnFinalDialogueFinished()
    {
        Debug.Log("[BattleManager] Final dialogue finished, switching to credits.");

        SaveLoadSystem.instance.NewGame(false);
        GameManager.instance.ChangeScene("Credits");
    }

    public void SetBattlePaused(bool paused)
    {
        var turnEngine = FindObjectOfType<TurnEngine>();
        if (turnEngine)
            turnEngine.SetPaused(paused);
    }
}
