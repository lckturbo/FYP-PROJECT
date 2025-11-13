using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NewHealth))]
public class PlayerLevelApplier : MonoBehaviour
{
    public event Action<NewCharacterStats> OnStatsUpdated;

    [Header("References")]
    public NewCharacterDefinition definition;
    public LevelSystem levelSystem;
    public LevelGrowth growth;

    [SerializeField] public NewCharacterStats runtimeStats;

    private NewHealth health;
    private NewPlayerMovement movement;
    private Combatant combatant;

    private bool _bound;

    private void Awake()
    {
        health = GetComponentInChildren<NewHealth>();
        movement = GetComponentInChildren<NewPlayerMovement>();
        combatant = GetComponent<Combatant>();
    }

    private void OnEnable()
    {
        // ALWAYS force a rebind when enabling (new scene, respawn, etc.)
        StartCoroutine(ForceRebind());
    }

    private void OnDisable()
    {
        if (_bound && levelSystem != null)
        {
            levelSystem.OnLevelUp -= HandleLevelUp;
            _bound = false;
        }
    }

    private IEnumerator ForceRebind()
    {
        // Wait until PartyLevelSystem exists (fixes allies binding too early)
        while (PartyLevelSystem.Instance == null)
            yield return null;

        // Assign shared level system
        levelSystem = PartyLevelSystem.Instance.levelSystem;

        // Clean old subscriptions to avoid double-binding
        levelSystem.OnLevelUp -= HandleLevelUp;
        levelSystem.OnLevelUp += HandleLevelUp;

        _bound = true;

        // Apply shared party level immediately when the overworld/battle loads
        ApplyForLevel(levelSystem.level);

        Debug.Log($"{name}: ForceRebind -> applied level {levelSystem.level}.");
    }

    private void HandleLevelUp(int newLevel)
    {
        ApplyForLevel(newLevel);
        Debug.Log($"{name}: LevelUp applied -> L{newLevel} HP:{runtimeStats?.maxHealth} ATK:{runtimeStats?.atkDmg}");
    }

    public void ApplyForLevel(int level)
    {
        if (definition == null || growth == null)
        {
            Debug.LogWarning($"{name}: Missing definition/growth in PlayerLevelApplier!", this);
            return;
        }

        // Build new runtime stats at the given level
        runtimeStats = StatsRuntimeBuilder.BuildRuntimeStats(definition.stats, level, growth);

        // Push stats to components that need it
        health?.ApplyStats(runtimeStats);
        movement?.ApplyStats(runtimeStats);

        // Combatant only exists in battle (null in overworld)
        if (combatant)
            combatant.stats = runtimeStats;

        // Reapply buffs if any exist
        if (BuffData.instance != null)
        {
            if (BuffData.instance.hasAttackBuff && BuffData.instance.attackTarget == runtimeStats)
                runtimeStats.atkDmg += BuffData.instance.latestAttackBuff;

            if (BuffData.instance.hasDefenseBuff && BuffData.instance.defenseTarget == runtimeStats)
                runtimeStats.attackreduction += BuffData.instance.latestDefenseBuff;
        }

        OnStatsUpdated?.Invoke(runtimeStats);
    }
}
