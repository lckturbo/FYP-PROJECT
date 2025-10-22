// PlayerLevelApplier.cs  (additions marked // NEW)
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NewHealth))]
public class PlayerLevelApplier : MonoBehaviour
{
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
        // Try bind immediately; if not ready, wait a frame loop until it is.
        if (!TryBind())
            StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (_bound && levelSystem != null)
        {
            levelSystem.OnLevelUp -= HandleLevelUp;
            _bound = false;
        }
    }

    private bool TryBind()
    {
        if (levelSystem == null && PartyLevelSystem.Instance != null)
            levelSystem = PartyLevelSystem.Instance.levelSystem;

        if (levelSystem == null) return false;

        // prevent double subscribe
        if (!_bound)
        {
            levelSystem.OnLevelUp += HandleLevelUp;
            _bound = true;
            ApplyForLevel(levelSystem.level); // apply current party level now
        }
        return true;
    }

    private IEnumerator BindWhenReady()
    {
        // Wait until the PartyLevelSystem singleton exists
        while (PartyLevelSystem.Instance == null)
            yield return null;

        TryBind();
    }

    private void HandleLevelUp(int newLevel)
    {
        ApplyForLevel(newLevel);
        Debug.Log($"{name}: Level {newLevel} applied. HP {runtimeStats?.maxHealth}, ATK {runtimeStats?.atkDmg}");
    }

    public void ApplyForLevel(int level)
    {
        runtimeStats = StatsRuntimeBuilder.BuildRuntimeStats(definition.stats, level, growth);
        health?.ApplyStats(runtimeStats);
        movement?.ApplyStats(runtimeStats);
        if (combatant) combatant.stats = runtimeStats;

        // Reapply buffs from BuffData
        if (BuffData.instance != null)
        {
            if (BuffData.instance.hasAttackBuff && BuffData.instance.attackTarget == runtimeStats)
                runtimeStats.atkDmg += BuffData.instance.latestAttackBuff;
            if (BuffData.instance.hasDefenseBuff && BuffData.instance.defenseTarget == runtimeStats)
                runtimeStats.attackreduction += BuffData.instance.latestDefenseBuff;
        }
    }

}
