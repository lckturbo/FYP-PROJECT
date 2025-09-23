using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
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

    [Header("TESTING")]
    [SerializeField] private TurnEngine turnEngine;

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

        //TESTING
        ApplyStatsIfPresent(leaderObj, leader.stats);
        var cL = leaderObj.AddComponent<Combatant>();
        cL.isPlayerTeam = true;
        cL.isLeader = true;
        cL.stats = leader.stats;
        turnEngine.Register(cL);

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

                //TESTING
                ApplyStatsIfPresent(allyObj, member.stats);
                var cA = allyObj.AddComponent<Combatant>();
                cA.isPlayerTeam = true;
                cA.isLeader = false;
                cA.stats = member.stats;
                turnEngine.Register(cA);
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

            //TESTING
            var cE = enemy.AddComponent<Combatant>();
            cE.isPlayerTeam = false;
            cE.isLeader = false;
            turnEngine.Register(cE);
        }

        SetUpHealth();

        //TESTING
        turnEngine.Begin();
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

    private void ApplyStatsIfPresent(GameObject go, BaseStats stats)
    {
        if (!go || stats == null) return;
        go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
        go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
    }

}
