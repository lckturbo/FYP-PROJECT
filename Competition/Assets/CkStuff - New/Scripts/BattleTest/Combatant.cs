using UnityEngine;

[DisallowMultipleComponent]
public class Combatant : MonoBehaviour
{
    public bool isPlayerTeam;
    public bool isLeader;                 // leader = player choice (unless auto)

    public BaseStats stats;               // assigned on spawn
    public NewHealth health;              // cached

    // ATB gauge 0..1
    [HideInInspector] public float atb;

    private void Awake()
    {
        if (!health) health = GetComponentInChildren<NewHealth>();
    }

    public float Speed => stats ? Mathf.Max(0.01f, stats.actionvaluespeed) : 0.01f;
    public bool IsAlive => health == null || health.GetCurrHealth() > 0;

    // Simple “Attack”: rawDamage=0 -> NewHealth uses attacker.atkDmg
    public void BasicAttack(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        target.health.TakeDamage(0, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} attacks {target.name}");

    }
}
