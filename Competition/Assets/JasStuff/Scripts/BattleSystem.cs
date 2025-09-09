using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    BATTLEWIN,
    BATTLELOSE,
}
public class BattleSystem : MonoBehaviour
{
    public BattleState battleState;
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("SpawnPoints")]
    [SerializeField] private Transform playerSpawnPt;
    [SerializeField] private Transform enemySpawnPt;

    [Header("UI")]
    [SerializeField] private Slider playerHealth;
    [SerializeField] private Slider enemyHealth;
    [SerializeField] private TMP_Text turnText;

    private GameObject _player;
    private GameObject _enemy;

    [Header("FOR TESTING ONLY")]
    [SerializeField] private GameObject normalScene;
    [SerializeField] private GameObject battleScene;

    public void RegisterEnemy(EnemyBase enemy)
    {
        enemy.OnAttackPlayer += HandleBattleTransition;
    }

    public void UnRegisterEnemy(EnemyBase enemy)
    {
        enemy.OnAttackPlayer -= HandleBattleTransition;
    }

    public void HandleBattleTransition(GameObject player, EnemyBase enemy)
    {
        _player = player;
        _enemy = enemy.gameObject;
        battleState = BattleState.START;
        SetupBattle();
    }

    private void Start()
    {
        battleState = BattleState.START;

        SetupBattle(); // FOR TESTING -> CALL WHEN ENEMY HIT PLAYER
    }

    private void SetupBattle()
    {
        _player = Instantiate(playerPrefab, playerSpawnPt.position, Quaternion.identity);
        _enemy = Instantiate(enemyPrefab, enemySpawnPt.position, Quaternion.identity);

        // FOR BATTLE MODE
        _player.transform.localScale = new Vector2(0.3f, 0.3f); // remove later
        _player.GetComponent<PlayerMovement>().enabled = false;
        _player.GetComponent<Rigidbody2D>().gravityScale = 0;

        if (_player != null && _enemy != null)
            SetUpHealth(_player, _enemy);

        // PLAYER TURN FIRST
        //PlayerTurn();
        EnemyTurn();
    }

    private void SetUpHealth(GameObject player, GameObject enemy)
    {
        playerHealth.maxValue = player.GetComponent<NewHealth>().GetMaxHealth();
        playerHealth.value = playerHealth.maxValue;
        MsgLog("Player MaxHealth: " + player.GetComponent<NewHealth>().GetMaxHealth());
        enemyHealth.maxValue = enemy.GetComponent<EnemyBase>().GetMaxHealth();
        enemyHealth.value = enemyHealth.maxValue;
        MsgLog("Enemy MaxHealth: " + enemy.GetComponent<EnemyBase>().GetMaxHealth());
    }
    private bool CheckHealth()
    {
        int playerCurrHealth = _player.GetComponent<NewHealth>().GetCurrHealth();
        int enemyCurrHealth = _enemy.GetComponent<EnemyBase>().GetCurrHealth();

        if (playerCurrHealth <= 0)
        {
            BattleLose();
            return false;
        }
        if (enemyCurrHealth <= 0)
        {
            BattleWin();
            return false;
        }
        return true;
    }
    private void PlayerTurn()
    {
        battleState = BattleState.PLAYERTURN;
        turnText.text = "PLAYER TURN";

        // PLAYER HIT -> PARTY HIT -> PARTY HIT
        _enemy.GetComponent<EnemyBase>().TakeDamage(10);
        // CHECK HEALTH
        if (CheckHealth())
        {
            // ENEMY TURN
            EnemyTurn();
        }

    }

    private void EnemyTurn()
    {
        battleState = BattleState.ENEMYTURN;
        turnText.text = "ENEMY TURN";

        // ENEMY HIT -> PLAYER TURN
        _enemy.GetComponent<EnemyBase>()._states = EnemyBase.EnemyStates.Attack;
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        while (_enemy.GetComponent<EnemyBase>()._states == EnemyBase.EnemyStates.Attack)
            yield return null;

        playerHealth.value = _player.GetComponent<NewHealth>().GetCurrHealth();
        MsgLog("Player currHealth: " + _player.GetComponent<NewHealth>().GetCurrHealth());
        if(CheckHealth())
            PlayerTurn();
    }

    private void BattleWin()
    {
        battleState = BattleState.BATTLEWIN;
        turnText.text = "Battle Win";
        Time.timeScale = 0;
        MsgLog("Player win");
        // game scene
        if(normalScene != null && battleScene != null)
        {
            Destroy(_enemy);
            Destroy(_player);
            normalScene.SetActive(true);
            battleScene.SetActive(false);
        }
    }

    private void BattleLose()
    {
        battleState = BattleState.BATTLELOSE;
        turnText.text = "Battle Lose";
        Time.timeScale = 0;
        MsgLog("Player lost");
        // lose scene
    }

    private void MsgLog(string msg)
    {
        if (msg != null)
            Debug.Log("(BATTLESYSTEM) " + msg);
    }
}
