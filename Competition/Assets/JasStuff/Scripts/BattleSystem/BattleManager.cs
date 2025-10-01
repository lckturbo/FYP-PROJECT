using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    private bool inBattle;

    public GameObject playerRef { get; private set; }
    public EnemyParty enemypartyRef { get; private set; }
    private string enemyPartyID;

    public static event Action<string, bool> OnGlobalBattleEnd;
    public event Action OnBattleStart;
    public event Action<bool> OnBattleEndEvent;

    //  New event for buff cleanup
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

    public void SetBattleMode(bool v) => inBattle = v;

    public bool GetBattleMode() => inBattle;

    public void HandleBattleTransition(GameObject player, EnemyParty enemyParty)
    {
        playerRef = player;
        enemypartyRef = enemyParty;
        enemyPartyID = enemypartyRef.GetID();

        OnBattleStart?.Invoke(); // notify listeners

        SaveLoadSystem.instance.SaveGame();
        GameManager.instance.ChangeScene("jasBattle");
    }

    public void HandleBattleEnd(bool playerWon)
    {
        inBattle = false;

        OnBattleEndEvent?.Invoke(playerWon);

        // Remove buffs from the player in the previous scene
        var player = FindObjectOfType<PlayerBuffHandler>();
        if (player != null)
        {
            player.ClearAllBuffs();
            Debug.Log("Buffs cleared before scene change.");
        }

        if (playerWon)
        {
            Debug.Log("Victory");
            if (!string.IsNullOrEmpty(enemyPartyID))
            {
                EnemyTracker.instance?.MarkDefeated(enemyPartyID);
            }
        }
        else
        {
            Debug.Log("Defeated");
            SaveLoadSystem.instance.NewGame(true);
            SaveLoadSystem.instance.SaveGame(false);
        }

        OnGlobalBattleEnd?.Invoke(enemyPartyID, playerWon);

        GameManager.instance.ChangeScene("SampleScene");
    }

}
