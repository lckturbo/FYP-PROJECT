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

    // TurnEngine pauses while an action runs
    public event System.Action ActionBegan;
    public event System.Action ActionEnded;

    private void Awake()
    {
        if (!health) health = GetComponentInChildren<NewHealth>();
        if (!anim) anim = GetComponent<Animator>();
    }

    public float Speed => stats ? Mathf.Max(0.01f, stats.actionvaluespeed) : 0.01f;
    public bool IsAlive => health == null || health.GetCurrHealth() > 0;

    // --- Approach / movement settings ---
    [Header("Approach")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float approachDistance = 0.7f;
    [SerializeField] private float goSpeed = 6f;
    [SerializeField] private float backSpeed = 8f;
    [SerializeField] private float hopArcHeight = 0.15f;

    // --- Animator state names to wait for ---
    [Header("Animator State Names")]
    [SerializeField] private string attackStateName = "attack";
    [SerializeField] private string skill1StateName = "skill1";
    [SerializeField] private string skill2StateName = "skill2";
    [SerializeField] private int animLayer = 0;

    [Header("Animation Wait Settings")]
    [SerializeField] private float animEnterTimeout = 0.5f;
    [SerializeField] private float animMaxWait = 3.0f;
    [SerializeField] private float animFallbackSeconds = 0.4f;

    // --- Skill cooldowns ---
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
        StartCoroutine(MoveRoutine(target, DoBasicAttackDamage, attackStateName));
    }

    public bool TryUseSkill1(Combatant target)
    {
        if (!IsSkill1Ready || !stats || !target || !target.health) return false;
        _skill1CD = Mathf.Max(1, skill1CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill1Damage, skill1StateName));
        return true;
    }

    public bool TryUseSkill2(Combatant target)
    {
        if (!IsSkill2Ready || !stats || !target || !target.health) return false;
        _skill2CD = Mathf.Max(1, skill2CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill2Damage, skill2StateName));
        return true;
    }

    // Kept for compatibility
    public void Skill1(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        StartCoroutine(MoveRoutine(target, DoSkill1Damage, skill1StateName));
    }

    public void Skill2(Combatant target)
    {
        if (!stats || !target || !target.health) return;
        StartCoroutine(MoveRoutine(target, DoSkill2Damage, skill2StateName));
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

    // === Movement + wait-for-animation routine ===
    private IEnumerator MoveRoutine(Combatant target, System.Action<Combatant> doHit, string stateToWait)
    {
        if (target == null || !target.IsAlive) yield break;

        ActionBegan?.Invoke();

        Transform mover = visualRoot ? visualRoot : transform;

        Vector3 startPos = mover.position;
        Vector3 tgtPos = ApproachPoint(target);

        yield return SmoothMove(mover, startPos, tgtPos, goSpeed, hopArcHeight);

        // play animation + apply damage
        doHit?.Invoke(target);

        // Let Animator process the trigger
        yield return null;

        // Wait until the attack/skill animation actually finishes (with timeouts/fallbacks)
        yield return WaitForAnimationRobust(anim, stateToWait);

        // then move back
        yield return SmoothMove(mover, tgtPos, startPos, backSpeed, hopArcHeight);

        mover.position = startPos;

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
                float hop = arc * 4f * u * (1f - u); // parabola, peak at 0.5
                pos.y += hop;
            }

            mover.position = pos;
            yield return null;
        }
    }

    // ---- ROBUST ANIM WAITER ----
    private IEnumerator WaitForAnimationRobust(Animator a, string stateName)
    {
        if (!a)
        {
            yield return new WaitForSeconds(animFallbackSeconds);
            yield break;
        }

        // 1) Try the configured layer first
        if (!string.IsNullOrEmpty(stateName))
        {
            bool entered = false;
            float t = 0f;
            while (t < animEnterTimeout)
            {
                var st = a.GetCurrentAnimatorStateInfo(animLayer);
                if (st.IsName(stateName)) { entered = true; break; }
                t += Time.deltaTime;
                yield return null;
            }

            // 2) If not entered, scan all layers quickly
            if (!entered)
            {
                int layers = a.layerCount;
                float scanT = 0f;
                while (scanT < animEnterTimeout && !entered)
                {
                    for (int L = 0; L < layers; L++)
                    {
                        var st = a.GetCurrentAnimatorStateInfo(L);
                        if (st.IsName(stateName)) { entered = true; break; }
                    }
                    scanT += Time.deltaTime;
                    yield return null;
                }
            }

            // 3) If still not found, fall back to clip time or fixed wait
            if (!entered)
            {
                float len = FirstClipLength(a);
                yield return new WaitForSeconds(len > 0f ? Mathf.Min(len, animMaxWait) : animFallbackSeconds);
                yield break;
            }

            // 4) We’re in the right state on some layer – wait until it finishes or we time out
            float waited = 0f;
            while (waited < animMaxWait)
            {
                bool anyTransition = false;
                bool finishedAll = true;

                int layers = a.layerCount;
                for (int L = 0; L < layers; L++)
                {
                    var st = a.GetCurrentAnimatorStateInfo(L);
                    if (st.IsName(stateName))
                    {
                        if (a.IsInTransition(L)) { anyTransition = true; finishedAll = false; break; }
                        if (st.loop) // looping state: consider done after one cycle
                        {
                            if (st.normalizedTime >= 1f) { /* ok */ }
                            else { finishedAll = false; break; }
                        }
                        else
                        {
                            if (st.normalizedTime < 1f) { finishedAll = false; break; }
                        }
                    }
                }

                if (finishedAll && !anyTransition) break;

                waited += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        // No state name provided: try clip length or fallback
        float fallback = FirstClipLength(a);
        yield return new WaitForSeconds(fallback > 0f ? Mathf.Min(fallback, animMaxWait) : animFallbackSeconds);
    }

    private float FirstClipLength(Animator a)
    {
        var info = a.GetCurrentAnimatorClipInfo(animLayer);
        if (info != null && info.Length > 0 && info[0].clip) return info[0].clip.length;
        return 0f;
    }

    private float EaseInOut(float x)
    {
        return (x < 0.5f) ? 4f * x * x * x
                          : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }
}
