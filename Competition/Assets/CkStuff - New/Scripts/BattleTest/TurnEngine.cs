using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnEngine : MonoBehaviour
{
    [Tooltip("Seconds to fill ATB from 0->1 when Speed = 1")]
    [SerializeField] private float atbFillSeconds = 3f;

    public bool autoBattle = false;                 // <-- toggle ON/OFF

    private readonly List<Combatant> _units = new();
    private bool _running;

    // Leader choice plumbing
    public event Action<Combatant> OnLeaderTurnStart;   // UI listens
    private bool _waitingForLeader;
    private Combatant _currentLeader;

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
                    {
                        AutoAct(u);
                    }
                    else
                    {
                        _waitingForLeader = true;
                        _currentLeader = u;
                        OnLeaderTurnStart?.Invoke(u); // show action UI
                        return;
                    }
                }
                else
                {
                    AutoAct(u);
                }

                if (IsTeamWiped(true) || IsTeamWiped(false)) { _running = false; return; }
            }
        }
    }

    // ---- UI callbacks ----
    public void LeaderChooseBasicAttack()
    {
        if (!_waitingForLeader || _currentLeader == null) return;
        var target = FindFirstAlive(opponentOf: _currentLeader);
        if (target != null) _currentLeader.BasicAttack(target);
        _waitingForLeader = false;
        _currentLeader = null;
        if (IsTeamWiped(true) || IsTeamWiped(false)) _running = false;
    }

    public void LeaderChooseSkill(int skillIndex)
    {
        // TODO: pick a skill later; for now behave like Attack
        LeaderChooseBasicAttack();
    }

    // ---- helpers ----
    private void AutoAct(Combatant actor)
    {
        var target = FindFirstAlive(opponentOf: actor);
        if (target != null) actor.BasicAttack(target);

        Debug.Log($"[TURN] {actor.name} -> {target.name}");
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
