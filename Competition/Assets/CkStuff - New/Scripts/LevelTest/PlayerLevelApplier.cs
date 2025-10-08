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

    [SerializeField] private NewCharacterStats runtimeStats;

    private NewHealth health;
    private NewPlayerMovement movement;
    private Combatant combatant;

    private bool _bound; // NEW

    private void Awake()
    {
        health = GetComponentInChildren<NewHealth>();
        movement = GetComponentInChildren<NewPlayerMovement>();
        combatant = GetComponent<Combatant>();
    }

    private void OnEnable() // NEW
    {
        // Try bind immediately; if not ready, wait a frame loop until it is.
        if (!TryBind())
            StartCoroutine(BindWhenReady());
    }

    private void OnDisable() // NEW
    {
        if (_bound && levelSystem != null)
        {
            levelSystem.OnLevelUp -= HandleLevelUp;
            _bound = false;
        }
    }

    private bool TryBind() // NEW
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

    private IEnumerator BindWhenReady() // NEW
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
        if (!definition || !definition.stats) return;

        //runtimeStats = StatsRuntimeBuilder.BuildRuntimeStats(definition.stats, level, growth);

        health?.ApplyStats(runtimeStats);
        movement?.ApplyStats(runtimeStats);
        if (combatant) combatant.stats = runtimeStats;
    }
}
