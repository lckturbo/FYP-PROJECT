using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Combatant : MonoBehaviour
{
    [Header("Skill Behavior Flags")]
    public bool skill1IsCommand = false;
    public bool Skill1IsCommand => skill1IsCommand;
    public bool blockMinigames = false;

    public bool skill2IsSupport = false;
    public bool Skill2IsSupport => skill2IsSupport;

    private enum AttackType
    {
        Basic,
        Skill1,
        Skill2,
        Support
    }

    private Combatant currentTarget;
    private AttackType currentAttackType;

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

    // --- Skill bonus suppression (for auto) ---
    private bool _suppressSkillBonusNextHit = false;
    public void SuppressSkillBonusOnce() { _suppressSkillBonusNextHit = true; }
    public void ClearSkillBonusSuppression() { _suppressSkillBonusNextHit = false; }


    // --- Physics we’ll toggle while lunging ---
    [Header("Collision Control While Acting")]
    [SerializeField] private bool disablePhysicsWhileActing = true;

    private Rigidbody2D rb;
    private Collider2D[] cols;
    private RigidbodyType2D _rbOriginalType;
    private bool[] _colOriginalIsTrigger;
    private bool _collisionDisabledActive = false;
    private BattleParodyEffect parodyEffect;

    private int turns;

    [SerializeField] private NewCharacterStats runtimeStats;

    // --- Minigame control ---
   //private bool minigameTriggered = false;
    private bool waitingForMinigameResult = false;
    private int pendingMinigameDamage = -1;

    private void Awake()
    {
        if (!health) health = GetComponentInChildren<NewHealth>();
        if (!anim) anim = GetComponent<Animator>();
        if (!parodyEffect) parodyEffect = FindObjectOfType<BattleParodyEffect>();

        rb = GetComponent<Rigidbody2D>();
        cols = GetComponentsInChildren<Collider2D>(includeInactive: true);
        if (cols == null) cols = System.Array.Empty<Collider2D>();
        _colOriginalIsTrigger = new bool[cols.Length];
        for (int i = 0; i < cols.Length; i++) _colOriginalIsTrigger[i] = cols[i].isTrigger;
        if (rb) _rbOriginalType = rb.bodyType;

        runtimeStats = GetComponent<PlayerLevelApplier>()?.runtimeStats;
    }

    public float Speed => stats ? Mathf.Max(0.01f, stats.actionvaluespeed) : 0.01f;
    public bool IsAlive => health == null || health.GetCurrHealth() > 0;

    // --- Approach / movement settings ---
    [Header("Approach")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float approachDistance = 1.0f;
    [SerializeField] private float goSpeed = 12f;
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

        turns++;
        Debug.Log("turns: " + turns);
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

        if (skill1IsCommand)
        {
            StartCoroutine(CommandSkill1Routine(target));
            return true;
        }
        _skill1CD = Mathf.Max(1, skill1CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill1Damage, skill1StateName));
        return true;
    }

    public bool TryUseSkill2(Combatant target)
    {
        if (!IsSkill2Ready || !stats) return false;

        if (skill2IsSupport)
        {
            // Support skill: no movement, just animation + buff.
            _skill2CD = Mathf.Max(1, skill2CooldownTurns);
            StartCoroutine(SupportSkill2Routine(target));
            return true;
        }

        // Normal offensive Skill 2 (existing behaviour)
        if (target == null || !target.health) return false;
        _skill2CD = Mathf.Max(1, skill2CooldownTurns);
        StartCoroutine(MoveRoutine(target, DoSkill2Damage, skill2StateName));
        return true;
    }

    //private void DoSkill2Support(Combatant target)
    //{
    //    currentTarget = target;
    //    currentAttackType = AttackType.Support;

    //    if (anim) anim.SetTrigger("skill2");

    //    ApplyCameramanCritBuff(target);
    //}


    [Header("Support Skill2 Settings (Cameraman)")]
    [SerializeField] private float skill2CritMultiplier = 2f; // 100% increase
    private bool skill2BuffAppliedOnce = false;               // simple guard vs infinite stacking

    private void ApplyCameramanCritBuff(Combatant recipient)
    {
        if (recipient == null || recipient.stats == null) return;

        // Simple: one-time permanent buff for this battle.
        if (skill2BuffAppliedOnce && recipient == this)
            return;

        var ncs = recipient.stats as NewCharacterStats;
        if (ncs != null)
        {
            ncs.critRate *= skill2CritMultiplier;
        }
        // (If your stats type is different, adjust accordingly.)

        if (recipient == this)
            skill2BuffAppliedOnce = true;

        // Extra turn for the caster: instantly fill their ATB.
        atb = 1f;

        Debug.Log($"{name} used Support Skill2 on {recipient.name}: Crit Rate x{skill2CritMultiplier}, extra turn granted.");
    }

    private IEnumerator SupportSkill2Routine(Combatant rawTarget)
    {
        ActionBegan?.Invoke();

        // validate target or fallback to self
        var target = rawTarget;
        if (target == null || !target.IsAlive || target.isPlayerTeam != this.isPlayerTeam)
            target = this;

        // Mark as a support action (no damage)
        currentTarget = target;
        currentAttackType = AttackType.Support;

        // Play support anim IN PLACE (no MoveRoutine)
        if (anim && !string.IsNullOrEmpty(skill2StateName))
            anim.SetTrigger("skill2");

        // let Animator process trigger 1 frame
        yield return null;

        // optional: wait for the support animation state robustly
        if (!string.IsNullOrEmpty(skill2StateName))
            yield return WaitForAnimationRobust(anim, skill2StateName);

        // Apply the buff + extra turn (your Cameraman logic)
        ApplyCameramanCritBuff(target);

        ActionEnded?.Invoke();
    }
    private IEnumerator CommandSkill1Routine(Combatant target)
    {
        ActionBegan?.Invoke();

        currentTarget = target;
        currentAttackType = AttackType.Skill1;

        //Transform mover = visualRoot ? visualRoot : transform;
        //Vector3 startPos = mover.position;
        //Vector3 backPos = startPos - transform.right * 1.0f;

        // Move backward before animation
        //yield return SmoothMove(mover, startPos, backPos, backSpeed, 0f);

        // Play skill animation
        if (anim) anim.SetTrigger("skill1");
        yield return WaitForAnimationRobust(anim, skill1StateName);

        // Gather enemy list
        var allCombatants = FindObjectsOfType<Combatant>();
        List<Combatant> livingEnemies = new List<Combatant>();

        foreach (var c in allCombatants)
        {
            if (c.isPlayerTeam == this.isPlayerTeam) continue;
            if (!c.IsAlive) continue;
            livingEnemies.Add(c);
        }

        int attacks = 0;

        // Allies attack with random targets
        foreach (var ally in allCombatants)
        {
            if (ally == this) continue;
            if (ally.isPlayerTeam != this.isPlayerTeam) continue;
            if (!ally.IsAlive) continue;

            if (attacks >= 2) break;

            ally.blockMinigames = true;

            if (livingEnemies.Count > 0)
            {
                Combatant randomTarget = livingEnemies[Random.Range(0, livingEnemies.Count)];

                yield return ally.BasicAttackRoutine(randomTarget);
                attacks++;
            }
        }

        // Return to starting position
        //yield return SmoothMove(mover, backPos, startPos, goSpeed, 0f);
        //mover.position = startPos;

        ActionEnded?.Invoke();
    }

    public IEnumerator BasicAttackRoutine(Combatant target)
    {
        if (!stats || !target || !target.health) yield break;
        yield return MoveRoutine(target, DoBasicAttackDamage, attackStateName);
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
        currentTarget = target;
        currentAttackType = AttackType.Basic;
        if (anim) anim.SetTrigger("attack");
    }

    private void DoSkill1Damage(Combatant target)
    {
        currentTarget = target;
        currentAttackType = AttackType.Skill1;

        if (anim) anim.SetTrigger("skill1");
    }

    private void DoSkill2Damage(Combatant target)
    {
        currentTarget = target;
        currentAttackType = AttackType.Skill2;

        if (anim)
        {
            if (turns >= 3 && HasAnimatorParameter("skill3", AnimatorControllerParameterType.Trigger))
                anim.SetTrigger("skill3");
            else
                anim.SetTrigger("skill2");
        }
    }

    public void OnZoom()
    {
        if (parodyEffect != null)
            parodyEffect.PlayParody(BattleParodyEffect.ParodyType.ZoomEffect, this, currentTarget);
    }

    public void DealDamage()
    {
        if (currentTarget == null) return;

        if (currentAttackType == AttackType.Support)
            return;

        if (waitingForMinigameResult)
        {
            Debug.Log("Waiting for minigame result — skipping base damage.");
            return;
        }

        if (pendingMinigameDamage > 0)
        {
            currentTarget.health.TakeDamage(pendingMinigameDamage, stats, NewElementType.None);
            pendingMinigameDamage = -1;
            return;
        }

        float baseMultiplier = 1f;
        switch (currentAttackType)
        {
            case AttackType.Basic: baseMultiplier = 1.0f; break;
            case AttackType.Skill1: baseMultiplier = 1.2f; break;
            case AttackType.Skill2: baseMultiplier = 1.5f; break;
        }

        if (_suppressSkillBonusNextHit &&
            (currentAttackType == AttackType.Skill1 || currentAttackType == AttackType.Skill2))
        {
            baseMultiplier = 1.0f;
        }

        int damage = CalculateDamage(currentTarget, baseMultiplier);

        _suppressSkillBonusNextHit = false;

        currentTarget.health.TakeDamage(damage, stats, NewElementType.None);
    }


    private bool HasAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (!anim) return false;
        foreach (var param in anim.parameters)
            if (param.type == type && param.name == name)
                return true;
        return false;
    }

    // === Movement + wait-for-animation routine ===
    private IEnumerator MoveRoutine(Combatant target, System.Action<Combatant> doHit, string stateToWait)
    {
        if (target == null || !target.IsAlive) yield break;

        ActionBegan?.Invoke();

        // Disable collisions for the duration of the lunge (and return)
        if (disablePhysicsWhileActing) EnterNonColliding();

        Transform mover = visualRoot ? visualRoot : transform;
        Vector3 startPos = mover.position;
        Vector3 tgtPos = ApproachPoint(target);

        yield return SmoothMove(mover, startPos, tgtPos, goSpeed, hopArcHeight);

        // play animation + apply damage
        doHit?.Invoke(target);
        yield return null; // let Animator process trigger

        // wait until the specific state finishes (with timeouts/fallbacks)
        yield return WaitForAnimationRobust(anim, stateToWait);

        // then move back
        yield return SmoothMove(mover, tgtPos, startPos, backSpeed, hopArcHeight);
        mover.position = startPos;

        if (disablePhysicsWhileActing) ExitNonColliding();

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
                float hop = arc * 4f * u * (1f - u);
                pos.y += hop;
            }

            mover.position = pos;
            yield return null;
        }
    }

    // ---- ANIM WAITER ----
    private IEnumerator WaitForAnimationRobust(Animator a, string stateName)
    {
        if (!a)
        {
            yield return new WaitForSeconds(animFallbackSeconds);
            yield break;
        }

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

            if (!entered)
            {
                float len = FirstClipLength(a);
                yield return new WaitForSeconds(len > 0f ? Mathf.Min(len, animMaxWait) : animFallbackSeconds);
                yield break;
            }

            float waited = 0f;
            while (waited < animMaxWait)
            {
                bool finishedAll = true;
                int layers = a.layerCount;
                for (int L = 0; L < layers; L++)
                {
                    var st = a.GetCurrentAnimatorStateInfo(L);
                    if (st.IsName(stateName))
                    {
                        if (a.IsInTransition(L)) { finishedAll = false; break; }
                        if (st.loop)
                        {
                            if (st.normalizedTime < 1f) { finishedAll = false; break; }
                        }
                        else
                        {
                            if (st.normalizedTime < 1f) { finishedAll = false; break; }
                        }
                    }
                }

                if (finishedAll) break;
                waited += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

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

    // ===== Collision toggling =====
    private void EnterNonColliding()
    {
        if (_collisionDisabledActive) return;
        _collisionDisabledActive = true;

        if (rb)
        {
            _rbOriginalType = rb.bodyType;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i]) continue;
            _colOriginalIsTrigger[i] = cols[i].isTrigger;
            cols[i].isTrigger = true;
        }
    }

    private void ExitNonColliding()
    {
        if (!_collisionDisabledActive) return;
        _collisionDisabledActive = false;

        if (rb)
            rb.bodyType = _rbOriginalType;

        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i]) continue;
            cols[i].isTrigger = _colOriginalIsTrigger[i];
        }
    }

    public void OnMinigameResult(MinigameManager.ResultType result, string id)
    {
        waitingForMinigameResult = false;

        int dmg = 0;
        switch (result)
        {
            case MinigameManager.ResultType.Fail:
                if (id == "TakeABreak")
                    dmg = 1;
                else
                    dmg = Mathf.RoundToInt(stats.atkDmg);
                break;

            case MinigameManager.ResultType.Success:
                dmg = Mathf.RoundToInt(stats.atkDmg * 1.2f);
                break;

            case MinigameManager.ResultType.Perfect:
                if (id == "TakeABreak")
                    dmg = currentTarget?.health.GetCurrHealth() ?? 0;
                else
                    dmg = Mathf.RoundToInt(stats.atkDmg * 1.5f);
                break;
        }

        pendingMinigameDamage = dmg;
    }

    private int CalculateDamage(Combatant target, float multiplierOverride = 1f)
    {
        if (target == null || target.health == null) return 0;

        float attackPower = stats ? stats.atkDmg : 0f;
        float defensePower = target.stats ? target.stats.attackreduction : 0f;

        // Apply buffs (if you have BuffData, keep this logic)
        PlayerBuffHandler attackerBuffHandler = GetComponent<PlayerBuffHandler>();
        PlayerBuffHandler targetBuffHandler = target.GetComponent<PlayerBuffHandler>();

        if (BuffData.instance != null)
        {
            if (BuffData.instance.hasAttackBuff && attackerBuffHandler != null &&
                BuffData.instance.attackTarget == attackerBuffHandler.levelApplier.runtimeStats)
                attackPower += BuffData.instance.latestAttackBuff;

            if (BuffData.instance.hasDefenseBuff && targetBuffHandler != null &&
                BuffData.instance.defenseTarget == targetBuffHandler.levelApplier.runtimeStats)
                defensePower += BuffData.instance.latestDefenseBuff;
        }

        float rawDamage = (attackPower * multiplierOverride) - defensePower;
        int finalDamage = Mathf.Max(Mathf.RoundToInt(rawDamage), 1);
        return finalDamage;
    }
}
