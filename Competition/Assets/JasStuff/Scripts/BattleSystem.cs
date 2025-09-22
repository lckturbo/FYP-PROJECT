using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    //[SerializeField] private Transform leaderSpawnPt;
    [SerializeField] private Transform[] allySpawnPt;
    [SerializeField] private Transform[] enemySpawnPt;

    [Header("UI")]
    [SerializeField] private Slider[] playerHealth;
    [SerializeField] private Slider[] enemyHealth;

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
        NewCharacterDefinition leader = PlayerParty.instance.GetLeader();
        if (fullParty == null || fullParty.Count < 1 || leader == null) return;

        // leader
        GameObject leaderObj = Instantiate(leader.playerPrefab, allySpawnPt[0].position, Quaternion.identity);
        playerLeader = leaderObj;
        leaderObj.name = "Leader_" + leader.name;
        leaderObj.GetComponent<PlayerInput>().enabled = false;
        ApplyStatsIfPresent(leaderObj, leader.stats);          // <-- ADDED THIS

        // allies
        for (int i = 0; i < fullParty.Count; i++)
        {
            NewCharacterDefinition member = fullParty[i];
            if (member == leader) continue;

            int allyIndex = playerAllies.Count + 1;
            if (allyIndex < allySpawnPt.Length)
            {
                GameObject allyObj = Instantiate(member.playerPrefab, allySpawnPt[allyIndex].position, Quaternion.identity);
                allyObj.name = "Ally_" + member.name;
                allyObj.GetComponent<PlayerInput>().enabled = false;
                ApplyStatsIfPresent(allyObj, member.stats);     // <-- ADDED THIS
                playerAllies.Add(allyObj);
            }
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

        SetUpHealth();
        // start battle
        PlayerTurn();
    }

    private void SetUpHealth()
    {
        // Players
        var allPlayers = new List<GameObject>();
        allPlayers.Add(playerLeader);
        allPlayers.AddRange(playerAllies);

        for (int i = 0; i < allPlayers.Count && i < playerHealth.Length; i++)
        {
            var h = allPlayers[i] ? allPlayers[i].GetComponent<NewHealth>() : null;
            if (h)
            {
                playerHealth[i].maxValue = h.GetMaxHealth();
                playerHealth[i].value = h.GetCurrHealth();
            }
        }

        // Enemies
        for (int i = 0; i < enemies.Count && i < enemyHealth.Length; i++)
        {
            var h = enemies[i] ? enemies[i].GetComponent<NewHealth>() : null;
            if (h)
            {
                enemyHealth[i].maxValue = h.GetMaxHealth();
                enemyHealth[i].value = h.GetCurrHealth();
            }
        }
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

    private void ApplyStatsIfPresent(GameObject go, BaseStats stats)
    {
        if (!go || stats == null) return;
        go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
        go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
    }

}
