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

    [Header("Skill Cooldowns (turns)")]
    [SerializeField] public int skill1CooldownTurns = 2;
    [SerializeField] public int skill2CooldownTurns = 3;

    private int _skill1CD;
    private int _skill2CD;

    public bool IsSkill1Ready => _skill1CD <= 0;
    public bool IsSkill2Ready => _skill2CD <= 0;
    public int Skill1Remaining => Mathf.Max(0, _skill1CD);
    public int Skill2Remaining => Mathf.Max(0, _skill2CD);

    public void OnTurnStarted()
    {
        if (_skill1CD > 0) _skill1CD--;
        if (_skill2CD > 0) _skill2CD--;
    }

    public bool TryUseSkill1(Combatant target)
    {
        if (!IsSkill1Ready) return false;
        Skill1(target);
        _skill1CD = Mathf.Max(1, skill1CooldownTurns);
        return true;
    }

    public bool TryUseSkill2(Combatant target)
    {
        if (!IsSkill2Ready) return false;
        Skill2(target);
        _skill2CD = Mathf.Max(1, skill2CooldownTurns);
        return true;
    }

    public void Skill1(Combatant target)
    {
        if (!stats || !target || !target.health) return;

        if (anim) anim.SetTrigger("skill1");
        //1.2x normal attack damage
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.2f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} used SKILL 1 on {target.name} (raw {rawDamage})");
    }

    public void Skill2(Combatant target)
    {
        if (!stats || !target || !target.health) return;

        if (anim) anim.SetTrigger("skill2");
        //1.5x normal attack damage
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.5f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);

        Debug.Log($"[ACTION] {name} used SKILL 2 on {target.name} (raw {rawDamage})");
    }
}
