using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnEngine : MonoBehaviour
{
    [SerializeField] private float atbFillSeconds = 3f;

    public bool autoBattle = false;

    private readonly List<Combatant> _units = new();
    public List<Combatant> GetAllCombatants() { return new List<Combatant>(_units); }
    private bool _running;

    [SerializeField] private TargetSelector targetSelector;
    [SerializeField] private TurnIndicator turnIndicator;
    public StunIndicator stunIndicator;
    [SerializeField] private FloatingDamage floatingDamagePrefab;

    private bool _waitingForPlayerDecision;
    private Combatant _currPlayerUnit;
    public event Action<Combatant> OnPlayerTurnStart;

    public event Action<Combatant, Combatant> OnTurnUnitStart;

    public event Action<bool> OnBattleEnd;

    private int _nextIndex = 0;
    private bool _resolvingAction = false;
    private bool _ended = false;

    // JAS ADDED //
    private float battleSpeed = 1f;
    public float BattleSpeed
    {
        get => battleSpeed;
        set
        {
            battleSpeed = Mathf.Clamp(value, 0.5f, 4f);
            if (!_paused)
                ApplyTimeScale();
        }
    }
    private bool _paused = false;
    public bool IsPaused => _paused;

    public void SetPaused(bool paused)
    {
        _paused = paused;
        ApplyTimeScale();
    }
    private void ApplyTimeScale()
    {
        Time.timeScale = _paused ? 0f : battleSpeed;
    }

    public void Register(Combatant c)
    {
        if (c != null && !_units.Contains(c)) _units.Add(c);
    }

    public void Begin()
    {
        _running = true;
        _waitingForPlayerDecision = false;
        _currPlayerUnit = null;
        _nextIndex = 0;
        _resolvingAction = false;
        _ended = false;

        for (int i = 0; i < _units.Count; i++)
            if (_units[i])
                _units[i].atb = UnityEngine.Random.Range(0.05f, 0.45f);
    }

    public void ForceEnd(bool playerWon)
    {
        if (_ended) return;
        _ended = true;

        BattleSpeed = 1f;
        Time.timeScale = 1f;

        if (!_running) return;

        _running = false;
        _waitingForPlayerDecision = false;
        _currPlayerUnit = null;
        _resolvingAction = false;

        targetSelector?.Disable();

        if (turnIndicator != null) turnIndicator.HideArrow();
        if (stunIndicator != null) stunIndicator.HideAll();

        OnBattleEnd?.Invoke(playerWon);
    }

    private void Update()
    {
        if (!_running || _paused) return;

        //if (IsTeamWiped(false))
        //{
        //    Debug.Log("[TurnEngine] Failsafe: no enemies alive — force end battle (WIN).");
        //    ForceEnd(true);
        //    return;
        //}

        //Time.timeScale = BattleSpeed;

        var leader = _units.Find(u => u && u.isPlayerTeam && u.isLeader);
        if (leader != null && !leader.IsAlive)
        {
            Debug.Log("[TurnEngine] Leader dead — force end battle (LOSS).");
            ForceEnd(false);
            return;
        }

        if (_resolvingAction || _waitingForPlayerDecision) return;

        float step = Time.deltaTime / Mathf.Max(0.01f, atbFillSeconds);

        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;
            u.atb += u.Speed * step;
        }

        int count = _units.Count;
        for (int s = 0; s < count; s++)
        {
            int i = (_nextIndex + s) % count;
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;

            if (u.atb >= 1f)
            {
                if (u.atb >= 1f)
                {
                    if (u.IsStunned)
                    {
                        Combatant nextForUI = PredictNextFromIndex(i);
                        OnTurnUnitStart?.Invoke(u, nextForUI);

                        Debug.Log($"{u.name} is stunned and skips the turn!");
                        _nextIndex = (i + 1) % count;
                        return;
                    }
                    if (u.IsSilenced)
                    {
                        _nextIndex = (i + 1) % count;
                        return;
                    }

                    u.atb = 0f;

                    u.OnTurnStarted();

                    if (turnIndicator != null)
                        turnIndicator.ShowArrow(u.transform);

                    Combatant predictedNext = PredictNextFromIndex(i);

                    if (u.isPlayerTeam)
                    {
                        if (autoBattle)
                        {
                            HookActionLock(u);
                            OnTurnUnitStart?.Invoke(u, predictedNext);
                            AutoAct(u, true);
                        }
                        else
                        {
                            _waitingForPlayerDecision = true;
                            _currPlayerUnit = u;

                            OnPlayerTurnStart?.Invoke(u);
                            OnTurnUnitStart?.Invoke(u, predictedNext);

                            _nextIndex = (i + 1) % count;
                            return;
                        }
                    }
                    else
                    {
                        HookActionLock(u);
                        OnTurnUnitStart?.Invoke(u, predictedNext);
                        AutoAct(u, false);
                    }

                    if (IsTeamWiped(true) || IsTeamWiped(false))
                    {
                        bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
                        ForceEnd(playerWon);
                        return;
                    }

                    _nextIndex = (i + 1) % count;
                    return;
                }
            }
        }
    }
    private Combatant PredictNextFromIndex(int currentIndex)
    {
        int count = _units.Count;
        if (count == 0) return null;

        int start = (currentIndex + 1) % count;
        for (int s = 0; s < count; s++)
        {
            int idx = (start + s) % count;
            if (idx == currentIndex) continue;

            var u = _units[idx];
            if (u == null || !u.IsAlive) continue;

            if (u.atb >= 1f)
                return u;
        }

        float bestAtb = -1f;
        Combatant best = null;
        for (int i = 0; i < count; i++)
        {
            if (i == currentIndex) continue;
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;

            if (u.atb > bestAtb)
            {
                bestAtb = u.atb;
                best = u;
            }
        }

        return best;
    }

    private void HookActionLock(Combatant actor)
    {
        if (actor == null) return;

        actor.ActionBegan -= OnActorActionBegan;
        actor.ActionEnded -= OnActorActionEnded;

        actor.ActionBegan += OnActorActionBegan;
        actor.ActionEnded += OnActorActionEnded;
    }

    private void OnActorActionBegan()
    {
        _resolvingAction = true;

        if (turnIndicator != null)
            turnIndicator.HideArrow(0.50f);


        _currPlayerUnit = null;
    }

    private void OnActorActionEnded()
    {
        _resolvingAction = false;
    }

    public void PlayerChooseBasicAttackTarget(Combatant explicitTarget)
    {
        if (!_waitingForPlayerDecision || _currPlayerUnit == null) return;

        var target = ValidateOrFallback(explicitTarget);
        if (target == null) return;

        HookActionLock(_currPlayerUnit);
        _currPlayerUnit.BasicAttack(target);
        EndPlayerDecisionAndCheck();
    }

    public void PlayerChooseSkillTarget(int skillIndex, Combatant explicitTarget)
    {
        if (!_waitingForPlayerDecision || _currPlayerUnit == null) return;

        HookActionLock(_currPlayerUnit);
        bool used = false;

        if (skillIndex == 0)
        {
            if (_currPlayerUnit.Skill1IsCommand)
            {
                used = _currPlayerUnit.TryUseSkill1(_currPlayerUnit);
            }
            else
            {
                var target = ValidateOrFallback(explicitTarget);
                if (target == null) { _resolvingAction = false; return; }
                used = _currPlayerUnit.TryUseSkill1(target);
            }
        }
        else if (skillIndex == 1)
        {
            if (_currPlayerUnit.Skill2IsSupport)
            {
                used = _currPlayerUnit.TryUseSkill2(_currPlayerUnit);
            }
            else
            {
                var target = ValidateOrFallback(explicitTarget);
                if (target == null) { _resolvingAction = false; return; }
                used = _currPlayerUnit.TryUseSkill2(target);
            }
        }

        if (!used)
        {
            _resolvingAction = false;
            return;
        }

        EndPlayerDecisionAndCheck();
    }

    private void EndPlayerDecisionAndCheck()
    {
        _waitingForPlayerDecision = false;
        _currPlayerUnit = null;
        targetSelector?.Disable();

        if (IsTeamWiped(true) || IsTeamWiped(false))
        {
            bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
            ForceEnd(playerWon);
        }
    }

    private Combatant ValidateOrFallback(Combatant explicitTarget)
    {
        if (explicitTarget != null &&
            _currPlayerUnit != null &&
            explicitTarget.IsAlive &&
            explicitTarget.isPlayerTeam != _currPlayerUnit.isPlayerTeam)
        {
            return explicitTarget;
        }
        return _currPlayerUnit != null ? FindRandomAlive(!_currPlayerUnit.isPlayerTeam) : null;
    }

    private void AutoAct(Combatant actor, bool isLeaderAuto)
    {
        var target = FindRandomAlive(!actor.isPlayerTeam);
        if (target == null) return;

        if (actor.isPlayerTeam)
        {
            if (actor.isLeader && isLeaderAuto)
            {
                int roll = UnityEngine.Random.Range(0, 3);
                if (roll == 0) actor.BasicAttack(target);
                else if (roll == 1) { if (!actor.TryUseSkill1(target)) actor.BasicAttack(target); }
                else { if (!actor.TryUseSkill2(target)) actor.BasicAttack(target); }
            }
            else
            {
                float r = UnityEngine.Random.value;
                if (r < 0.6f) actor.BasicAttack(target);
                else if (r < 0.8f) { if (!actor.TryUseSkill1(target)) actor.BasicAttack(target); }
                else { if (!actor.TryUseSkill2(target)) actor.BasicAttack(target); }
            }
        }
        else
        {
            if (actor.CompareTag("Boss"))
            {
                actor.BasicAttack(target);
                return;
            }

            float r = UnityEngine.Random.value;

            if (r < 0.5f) // 50% 
            {
                actor.BasicAttack(target);
            }
            else if (r < 0.80f)
            {
                // 30%
                if (!actor.TryUseSkill1(target))
                    actor.BasicAttack(target);
            }
            else
            {
                // 20%
                if (!actor.TryUseSkill2(target))
                    actor.BasicAttack(target);
            }
        }
    }

    private Combatant FindRandomAlive(bool playerTeam)
    {
        List<Combatant> alive = new();
        foreach (var u in _units)
            if (u && u.isPlayerTeam == playerTeam && u.IsAlive)
                alive.Add(u);

        if (alive.Count == 0) return null;
        return alive[UnityEngine.Random.Range(0, alive.Count)];
    }

    private bool IsTeamWiped(bool playerTeam)
    {
        foreach (var u in _units)
            if (u && u.isPlayerTeam == playerTeam && u.IsAlive)
                return false;
        return true;
    }
    public void ShowFloatingText(Combatant target, string message)
    {
        if (floatingDamagePrefab == null || target == null) return;

        var floatObj = Instantiate(floatingDamagePrefab, target.transform.position, Quaternion.identity);
        floatObj.InitializeStatus(message, Color.yellow);  
    }
}
