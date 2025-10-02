using UnityEngine;

[DisallowMultipleComponent]
public class Combatant : MonoBehaviour
{
    public bool isPlayerTeam;
    public bool isLeader;

    public BaseStats stats;
    public NewHealth health;

    // ATB gauge 0..1
    [HideInInspector] public float atb;
    private Animator anim;

    private void Awake()
    {
        if (!health) health = GetComponentInChildren<NewHealth>();
        if(!anim) anim = GetComponent<Animator>();
    }

    public float Speed => stats ? Mathf.Max(0.01f, stats.actionvaluespeed) : 0.01f;
    public bool IsAlive => health == null || health.GetCurrHealth() > 0;

    public void BasicAttack(Combatant target)
    {
        if (!stats || !target || !target.health) return;

        if (anim) anim.SetTrigger("attack");
        target.health.TakeDamage(0, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} used BASIC ATTACK on {target.name}");
    }

    public void Skill1(Combatant target)
    {
        if (!stats || !target || !target.health) return;

        if (anim) anim.SetTrigger("skill1");
        // Example: 1.2x normal attack damage
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.2f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} used SKILL 1 on {target.name} (raw {rawDamage})");
    }

    public void Skill2(Combatant target)
    {
        if (!stats || !target || !target.health) return;

        // Example: 1.5x normal attack damage
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.5f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} used SKILL 2 on {target.name} (raw {rawDamage})");
    }
}
