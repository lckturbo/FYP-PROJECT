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
    public bool isProducer;

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


    // --- Physics we�ll toggle while lunging ---
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
    [SerializeField] private float approachDistance = 1.5f;
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
        //if (!isPlayerTeam && BattleManager.instance.IsBossBattle)
        //{
        //    DoBasicAttackDamage(target);
        //    return;
        //}
        currentAttackType = AttackType.Basic;
        StartCoroutine(MoveRoutine(target, DoBasicAttackDamage, attackStateName));
    }

    public bool TryUseSkill1(Combatant target)
    {
        if (!IsSkill1Ready || !stats) return false;

        if (skill1IsCommand)
        {
            StartCoroutine(CommandSkill1Routine(target));
            return true;
        }

        if (!target || !target.health) return false;

        _skill1CD = Mathf.Max(1, skill1CooldownTurns);
        currentAttackType = AttackType.Skill1;
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
        currentAttackType = AttackType.Skill2;
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

        // NEW: Trigger visual buff on the recipient
        var buffHandler = recipient.GetComponent<PlayerBuffHandler>();
        if (buffHandler != null)
            buffHandler.ApplySkill2Buff();

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

        if (anim) anim.SetTrigger("skill1");
        yield return WaitForAnimationRobust(anim, skill1StateName);

        var allCombatants = FindObjectsOfType<Combatant>();
        List<Combatant> livingEnemies = new List<Combatant>();

        foreach (var c in allCombatants)
        {
            if (!c.IsAlive) continue;
            if (c.isPlayerTeam == this.isPlayerTeam) continue;
            livingEnemies.Add(c);
        }

        int attacks = 0;
        int active = 0;
        List<Coroutine> runningCoroutines = new List<Coroutine>();

        foreach (var ally in allCombatants)
        {
            if (attacks >= 2)
            {
                var handler = ally.GetComponent<PlayerBuffHandler>();
                if (handler != null)
                {
                    handler.RemoveStoredBuffs();
                    Debug.LogWarning($"Buff removed from {ally.name}");
                }
                break;
            }
            else
            {
                var handler = ally.GetComponent<PlayerBuffHandler>();
                if (handler != null)
                {
                    handler.RemoveAttackBuff();
                    Debug.LogWarning($"Buff removed from {ally.name}");
                }
            }
            if (ally == this) continue;
            if (!ally.IsAlive) continue;
            if (ally.isPlayerTeam != this.isPlayerTeam) continue;

            ally.blockMinigames = true;

            if (livingEnemies.Count > 0)
            {
                int index = Random.Range(0, livingEnemies.Count);
                Combatant randomTarget = livingEnemies[index];

                if (livingEnemies.Count > 1)
                    livingEnemies.RemoveAt(index);

                // EXTRA DMG
                int bonusDamage = 3;
                ally.stats.atkDmg += bonusDamage;
                active++;

                StartCoroutine(BasicAttackAndCount());

                IEnumerator BasicAttackAndCount()
                {
                    yield return ally.BasicAttackRoutine(randomTarget);
                    ally.stats.atkDmg -= bonusDamage;
                    active--;
                }
                attacks++;
            }
        }

        while (active > 0)
            yield return null;
        //while (runningCoroutines.Exists(c => c != null))
        //    yield return null;

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

    // BATTLE CAMERA / PARODY STUFF //
    // private bool zoomFinished = false;

    public void OnZoomIn()
    {
        if (parodyEffect != null)
        {
            if (BattleUIManager.instance)
                BattleUIManager.instance.HideAllUI();
            parodyEffect.ZoomIn(this);
        }
    }
    public void OnZoomOut()
    {
        if (parodyEffect != null)
        {
            if (BattleUIManager.instance)
                BattleUIManager.instance.ShowAllUI();
            parodyEffect.ZoomOut();
        }
    }
    public void OnCamShake()
    {
        if (parodyEffect != null)
            parodyEffect.ShakeScreen();
    }

    public bool isMultiHit = false;
    public int multiHitTotal = 5;
    public int multiHitIndex = 0;
    public void DealDamage()
    {
        if (currentTarget == null) return;

        if (currentAttackType == AttackType.Support || waitingForMinigameResult)
            return;

        // Skill2 buff removal
        var buff = GetComponent<PlayerBuffHandler>();
        if (buff != null && buff.IsSkill2BuffActive)
        {
            buff.RemoveSkill2Buff();
            Debug.Log("[Skill2] Cameraman crit buff removed after next action.");
        }

        bool wasSuppressed = _suppressSkillBonusNextHit;
        _suppressSkillBonusNextHit = false;

        // MINIGAME OVERRIDE
        if (pendingMinigameDamage > 0)
        {
            currentTarget.health.TakeDamage(pendingMinigameDamage, stats, NewElementType.None);
            pendingMinigameDamage = -1;
            return;
        }

        // --- Base multiplier ---
        float baseMultiplier = 1f;
        switch (currentAttackType)
        {
            case AttackType.Basic:
                if (!BattleManager.instance.IsBossBattle && !isPlayerTeam)
                    isMultiHit = true;
                baseMultiplier = 1.0f;
                break;

            case AttackType.Skill1:
                baseMultiplier = 1.2f;
                break;

            case AttackType.Skill2:
                baseMultiplier = 1.5f;
                break;
        }

        // Suppression removes bonus multiplier
        if (wasSuppressed &&
            (currentAttackType == AttackType.Skill1 || currentAttackType == AttackType.Skill2))
        {
            baseMultiplier = 1.0f;
        }

        // === Actual Damage Calculation ===
        int fullDamage = CalculateDamage(currentTarget, baseMultiplier);
        int damageToApply = fullDamage;

        // === Multi-hit system ===
        if (isMultiHit)
        {
            int perHit = fullDamage / multiHitTotal;

            if (multiHitIndex == multiHitTotal - 1)
                perHit = fullDamage - perHit * (multiHitTotal - 1);

            damageToApply = perHit;

            multiHitIndex++;

            if (multiHitIndex >= multiHitTotal)
            {
                isMultiHit = false;
                multiHitIndex = 0;
            }
        }

        // === BOSS attacks all ===
        if (!isPlayerTeam && BattleManager.instance.IsBossBattle)
        {
            Debug.Log("[BOSS] Attacks ALL player characters!");

            Combatant[] all = FindObjectsOfType<Combatant>();
            foreach (Combatant c in all)
            {
                if (c.isPlayerTeam && c.IsAlive)
                {
                    c.health.TakeDamage(damageToApply, stats, NewElementType.None);
                }
            }
            return;
        }

        // === Normal damage ===
        currentTarget.health.TakeDamage(damageToApply, stats, NewElementType.None);
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
        ActionBegan?.Invoke();

        try
        {
            if (target == null || !target.IsAlive)
                yield break;

            if (disablePhysicsWhileActing) EnterNonColliding();

            Transform mover = visualRoot ? visualRoot : transform;
            Vector3 startPos = mover.position;
            Vector3 tgtPos = ApproachPoint(target);

            yield return SmoothMove(mover, startPos, tgtPos, goSpeed, hopArcHeight);

            doHit?.Invoke(target);
            yield return null;

            yield return WaitForAnimationRobust(anim, stateToWait);

            yield return SmoothMove(mover, tgtPos, startPos, backSpeed, hopArcHeight);
            mover.position = startPos;
        }
        finally
        {
            if (disablePhysicsWhileActing) ExitNonColliding();
            ActionEnded?.Invoke();
        }
    }

    private Vector3 ApproachPoint(Combatant target)
    {
        if (!isPlayerTeam && BattleManager.instance.IsBossBattle)
            return visualRoot ? visualRoot.position : transform.position;

        Vector3 a = (visualRoot ? visualRoot.position : transform.position);
        Vector3 b = target.transform.position;
        Vector3 dir = (a - b).normalized;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.right;

        //if (!isPlayerTeam && currentAttackType == AttackType.Skill1 && !BattleManager.instance.IsBossBattle)
        //    return b + dir * (approachDistance * 0.8f);
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

        int buff = 0;
        int dmg = 0;
        switch (result)
        {
            case MinigameManager.ResultType.Fail:
                if (id == "CommandBurst")
                    buff = 0;
                else if (id == "TakeABreak")
                    dmg = 1;
                else
                    dmg = Mathf.RoundToInt(stats.atkDmg);
                break;

            case MinigameManager.ResultType.Success:
                if (id == "CommandBurst")
                    buff = 30;
                else
                    dmg = Mathf.RoundToInt(stats.atkDmg * 1.2f);
                break;

            case MinigameManager.ResultType.Perfect:
                if (id == "CommandBurst")
                    buff = 50;
                else if (id == "TakeABreak")
                {
                    int hp = currentTarget?.health.GetCurrHealth() ?? 0;

                    if (!currentTarget.isPlayerTeam && BattleManager.instance.IsBossBattle)
                        dmg = Mathf.RoundToInt(hp * 0.5f);
                    else
                        dmg = hp;
                }
                else
                    dmg = Mathf.RoundToInt(stats.atkDmg * 1.5f);
                break;
        }

        if (id == "CommandBurst" &&
            isProducer &&
            currentAttackType == AttackType.Skill1)
        {
            ApplyProducerRLGLBuff(buff);
        }

        pendingMinigameDamage = dmg;
    }

    private void ApplyProducerRLGLBuff(int buff)
    {
        foreach (var ally in FindObjectsOfType<Combatant>())
        {
            if (!ally.isPlayerTeam || !ally.IsAlive)
                continue;

            if (ally == this)
                continue;

            var buffHandler = ally.GetComponent<PlayerBuffHandler>();
            if (buffHandler != null)
            {
                buffHandler.ApplyAttackBuff(buff, 999f);
            }
        }

        Debug.Log($"[Producer] RLGL Buff Applied: +{buff} ATK to all allies");
    }


    private int CalculateDamage(Combatant target, float multiplierOverride)
    {
        if (target == null || target.health == null)
            return 0;

        float attackPower = stats ? stats.atkDmg : 0f;
        float defensePower = target.stats ? target.stats.attackreduction : 0f;

        // === BuffData Attack Buff ===
        PlayerBuffHandler attackerBH = GetComponent<PlayerBuffHandler>();
        if (BuffData.instance != null && BuffData.instance.hasAttackBuff)
        {
            if (attackerBH != null &&
                BuffData.instance.attackTarget == attackerBH.levelApplier.runtimeStats)
            {
                attackPower += BuffData.instance.latestAttackBuff;
            }
        }

        // === BuffData Defense Buff (applied to target) ===
        PlayerBuffHandler targetBH = target.GetComponent<PlayerBuffHandler>();
        if (BuffData.instance != null && BuffData.instance.hasDefenseBuff)
        {
            if (targetBH != null &&
                BuffData.instance.defenseTarget == targetBH.levelApplier.runtimeStats)
            {
                defensePower += BuffData.instance.latestDefenseBuff;
            }
        }

        float rawDamage = (attackPower * multiplierOverride) - defensePower;
        return Mathf.Max(Mathf.RoundToInt(rawDamage), 1);
    }


    public void Hitstop(float duration)
    {
        HitstopManager.instance.TriggerHitstop(duration);
    }

}
