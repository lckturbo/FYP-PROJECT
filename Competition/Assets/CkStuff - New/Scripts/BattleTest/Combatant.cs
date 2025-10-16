using System.Collections;
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

    // === Action lifecycle ===
    public event System.Action ActionBegan;
    public event System.Action ActionEnded;

    private void Awake()
    {
        if (!health) health = GetComponentInChildren<NewHealth>();
        if (!anim) anim = GetComponent<Animator>();
    }

    public float Speed => stats ? Mathf.Max(0.01f, stats.actionvaluespeed) : 0.01f;
    public bool IsAlive => health == null || health.GetCurrHealth() > 0;

    // === Approach / movement settings ===
    [Header("Approach")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float approachDistance = 0.7f;
    [SerializeField] private float goSpeed = 6f;
    [SerializeField] private float backSpeed = 8f;
    [SerializeField] private float hopArcHeight = 0.15f;

    // === Skill cooldowns ===
    [Header("Skill Cooldowns")]
    [SerializeField] public int skill1CooldownTurns = 3;
    [SerializeField] public int skill2CooldownTurns = 4;

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

    // === Public actions ===
    public void BasicAttack(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        StartCoroutine(MoveRoutine(target, DoBasicAttackDamage));
    }

    public bool TryUseSkill1(Combatant target)
    {
        if (!IsSkill1Ready || !stats || !target || !target.health) return false;
        _skill1CD = Mathf.Max(1, skill1CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill1Damage));
        return true;
    }

    public bool TryUseSkill2(Combatant target)
    {
        if (!IsSkill2Ready || !stats || !target || !target.health) return false;
        _skill2CD = Mathf.Max(1, skill2CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill2Damage));
        return true;
    }

    public void Skill1(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        StartCoroutine(MoveRoutine(target, DoSkill1Damage));
    }

    public void Skill2(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        StartCoroutine(MoveRoutine(target, DoSkill2Damage));
    }

    // === Damage payloads ===
    private void DoBasicAttackDamage(Combatant target)
    {
        if (anim) anim.SetTrigger("attack");
        target.health.TakeDamage(0, stats, NewElementType.None);
        Debug.Log($"[ACTION] {name} used BASIC ATTACK on {target.name}");
    }

    private void DoSkill1Damage(Combatant target)
    {
        if (anim) anim.SetTrigger("skill1");
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.2f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);
        Debug.Log($"[ACTION] {name} used SKILL 1 on {target.name} (raw {rawDamage})");
    }

    private void DoSkill2Damage(Combatant target)
    {
        if (anim) anim.SetTrigger("skill2");
        int rawDamage = Mathf.RoundToInt(stats.atkDmg * 1.5f);
        target.health.TakeDamage(rawDamage, stats, NewElementType.None);
        Debug.Log($"[ACTION] {name} used SKILL 2 on {target.name} (raw {rawDamage})");
    }

    // === Movement routine ===
    private IEnumerator MoveRoutine(Combatant target, System.Action<Combatant> doHit)
    {
        if (target == null || !target.IsAlive) yield break;

        // tell the TurnEngine to pause turn processing
        ActionBegan?.Invoke();

        Transform mover = visualRoot ? visualRoot : transform;

        Vector3 startPos = mover.position;
        Vector3 tgtPos = ApproachPoint(target);

        yield return SmoothMove(mover, startPos, tgtPos, goSpeed, hopArcHeight);

        doHit?.Invoke(target);

        yield return new WaitForSeconds(0.05f);

        yield return SmoothMove(mover, tgtPos, startPos, backSpeed, hopArcHeight);

        mover.position = startPos;

        // finished – TurnEngine may resume
        ActionEnded?.Invoke();
    }

    private Vector3 ApproachPoint(Combatant target)
    {
        Vector3 a = (visualRoot ? visualRoot.position : transform.position);
        Vector3 b = target.transform.position;
        Vector3 dir = (a - b).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right;
        return b + dir * approachDistance;
    }

    private IEnumerator SmoothMove(Transform mover, Vector3 from, Vector3 to, float speed, float arc)
    {
        float dist = Vector3.Distance(from, to);
        float dur = Mathf.Max(0.01f, dist / Mathf.Max(0.01f, speed));
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float u = EaseInOut(Mathf.Clamp01(t));

            Vector3 pos = Vector3.Lerp(from, to, u);

            if (arc > 0f)
            {
                // simple parabola: peak at mid-point
                float hop = arc * 4f * u * (1f - u);
                pos.y += hop;
            }

            mover.position = pos;
            yield return null;
        }
    }

    private float EaseInOut(float x)
    {
        return (x < 0.5f) ? 4f * x * x * x
                          : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }
}
