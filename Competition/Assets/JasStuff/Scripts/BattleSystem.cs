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
    public static BattleSystem instance;
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

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    //public void RegisterEnemy(EnemyBase enemy)
    //{
    //    enemy.OnAttackPlayer += HandleBattleTransition;
    //}
    //public void UnRegisterEnemy(EnemyBase enemy)
    //{
    //    enemy.OnAttackPlayer -= HandleBattleTransition;
    //}
    public void HandleBattleTransition(GameObject player, EnemyBase enemy)
    {
        MsgLog("HandleBattleTransition");
        if (!_player) _player = player;
        if (!_enemy) _enemy = enemy.gameObject;
        battleState = BattleState.START;
        SetupBattle();
    }

    private void SetupBattle()
    {
        _player = Instantiate(playerPrefab, playerSpawnPt.position, Quaternion.identity);
        _enemy = Instantiate(enemyPrefab, enemySpawnPt.position, Quaternion.identity);

        // FOR BATTLE MODE
        _player.GetComponent<NewPlayerMovement>().enabled = false;

        if (_player && _enemy)
            SetUpHealth(_player, _enemy);

        // PLAYER TURN FIRST
        PlayerTurn();
        //EnemyTurn();
    }

    private void SetUpHealth(GameObject player, GameObject enemy)
    {
        //playerHealth.maxValue = player.GetComponent<Health>().GetMaxHealth();
        //playerHealth.value = playerHealth.maxValue;
        //MsgLog("Player MaxHealth: " + player.GetComponent<Health>().GetMaxHealth());
        //enemyHealth.maxValue = enemy.GetComponent<EnemyBase>().GetMaxHealth();
        //enemyHealth.value = enemyHealth.maxValue;
        //MsgLog("Enemy MaxHealth: " + enemy.GetComponent<EnemyBase>().GetMaxHealth());
    }
    private bool CheckHealth()
    {
        //int playerCurrHealth = _player.GetComponent<Health>().GetCurrHealth();
        //int enemyCurrHealth = _enemy.GetComponent<EnemyBase>().GetCurrHealth();

        //if (playerCurrHealth <= 0)
        //{
        //    BattleLose();
        //    return false;
        //}
        //if (enemyCurrHealth <= 0)
        //{
        //    BattleWin();
        //    return false;
        //}
        return true;
    }
    private void PlayerTurn()
    {
        //battleState = BattleState.PLAYERTURN;
        //turnText.text = "PLAYER TURN";

        //// PLAYER HIT -> PARTY HIT -> PARTY HIT
        //_enemy.GetComponent<EnemyBase>().TakeDamage(10);
        //enemyHealth.value = _enemy.GetComponent<EnemyBase>().GetCurrHealth();
        //// CHECK HEALTH
        //if (CheckHealth())
        //{
        //    // ENEMY TURN
        //    //EnemyTurn();
        //    StartCoroutine(WaitTurn("Player"));
        //}
    }

    private void EnemyTurn()
    {
        //battleState = BattleState.ENEMYTURN;
        //turnText.text = "ENEMY TURN";

        //// ENEMY HIT -> PLAYER TURN
        //_enemy.GetComponent<EnemyBase>()._states = EnemyBase.EnemyStates.BattleAttack;
        //StartCoroutine(EnemyTurnCoroutine());
    }

    //private IEnumerator EnemyTurnCoroutine()
    //{
    //    while (_enemy.GetComponent<EnemyBase>()._states == EnemyBase.EnemyStates.Attack)
    //        yield return null;

    //    //playerHealth.value = _player.GetComponent<Health>().GetCurrHealth();
    //    //MsgLog("Player currHealth: " + _player.GetComponent<Health>().GetCurrHealth());
    //    if (CheckHealth())
    //        StartCoroutine(WaitTurn("Enemy"));
    //}

    private IEnumerator WaitTurn(string n)
    {
        yield return new WaitForSeconds(2);
        if (n == "Player")
            EnemyTurn();
        else
            PlayerTurn();
    }

    private void BattleWin()
    {
        //UnRegisterEnemy(_enemy.GetComponent<EnemyBase>());
        //battleState = BattleState.BATTLEWIN;
        //turnText.text = "Battle Win";
        //Time.timeScale = 0;
        //MsgLog("Player win");
        //// game scene
        //if (normalScene && battleScene)
        //{
        //    Destroy(_enemy);
        //    Destroy(_player);
        //    normalScene.SetActive(true);
        //    battleScene.SetActive(false);
        //}
    }

    private void BattleLose()
    {
        //UnRegisterEnemy(_enemy.GetComponent<EnemyBase>());
        //battleState = BattleState.BATTLELOSE;
        //turnText.text = "Battle Lose";
        //Time.timeScale = 0;
        //MsgLog("Player lost");
        // lose scene
    }

    private void MsgLog(string msg)
    {
        Debug.Log("[BattleSystem] " + msg);
    }
}
