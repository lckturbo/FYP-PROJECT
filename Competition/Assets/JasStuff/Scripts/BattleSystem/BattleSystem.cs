using Pathfinding;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    [Header("SpawnPoints")]
    [SerializeField] private Transform[] allySpawnPt;
    [SerializeField] private Transform[] enemySpawnPt;

    [Header("UI")]
    [SerializeField] private Slider[] playerHealth;
    [SerializeField] private Slider[] enemyHealth;

    private GameObject playerLeader;
    private List<GameObject> playerAllies = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    [Header("Systems")]
    [SerializeField] private TurnEngine turnEngine;

    [Header("Level Growth")]
    [SerializeField] private LevelGrowth playerGrowth;
    [SerializeField] private LevelGrowth enemyGrowth;

    public event Action<bool> OnBattleEnd;

    private void OnEnable()
    {
        if (turnEngine)
            turnEngine.OnBattleEnd += HandleBattleEnd;
    }

    private void OnDisable()
    {
        if (turnEngine)
            turnEngine.OnBattleEnd -= HandleBattleEnd;
    }

    private void Start()
    {
        SetupBattle();

        if (BattleManager.instance)
            OnBattleEnd += BattleManager.instance.HandleBattleEnd;
    }

    private void SetupBattle()
    {
        List<NewCharacterDefinition> fullParty = PlayerParty.instance.GetFullParty();
        NewCharacterDefinition leader = PlayerParty.instance.GetLeader();
        if (fullParty == null || fullParty.Count < 1 || leader == null) return;

        // Leader
        GameObject leaderObj = Instantiate(leader.playerPrefab, allySpawnPt[0].position, Quaternion.identity);
        playerLeader = leaderObj;
        leaderObj.name = "Leader_" + leader.name;
        leaderObj.GetComponent<PlayerInput>().enabled = false;
        ApplyStatsIfPresent(leaderObj, leader.stats);

        var cL = leaderObj.AddComponent<Combatant>();
        cL.isPlayerTeam = true;
        cL.isLeader = true;
        cL.stats = leader.stats;
        turnEngine.Register(cL);

        var leaderHealth = leaderObj.GetComponent<NewHealth>();
        if (leaderHealth)
        {
            leaderHealth.OnHealthChanged += (h) =>
            {
                if (h.GetCurrHealth() <= 0)
                {
                    Debug.Log("[Battle System] Leader died, ending battle");
                    HandleBattleEnd(false);
                }
            };
        }

        AddPlayerLevelApplier(leaderObj, leader);

        // Allies
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
                ApplyStatsIfPresent(allyObj, member.stats);
                playerAllies.Add(allyObj);

                var cA = allyObj.AddComponent<Combatant>();
                cA.isPlayerTeam = true;
                cA.isLeader = false;
                cA.stats = member.stats;
                turnEngine.Register(cA);

                AddPlayerLevelApplier(allyObj, member);
            }
        }

        // Enemies
        List<GameObject> spawnList = BattleManager.instance.enemypartyRef.GetEnemies();
        for (int i = 0; i < spawnList.Count; i++)
        {
            GameObject enemy = Instantiate(spawnList[i], enemySpawnPt[i].position, Quaternion.identity);
            enemy.name = "Enemy_" + i;
            this.enemies.Add(enemy);

            enemy.GetComponent<AIPath>().enabled = false;
            enemy.GetComponent<Seeker>().enabled = false;

            var eb = enemy.GetComponent<EnemyBase>();
            var es = eb ? eb.GetEnemyStats() : null;

            var eh = enemy.GetComponentInChildren<NewHealth>();
            if (eh && es) eh.ApplyStats(es);

            var cE = enemy.AddComponent<Combatant>();
            cE.isPlayerTeam = false;
            cE.isLeader = false;
            cE.stats = es;
            turnEngine.Register(cE);

            AddEnemyScaler(enemy);
        }

        SetUpHealth();
        turnEngine.Begin();
    }

    private void SetUpHealth()
    {
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

                int idx = i;
                h.OnHealthChanged += (nh) =>
                {
                    playerHealth[idx].maxValue = nh.GetMaxHealth();
                    playerHealth[idx].value = nh.GetCurrHealth();
                };
            }
        }

        for (int i = 0; i < enemies.Count && i < enemyHealth.Length; i++)
        {
            var h = enemies[i] ? enemies[i].GetComponent<NewHealth>() : null;
            if (h)
            {
                enemyHealth[i].maxValue = h.GetMaxHealth();
                enemyHealth[i].value = h.GetCurrHealth();

                int idx = i;
                h.OnHealthChanged += (nh) =>
                {
                    enemyHealth[idx].maxValue = nh.GetMaxHealth();
                    enemyHealth[idx].value = nh.GetCurrHealth();
                };
            }
        }
    }

    private void ApplyStatsIfPresent(GameObject go, NewCharacterStats stats)
    {
        if (!go || stats == null) return;
        go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
        go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
    }

    private void AddPlayerLevelApplier(GameObject go, NewCharacterDefinition def)
    {
        var applier = go.GetComponent<PlayerLevelApplier>();
        if (!applier) applier = go.AddComponent<PlayerLevelApplier>();
        applier.definition = def;
        if (applier.levelSystem == null && PartyLevelSystem.Instance != null)
            applier.levelSystem = PartyLevelSystem.Instance.levelSystem;
        if (applier.growth == null) applier.growth = playerGrowth;
    }

    private void AddEnemyScaler(GameObject enemyGO)
    {
        var scaler = enemyGO.GetComponent<EnemyScaler>();
        if (!scaler) scaler = enemyGO.AddComponent<EnemyScaler>();

        var field = typeof(EnemyScaler).GetField("enemyGrowth",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);
        if (field != null)
            field.SetValue(scaler, enemyGrowth);
    }

    private void HandleBattleEnd(bool playerWon)
    {
        if (playerWon)
        {
            int xp = CalculateXpReward();
            PartyLevelSystem.Instance?.AddXP(xp);
            Debug.Log($"[BattleEnd] Victory! Awarded {xp} XP to party.");
        }

        OnBattleEnd?.Invoke(playerWon);
    }

    private int CalculateXpReward()
    {
        int xp = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            var go = enemies[i];
            //original
            //if (!go) { xp += 20; continue; }

            //for testing
            if (!go) { xp += 120; continue; }

            var c = go.GetComponent<Combatant>();
            if (c && c.stats != null)
            {
                var cs = c.stats as NewCharacterStats;
                int lvl = (cs != null && cs.level > 0) ? cs.level : 1;
                xp += 20 + lvl * 5;
            }
            else
            {
                xp += 20;
            }
        }
        return xp;
    }
}
