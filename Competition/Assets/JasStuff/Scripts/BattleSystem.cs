using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("SpawnPoints")]
    [SerializeField] private Transform leaderSpawnPt;
    [SerializeField] private Transform[] allySpawnPt;
    [SerializeField] private Transform[] enemySpawnPt;

    //[Header("UI")]
    //[SerializeField] private Slider playerHealth;
    //[SerializeField] private Slider enemyHealth;
    //[SerializeField] private TMP_Text turnText;

    private GameObject playerLeader;
    private List<GameObject> playerAllies = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    private void Start()
    {
        SetupBattle();
    }
    private void SetupBattle()
    {
        List<NewCharacterDefinition> fullParty = PlayerParty.instance.GetFullParty();
        if (fullParty == null || fullParty.Count < 1) return;

        // spawn player "leader"
        NewCharacterDefinition leader = PlayerParty.instance.GetLeader();
        playerLeader = Instantiate(leader.playerPrefab, leaderSpawnPt.position, Quaternion.identity);
        playerLeader.name = "Leader_" + leader.name;
        playerLeader.GetComponent<PlayerInput>().enabled = false;

        // spawn player "allies"
        for (int i = 1; i < fullParty.Count && (i - 1) < allySpawnPt.Length; i++)
        {
            NewCharacterDefinition allies = fullParty[i];
            GameObject ally = Instantiate(allies.playerPrefab, allySpawnPt[i - 1].position, Quaternion.identity);
            ally.name = "Ally_" + allies.name;
            ally.GetComponent<PlayerInput>().enabled = false;
            playerAllies.Add(ally);
        }

        // spawn enemies
        List<GameObject> enemies = BattleManager.instance.enemypartyRef.GetEnemies();
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = Instantiate(enemies[i], enemySpawnPt[i].position, Quaternion.identity);
            enemy.name = "Enemy_" + i;
            this.enemies.Add(enemy);
            enemy.GetComponent<AIPath>().enabled = false;
            enemy.GetComponent<Seeker>().enabled = false;
        }

        if (playerLeader && this.enemies.Count > 0)
            SetUpHealth(playerLeader, this.enemies[0]);

        // start battle
    }

    private void SetUpHealth(GameObject player, GameObject enemy)
    {
        //playerHealth.maxValue = player.GetComponent<NewHealth>().GetMaxHealth();
        //playerHealth.value = playerHealth.maxValue;
        //enemyHealth.maxValue = enemy.GetComponent<EnemyBase>().GetMaxHealth();
        //enemyHealth.value = enemyHealth.maxValue;
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
}
