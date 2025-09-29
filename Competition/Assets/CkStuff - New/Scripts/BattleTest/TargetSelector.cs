using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    [Header("Pick Settings")]
    [SerializeField] private LayerMask selectableLayers;
    [SerializeField] private Color highlight = new(1f, .85f, .2f, 1f);
    [SerializeField] private bool clickEmptyClears = true;

    [Header("Debug")]
    [SerializeField] private bool debugForceActive = false;
    [SerializeField] private bool debugLogs = true;

    private bool _active;
    private Combatant _current;

    private readonly List<SpriteRenderer> _renderers = new();
    private readonly List<Color> _originalColors = new();

    public Combatant Current => _current;
    public event Action<Combatant> OnSelectionChanged;

    public void EnableForLeaderTurn()
    {
        _active = true;
        if (debugLogs) Debug.Log("[TargetSelector] ENABLED");
    }

    public void Disable()
    {
        _active = false;
        if (debugLogs) Debug.Log("[TargetSelector] DISABLED");
        Clear();
    }

    private void Update()
    {
        if (!(debugForceActive || _active)) return;

        if (_current != null && !_current.IsAlive)
            Clear();

        if (Input.GetMouseButtonDown(0))
            TryPickUnderMouse();
    }

    private void TryPickUnderMouse()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            if (debugLogs) Debug.Log("[TargetSelector] Click over UI — ignored");
            return;
        }

        var cam = Camera.main;
        if (!cam) { if (debugLogs) Debug.LogWarning("[TargetSelector] No MainCamera"); return; }

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, selectableLayers);
        Collider2D col = hit.collider;

        if (!col)
        {
            Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
            col = Physics2D.OverlapPoint(wp, selectableLayers);
            if (!col)
            {
                if (clickEmptyClears) Clear();
                if (debugLogs) Debug.Log("[TargetSelector] No collider under mouse");
                return;
            }
        }

        var c = col.GetComponentInParent<Combatant>();
        if (c == null) { if (debugLogs) Debug.Log("[TargetSelector] Collider has no Combatant"); return; }
        if (c.isPlayerTeam) { if (debugLogs) Debug.Log("[TargetSelector] Clicked a player, ignoring"); return; }
        if (!c.IsAlive) { if (debugLogs) Debug.Log("[TargetSelector] Target dead, ignoring"); return; }

        if (c == _current) { if (debugLogs) Debug.Log("[TargetSelector] Same target clicked, ignoring"); return; }

        Unhighlight();

        _current = c;
        CacheAndHighlight(_current);

        OnSelectionChanged?.Invoke(_current);
        if (debugLogs) Debug.Log($"[TARGET] Selected {_current.name}");
    }

    public void Clear()
    {
        if (_current == null) return;
        Unhighlight();
        _current = null;
        OnSelectionChanged?.Invoke(null);
        if (debugLogs) Debug.Log("[TARGET] Cleared");
    }

    private void CacheAndHighlight(Combatant c)
    {
        _renderers.Clear();
        _originalColors.Clear();

        c.GetComponentsInChildren(true, _renderers);
        if (_renderers.Count == 0)
        {
            if (debugLogs) Debug.LogWarning($"[TargetSelector] No SpriteRenderer found under {c.name}");
            return;
        }

        for (int i = 0; i < _renderers.Count; i++)
        {
            var sr = _renderers[i];
            _originalColors.Add(sr.color);
            sr.color = highlight;
        }
    }

    private void Unhighlight()
    {
        if (_renderers.Count == 0) return;

        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i]) _renderers[i].color = _originalColors[i];
        }

        _renderers.Clear();
        _originalColors.Clear();
    }
}
