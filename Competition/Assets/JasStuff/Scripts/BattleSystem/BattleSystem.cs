using Pathfinding;
using System;
using System.Collections.Generic;
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

    [SerializeField] private Slider[] playerATB;
    [SerializeField] private Slider[] enemyATB;

    private GameObject playerLeader;
    private List<GameObject> playerAllies = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    [Header("Systems")]
    [SerializeField] private TurnEngine turnEngine;

    [Header("Level Growth")]
    [SerializeField] private LevelGrowth playerGrowth;
    [SerializeField] private LevelGrowth enemyGrowth;

    public event Action<bool> OnBattleEnd;

    [Header("Results UI")]
    [SerializeField] private BattleResultsUI resultsUI;

    private int _preBattlePartyLevel;
    private List<PreBattleSnapshot> _snapshots = new();
    private bool _ended = false;

    private readonly List<Combatant> _playerCombatants = new List<Combatant>();
    private readonly List<Combatant> _enemyCombatants = new List<Combatant>();

    private struct PreBattleSnapshot
    {
        public NewCharacterDefinition def;
        public NewCharacterStats statsAtLevel;
    }

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

        _ended = false;
        _preBattlePartyLevel = (PartyLevelSystem.Instance != null) ? PartyLevelSystem.Instance.levelSystem.level : 1;
        _snapshots.Clear();
        _playerCombatants.Clear();
        _enemyCombatants.Clear();

        // ===== LEADER =====
        GameObject leaderObj = Instantiate(leader.playerPrefab, allySpawnPt[0].position, Quaternion.identity);
        playerLeader = leaderObj;
        leaderObj.name = "Leader_" + leader.name;
        var leaderPI = leaderObj.GetComponent<PlayerInput>();
        if (leaderPI) leaderPI.enabled = false;
        SetAnimatorBattleLayer(leaderObj);

        // Build runtime stats for leader at party level, apply to Health/Movement & Combatant
        var leaderRT = StatsRuntimeBuilder.BuildRuntimeStats(leader.stats, _preBattlePartyLevel, playerGrowth);
        ApplyRuntimeStats(leaderObj, leaderRT); // sets NewHealth & movement
        var cL = leaderObj.AddComponent<Combatant>();
        cL.isPlayerTeam = true;
        cL.isLeader = true;
        cL.stats = leaderRT; // IMPORTANT: Combatant uses leveled stats for ATB/attack
        turnEngine.Register(cL);
        _playerCombatants.Add(cL);

        var leaderHealth = leaderObj.GetComponent<NewHealth>();
        if (leaderHealth)
        {
            leaderHealth.OnDeathComplete += (h) =>
            {
                if (h.GetCurrHealth() <= 0)
                {
                    Debug.Log("[Battle System] Leader died — force end battle.");
                    turnEngine?.ForceEnd(false);
                }
            };
        }

        AddPlayerLevelApplier(leaderObj, leader);
        SnapshotChar(leader);

        // ===== ALLIES =====
        for (int i = 0; i < fullParty.Count; i++)
        {
            var member = fullParty[i];
            if (member == leader) continue;

            int allyIndex = playerAllies.Count + 1;
            if (allyIndex < allySpawnPt.Length)
            {
                GameObject allyObj = Instantiate(member.playerPrefab, allySpawnPt[allyIndex].position, Quaternion.identity);
                allyObj.name = "Ally_" + member.name;
                var allyPI = allyObj.GetComponent<PlayerInput>();
                if (allyPI) allyPI.enabled = false;
                SetAnimatorBattleLayer(allyObj);
                playerAllies.Add(allyObj);

                var allyRT = StatsRuntimeBuilder.BuildRuntimeStats(member.stats, _preBattlePartyLevel, playerGrowth);
                ApplyRuntimeStats(allyObj, allyRT);
                var cA = allyObj.AddComponent<Combatant>();
                cA.isPlayerTeam = true;
                cA.isLeader = false;
                cA.stats = allyRT; // IMPORTANT
                turnEngine.Register(cA);
                _playerCombatants.Add(cA);

                AddPlayerLevelApplier(allyObj, member);
                SnapshotChar(member);
            }
        }

        // ===== ENEMIES =====
        List<GameObject> spawnList = BattleManager.instance.enemypartyRef.GetEnemies();
        for (int i = 0; i < spawnList.Count; i++)
        {
            GameObject enemy = Instantiate(spawnList[i], enemySpawnPt[i].position, Quaternion.identity);
            enemy.name = "Enemy_" + i;
            enemies.Add(enemy);

            SetAnimatorBattleLayer(enemy);

            var aip = enemy.GetComponent<AIPath>();
            if (aip) aip.enabled = false;
            var seeker = enemy.GetComponent<Seeker>();
            if (seeker) seeker.enabled = false;

            var eb = enemy.GetComponent<EnemyBase>();

            // Get whatever the enemy uses (EnemyStats/BaseStats)
            BaseStats baseEnemyStats = eb ? eb.GetEnemyStats() : null;

            // Build leveled runtime stats from BaseStats via the generic overload
            BaseStats enemyRT = null;
            if (baseEnemyStats != null)
                enemyRT = StatsRuntimeBuilder.BuildRuntimeStats(baseEnemyStats, _preBattlePartyLevel, enemyGrowth);

            var eh = enemy.GetComponentInChildren<NewHealth>();
            if (eh && enemyRT != null) eh.ApplyStats(enemyRT);

            var cE = enemy.AddComponent<Combatant>();
            cE.isPlayerTeam = false;
            cE.isLeader = false;
            cE.stats = enemyRT ?? baseEnemyStats; // use leveled runtime if available, else fallback
            turnEngine.Register(cE);
            _enemyCombatants.Add(cE);

            AddEnemyScaler(enemy); // keeps them synced if party levels mid-battle, optional
        }

        SetUpHealth();
        SetUpATBBars();
        turnEngine.Begin();
    }

    private void SnapshotChar(NewCharacterDefinition def)
    {
        if (!def || !def.stats) return;
        var rt = StatsRuntimeBuilder.BuildRuntimeStats(def.stats, _preBattlePartyLevel, playerGrowth);
        _snapshots.Add(new PreBattleSnapshot { def = def, statsAtLevel = rt });
    }

    private void SetUpHealth()
    {
        var allPlayers = new List<GameObject>();
        allPlayers.Add(playerLeader);
        allPlayers.AddRange(playerAllies);

        // ===== Player Health Setup =====
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

                //  Disable health/ATB when dead
                h.OnDeathComplete += (nh) =>
                {
                    playerHealth[idx].gameObject.SetActive(false);
                    if (idx < playerATB.Length && playerATB[idx])
                        playerATB[idx].gameObject.SetActive(false);
                };
            }
        }

        // ===== Enemy Health Setup =====
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

                //  Disable health/ATB when enemy dies
                h.OnDeathComplete += (nh) =>
                {
                    enemyHealth[idx].gameObject.SetActive(false);
                    if (idx < enemyATB.Length && enemyATB[idx])
                        enemyATB[idx].gameObject.SetActive(false);
                };
            }
        }
    }


    private void SetUpATBBars()
    {
        for (int i = 0; i < playerATB.Length; i++)
        {
            if (!playerATB[i]) continue;
            playerATB[i].minValue = 0f;
            playerATB[i].maxValue = 1f;
            playerATB[i].value = 0f;
            playerATB[i].interactable = false;
        }
        for (int i = 0; i < enemyATB.Length; i++)
        {
            if (!enemyATB[i]) continue;
            enemyATB[i].minValue = 0f;
            enemyATB[i].maxValue = 1f;
            enemyATB[i].value = 0f;
            enemyATB[i].interactable = false;
        }
    }

    // apply a (leveled) runtime stats asset to health & movement
    private void ApplyRuntimeStats(GameObject go, NewCharacterStats rt)
    {
        if (!go || rt == null) return;
        go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(rt);
        go.GetComponentInChildren<NewHealth>()?.ApplyStats(rt);
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

    private void SetAnimatorBattleLayer(GameObject go)
    {
        var anim = go.GetComponent<Animator>();
        if (anim && anim.layerCount > 1)
        {
            anim.SetLayerWeight(0, 0f);
            anim.SetLayerWeight(1, 1f);
        }
    }

    // ========= UPDATED: now provides XP fields for results UI =========
    private void HandleBattleEnd(bool playerWon)
    {
        if (_ended) return;
        _ended = true;

        // Access level system
        var ls = (PartyLevelSystem.Instance != null) ? PartyLevelSystem.Instance.levelSystem : null;

        // BEFORE awarding
        int preLevel = (ls != null) ? ls.level : _preBattlePartyLevel;
        int xpBeforeIntoLevel = (ls != null) ? ls.currXP : 0;

        int xpAwarded = 0;
        if (playerWon)
        {
            xpAwarded = CalculateXpReward();
            PartyLevelSystem.Instance?.AddXP(xpAwarded);
        }

        // AFTER awarding
        int postLevel = (ls != null) ? ls.level : preLevel;
        int xpAfterIntoLevel = (ls != null) ? ls.currXP : xpBeforeIntoLevel;
        int xpRequiredForNext = (ls != null) ? ls.xpNextLevel : 0;

        var payload = BuildResultsPayload(
            playerWon,
            xpAwarded,
            preLevel,
            postLevel,
            xpBeforeIntoLevel,
            xpAfterIntoLevel,
            xpRequiredForNext
        );

        if (!playerWon)
        {
            var leader = playerLeader?.GetComponent<NewHealth>();
            if (leader)
            {
                leader.OnDeathComplete += (deadChar) =>
                {
                    resultsUI?.Show(payload, () => { OnBattleEnd?.Invoke(playerWon); });
                };
                return;
            }
        }

        resultsUI?.Show(payload, () => { OnBattleEnd?.Invoke(playerWon); });
    }

    // ========= UPDATED: signature + fills XP bar fields =========
    private BattleResultsPayload BuildResultsPayload(
        bool playerWon,
        int xp,
        int preLevel,
        int postLevel,
        int xpBeforeIntoLevel,
        int xpAfterIntoLevel,
        int xpRequiredForNext
    )
    {
        var leaderDef = PlayerParty.instance.GetLeader();
        var baseStats = leaderDef ? leaderDef.stats : null;

        var pre = (baseStats != null) ? StatsRuntimeBuilder.BuildRuntimeStats(baseStats, preLevel, playerGrowth) : null;
        var post = (baseStats != null) ? StatsRuntimeBuilder.BuildRuntimeStats(baseStats, postLevel, playerGrowth) : null;

        var stats = new List<BattleResultsStat>();

        if (pre != null && post != null)
        {
            stats.Add(MakeStat("HP", pre.maxHealth, post.maxHealth, false, ""));
            stats.Add(MakeStat("Attack", pre.atkDmg, post.atkDmg, false, ""));
            stats.Add(MakeStat("Defense", pre.attackreduction, post.attackreduction, false, ""));
            stats.Add(MakeStat("Action Speed", pre.actionvaluespeed, post.actionvaluespeed, false, ""));
            stats.Add(MakeStat("Crit Rate", pre.critRate, post.critRate, true, ""));
            stats.Add(MakeStat("Crit Damage", pre.critDamage, post.critDamage, false, "×"));
        }

        return new BattleResultsPayload
        {
            playerWon = playerWon,
            xpGained = xp,

            oldLevel = preLevel,
            newLevel = postLevel,

            // >>> New fields for XP bar UI <<<
            xpBefore = xpBeforeIntoLevel,          // XP inside preLevel before battle
            xpAfter = xpAfterIntoLevel,            // XP inside postLevel after battle
            xpRequiredForNext = xpRequiredForNext, // postLevel -> next threshold

            enemyScaledToLevel = (postLevel > preLevel) ? postLevel : 0,
            stats = stats
        };
    }

    private BattleResultsStat MakeStat(string name, float oldVal, float newVal, bool asPercent, string prefix)
    {
        string OldFmt(float v) => asPercent ? $"{Mathf.RoundToInt(v * 100f)}%" : $"{prefix}{v:0.##}";
        string NewFmt(float v) => asPercent ? $"{Mathf.RoundToInt(v * 100f)}%" : $"{prefix}{v:0.##}";

        return new BattleResultsStat
        {
            label = name,
            oldValue = oldVal,
            newValue = newVal,
            oldValueText = OldFmt(oldVal),
            newValueText = NewFmt(newVal)
        };
    }

    private int CalculateXpReward()
    {
        int xp = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            var go = enemies[i];
            if (!go) { xp += 35; continue; }

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

    private void LateUpdate()
    {
        for (int i = 0; i < _playerCombatants.Count && i < playerATB.Length; i++)
        {
            var c = _playerCombatants[i];
            if (!c || playerATB[i] == null) continue;
            playerATB[i].value = Mathf.Clamp01(c.atb);
        }

        for (int i = 0; i < _enemyCombatants.Count && i < enemyATB.Length; i++)
        {
            var c = _enemyCombatants[i];
            if (!c || enemyATB[i] == null) continue;
            enemyATB[i].value = Mathf.Clamp01(c.atb);
        }
    }
}
