using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnEngine : MonoBehaviour
{
    // Seconds to fill ATB from 0 -> 1 when Speed = 1
    [SerializeField] private float atbFillSeconds = 3f;

    public bool autoBattle = false;

    private readonly List<Combatant> _units = new();
    private bool _running;

    [SerializeField] private TargetSelector targetSelector;

    // Leader choice plumbing
    public event Action<Combatant> OnLeaderTurnStart;
    private bool _waitingForLeader;
    private Combatant _currentLeader;

    public event Action<bool> OnBattleEnd;

    // Round-robin pointer for fair turn order among ties
    private int _nextIndex = 0;

    // While true, an action animation/coroutine is running
    private bool _resolvingAction = false;

    // ==== NEW: guard to prevent double end-of-battle firing ====
    private bool _ended = false; // prevents re-entrancy

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

        // small random stagger so identical speeds don't pop together
        for (int i = 0; i < _units.Count; i++)
            if (_units[i])
                _units[i].atb = UnityEngine.Random.Range(0.05f, 0.45f);
    }

    public void ForceEnd(bool playerWon)
    {
        // guard: only end once
        if (_ended) return;
        _ended = true;

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
        if (!_running) return;

        // Pause the loop if an action is resolving
        if (_resolvingAction) return;

        // If leader died, end battle immediately
        var leader = _units.Find(u => u && u.isPlayerTeam && u.isLeader);
        if (leader != null && !leader.IsAlive)
        {
            ForceEnd(false);
            return;
        }

        if (_waitingForLeader) return;

        float step = Time.deltaTime / Mathf.Max(0.01f, atbFillSeconds);

        // PASS 1: fill everyone's ATB
        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;
            u.atb += u.Speed * step;
        }

        // PASS 2: pick exactly ONE ready unit to act (round-robin from _nextIndex)
        int count = _units.Count;
        for (int s = 0; s < count; s++)
        {
            int i = (_nextIndex + s) % count;
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;

            if (u.atb >= 1f)
            {
                // consume turn and tick per-turn systems (cooldowns)
                u.atb = 0f;
                u.OnTurnStarted();

                if (u.isPlayerTeam && u.isLeader)
                {
                    if (autoBattle)
                    {
                        HookActionLock(u); // lock while action plays
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
                        return; // exactly one actor per frame
                    }
                }
                else
                {
                    HookActionLock(u); // lock while action plays
                    AutoAct(u, false);
                }

                // After any action, check for wipe and end cleanly
                if (IsTeamWiped(true) || IsTeamWiped(false))
                {
                    bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
                    ForceEnd(playerWon);
                    return;
                }

                _nextIndex = (i + 1) % count; // advance RR pointer
                return; // exactly one actor per frame
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
    private void OnActorActionEnded()
    {
        // if battle already ended, do nothing special; just clear the lock
        _resolvingAction = false;
    }

    // === Leader actions ===
    public void LeaderChooseBasicAttackTarget(Combatant explicitTarget)
    {
        if (!_waitingForLeader || _currentLeader == null) return;

        var target = ValidateOrFallback(explicitTarget);
        if (target == null)
        {
            Debug.LogWarning("[TURN] No valid target for basic attack");
            return;
        }

        HookActionLock(_currentLeader);
        _currentLeader.BasicAttack(target);
        Debug.Log($"[TURN] Leader {_currentLeader.name} used BASIC ATTACK on {target.name}");
        EndLeaderDecisionAndCheck();
    }

    public void LeaderChooseSkillTarget(int skillIndex, Combatant explicitTarget)
    {
        if (!_waitingForLeader || _currentLeader == null) return;

        var target = ValidateOrFallback(explicitTarget);
        if (target == null)
        {
            Debug.LogWarning($"[TURN] No valid target for skill {skillIndex + 1}");
            return;
        }

        HookActionLock(_currentLeader);
        bool used = false;
        if (skillIndex == 0) used = _currentLeader.TryUseSkill1(target);
        else if (skillIndex == 1) used = _currentLeader.TryUseSkill2(target);

        if (!used)
        {
            // no ActionBegan fired (skill refused), ensure we’re not locked
            _resolvingAction = false;
            Debug.LogWarning("[TURN] Skill on cooldown.");
            return;
        }

        Debug.Log($"[TURN] Leader {_currentLeader.name} used SKILL {skillIndex + 1} on {target.name}");
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

        Debug.Log($"[TURN] {actor.name} auto-acted on {target.name}");
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
