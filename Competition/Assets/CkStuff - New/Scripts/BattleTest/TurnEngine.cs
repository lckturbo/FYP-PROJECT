using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnEngine : MonoBehaviour
{
    [Tooltip("Seconds to fill ATB from 0->1 when Speed = 1")]
    [SerializeField] private float atbFillSeconds = 3f;

    public bool autoBattle = false;

    private readonly List<Combatant> _units = new();
    private bool _running;

    // Leader choice plumbing
    public event Action<Combatant> OnLeaderTurnStart;
    private bool _waitingForLeader;
    private Combatant _currentLeader;

    public event Action<bool> OnBattleEnd;

    public void Register(Combatant c)
    {
        if (c != null && !_units.Contains(c)) _units.Add(c);
    }

    public void Begin()
    {
        _running = true;
        for (int i = 0; i < _units.Count; i++) if (_units[i]) _units[i].atb = 0f;
    }

    private void Update()
    {
        if (!_running || _waitingForLeader) return;
        float step = Time.deltaTime / Mathf.Max(0.01f, atbFillSeconds);

        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u == null || !u.IsAlive) continue;

            u.atb += u.Speed * step;
            if (u.atb >= 1f)
            {
                u.atb = 0f;

                if (u.isPlayerTeam && u.isLeader)
                {
                    if (autoBattle)
                        AutoAct(u, true);
                    else
                    {
                        _waitingForLeader = true;
                        _currentLeader = u;
                        OnLeaderTurnStart?.Invoke(u);
                        return;
                    }
                }
                else
                {
                    AutoAct(u, false);
                }

                if (IsTeamWiped(true) || IsTeamWiped(false))
                {
                    _running = false;
                    bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
                    OnBattleEnd?.Invoke(playerWon);
                    return;
                }
            }
        }
    }

    // ---- UI callbacks ----
    public void LeaderChooseBasicAttack()
    {
        if (!_waitingForLeader || _currentLeader == null) return;
        var target = FindFirstAlive(opponentOf: _currentLeader);
        if (target != null) _currentLeader.BasicAttack(target);

        Debug.Log($"[TURN] Leader {_currentLeader.name} used BASIC ATTACK on {target?.name}");

        _waitingForLeader = false;
        _currentLeader = null;

        if (IsTeamWiped(true) || IsTeamWiped(false))
        {
            _running = false;
            bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
            OnBattleEnd?.Invoke(playerWon);
        }
    }

    public void LeaderChooseSkill(int skillIndex)
    {
        if (!_waitingForLeader || _currentLeader == null) return;
        var target = FindFirstAlive(opponentOf: _currentLeader);

        if (skillIndex == 0)
        {
            _currentLeader.Skill1(target);
            Debug.Log($"[TURN] Leader {_currentLeader.name} used SKILL 1 on {target?.name}");
        }
        else if (skillIndex == 1)
        {
            _currentLeader.Skill2(target);
            Debug.Log($"[TURN] Leader {_currentLeader.name} used SKILL 2 on {target?.name}");
        }

        _waitingForLeader = false;
        _currentLeader = null;

        if (IsTeamWiped(true) || IsTeamWiped(false))
        {
            _running = false;
            bool playerWon = IsTeamWiped(false) && !IsTeamWiped(true);
            OnBattleEnd?.Invoke(playerWon);
        }
    }

    // ---- helpers ----
    private void AutoAct(Combatant actor, bool isLeaderAuto)
    {
        var target = FindFirstAlive(opponentOf: actor);
        if (target == null) return;

        if (actor.isPlayerTeam)
        {
            if (actor.isLeader && isLeaderAuto)
            {
                int roll = UnityEngine.Random.Range(0, 3);
                if (roll == 0) actor.BasicAttack(target);
                else if (roll == 1) actor.Skill1(target);
                else actor.Skill2(target);
            }
            else
            {
                float r = UnityEngine.Random.value;
                if (r < 0.6f) actor.BasicAttack(target);
                else if (r < 0.8f) actor.Skill1(target);
                else actor.Skill2(target);
            }
        }
        else
        {
            // Enemy AI
            if (UnityEngine.Random.value < 0.5f) actor.BasicAttack(target);
            else actor.Skill1(target);
        }

        Debug.Log($"[TURN] {actor.name} auto-acted on {target.name}");
    }

    private Combatant FindFirstAlive(Combatant opponentOf)
    {
        bool targetTeam = !opponentOf.isPlayerTeam;
        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u && u.isPlayerTeam == targetTeam && u.IsAlive) return u;
        }
        return null;
    }

    private bool IsTeamWiped(bool playerTeam)
    {
        for (int i = 0; i < _units.Count; i++)
        {
            var u = _units[i];
            if (u && u.isPlayerTeam == playerTeam && u.IsAlive) return false;
        }
        return true;
    }
}
