// PlayerLevelApplier.cs
using UnityEngine;

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

    private void Awake()
    {
        health = GetComponentInChildren<NewHealth>();
        movement = GetComponentInChildren<NewPlayerMovement>();
        combatant = GetComponent<Combatant>();
    }

    private void Start()
    {
        // Auto-bind to shared party system if not set
        if (levelSystem == null && PartyLevelSystem.Instance != null)
            levelSystem = PartyLevelSystem.Instance.levelSystem;

        if (levelSystem != null)
            levelSystem.OnLevelUp += HandleLevelUp;

        // initial apply at current party level
        ApplyForLevel(levelSystem != null ? levelSystem.level : 1);
    }

    private void OnDestroy()
    {
        if (levelSystem != null)
            levelSystem.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int newLevel)
    {
        ApplyForLevel(newLevel);
        Debug.Log($"{name}: leveled to {newLevel}");
    }

    public void ApplyForLevel(int level)
    {
        if (!definition || !definition.stats) return;

        runtimeStats = StatsRuntimeBuilder.BuildRuntimeStats(definition.stats, level, growth);

        health?.ApplyStats(runtimeStats);
        movement?.ApplyStats(runtimeStats);
        if (combatant) combatant.stats = runtimeStats;
    }
}
