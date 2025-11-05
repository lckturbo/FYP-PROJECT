using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnEngine : MonoBehaviour
{
    // Seconds to fill ATB from 0 -> 1 when Speed = 1
    [SerializeField] private float atbFillSeconds = 3f;

    public bool autoBattle = false;

    private readonly List<Combatant> _units = new();
    public List<Combatant> GetAllCombatants() { return new List<Combatant>(_units); }
    private bool _running;

    [SerializeField] private TargetSelector targetSelector;

    // Leader choice plumbing
    public event Action<Combatant> OnLeaderTurnStart;
    private bool _waitingForLeader;
    private Combatant _currentLeader;

    public event Action<bool> OnBattleEnd;

    private int _nextIndex = 0;
    private bool _resolvingAction = false;
    private bool _ended = false;

    // JAS ADDED //
    private float battleSpeed = 1f;
    public float BattleSpeed
    {
        get => battleSpeed;
        set => battleSpeed = Mathf.Clamp(value, 0.5f, 4f);
    }
    private bool _paused = false;
    public bool IsPaused => _paused;

    public void SetPaused(bool paused)
    {
        _paused = paused;
        Time.timeScale = paused ? 0f : battleSpeed;
    }

    public void Register(Combatant c)
    {
        if (c != null && !_units.Contains(c)) _units.Add(c);
    }

    public void Begin()
    {
        _running = true;
        _waitingForLeader = false;
        _currentLeader = null;
        _nextIndex = 0;
        _resolvingAction = false;
        _ended = false; // reset end guard for fresh battle

        // small random stagger
        for (int i = 0; i < _units.Count; i++)
            if (_units[i])
                _units[i].atb = UnityEngine.Random.Range(0.05f, 0.45f);
    }

    public void ForceEnd(bool playerWon)
    {
        // guard: only end once
        if (_ended) return;
        _ended = true;
        Time.timeScale = 1f;

        if (!_running) return;

        _running = false;
        _waitingForLeader = false;
        _currentLeader = null;
        _resolvingAction = false;

        targetSelector?.Disable();
        OnBattleEnd?.Invoke(playerWon);
    }

    private void Update()
    {
        if (!_running || _paused) return;

        Time.timeScale = battleSpeed;

        // ========= Global failsafes (run EVERY frame) =========
        // 1) If all ENEMIES are gone at any moment (DOT, projectile, etc.), end as WIN.
        if (IsTeamWiped(false))
        {
            Debug.Log("[TurnEngine] Failsafe: no enemies alive — force end battle (WIN).");
            ForceEnd(true);
            return;
        }

        // 2) If the LEADER died at any moment, end as LOSS.
        var leader = _units.Find(u => u && u.isPlayerTeam && u.isLeader);
        if (leader != null && !leader.IsAlive)
        {
            Debug.Log("[TurnEngine] Leader dead — force end battle (LOSS).");
            ForceEnd(false);
            return;
        }
        // ======================================================

        // Pause the loop if an action is resolving (but after failsafes above)
        if (_resolvingAction) return;

        if (_waitingForLeader) return;

        float step = Time.deltaTime / Mathf.Max(0.01f, atbFillSeconds);

        // PASS 1: fill everyone's ATB
        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;
            u.atb += u.Speed * step;
        }

        // PASS 2: pick exactly ONE ready unit to act
        int count = _units.Count;
        for (int s = 0; s < count; s++)
        {
            int i = (_nextIndex + s) % count;
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;

            if (u.atb >= 1f)
            {
                u.atb = 0f;
                u.OnTurnStarted();

                if (u.isPlayerTeam && u.isLeader)
                {
                    if (autoBattle)
                    {
                        HookActionLock(u);
                        AutoAct(u, true);
                    }
                    else
                    {
                        _waitingForLeader = true;
                        _currentLeader = u;
                        OnLeaderTurnStart?.Invoke(u);

                        if (targetSelector)
                        {
                            targetSelector.EnableForLeaderTurn();
                            targetSelector.Clear();
                        }

                        _nextIndex = (i + 1) % count;
                        return;
                    }
                }
                else
                {
                    HookActionLock(u);
                    AutoAct(u, false);
                }

                // Standard end check after any action
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

    // === Action lock wiring ===
    private void HookActionLock(Combatant actor)
    {
        if (actor == null) return;

        // avoid double-subscribe
        actor.ActionBegan -= OnActorActionBegan;
        actor.ActionEnded -= OnActorActionEnded;

        actor.ActionBegan += OnActorActionBegan;
        actor.ActionEnded += OnActorActionEnded;
    }

    private void OnActorActionBegan() { _resolvingAction = true; }
    private void OnActorActionEnded() { _resolvingAction = false; }

    // === Leader actions ===
    public void LeaderChooseBasicAttackTarget(Combatant explicitTarget)
    {
        if (!_waitingForLeader || _currentLeader == null) return;

        var target = ValidateOrFallback(explicitTarget);
        if (target == null) return;

        HookActionLock(_currentLeader);
        _currentLeader.BasicAttack(target);
        EndLeaderDecisionAndCheck();
    }

    public void LeaderChooseSkillTarget(int skillIndex, Combatant explicitTarget)
    {
        if (!_waitingForLeader || _currentLeader == null) return;

        var target = ValidateOrFallback(explicitTarget);
        if (target == null) return;

        HookActionLock(_currentLeader);
        bool used = false;
        if (skillIndex == 0) used = _currentLeader.TryUseSkill1(target);
        else if (skillIndex == 1) used = _currentLeader.TryUseSkill2(target);

        if (!used)
        {
            _resolvingAction = false;
            return;
        }
        EndLeaderDecisionAndCheck();
    }

    private void EndLeaderDecisionAndCheck()
    {
        _waitingForLeader = false;
        _currentLeader = null;
        targetSelector?.Disable();

        if (IsTeamWiped(true) || IsTeamWiped(false))
        {
            bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
            ForceEnd(playerWon);
        }
    }

    // === Helpers ===
    private Combatant ValidateOrFallback(Combatant explicitTarget)
    {
        if (explicitTarget != null &&
            _currentLeader != null &&
            explicitTarget.IsAlive &&
            explicitTarget.isPlayerTeam != _currentLeader.isPlayerTeam)
        {
            return explicitTarget;
        }
        return _currentLeader != null ? FindRandomAlive(!_currentLeader.isPlayerTeam) : null;
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
            // simple enemy logic with cooldown-aware usage of skill1
            if (UnityEngine.Random.value < 0.5f || !actor.IsSkill1Ready)
                actor.BasicAttack(target);
            else
                actor.TryUseSkill1(target);
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
}
