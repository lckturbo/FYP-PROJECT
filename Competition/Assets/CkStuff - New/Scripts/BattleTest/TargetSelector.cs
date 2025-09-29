using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TargetSelector : MonoBehaviour
{
    [Header("Pick Settings")]
    [SerializeField] private LayerMask selectableLayers;
    [SerializeField] private Color highlight = new(1f, .85f, .2f, 1f);
    [SerializeField] private bool clickEmptyClears = true;

    private bool _active;
    private Combatant _current;
    private SpriteRenderer _lastSR;
    private Color _lastColor;

    public Combatant Current => _current;
    public event Action<Combatant> OnSelectionChanged;

    public void EnableForLeaderTurn() => _active = true;

    public void Disable()
    {
        _active = false;
        Clear();
    }

    private void Update()
    {
        if (!_active) return;

        if (_current != null && !_current.IsAlive)
            Clear();

        if (Input.GetMouseButtonDown(0))
            TryPickUnderMouse();
    }

    private void TryPickUnderMouse()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            return;

        var cam = Camera.main;
        if (!cam) return;

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
                return;
            }
        }

        var c = col.GetComponentInParent<Combatant>();
        if (c == null || c.isPlayerTeam || !c.IsAlive) return;

        if (c == _current) return;

        Unhighlight();
        _current = c;
        _lastSR = c.GetComponentInChildren<SpriteRenderer>();
        if (_lastSR)
        {
            _lastColor = _lastSR.color;
            _lastSR.color = highlight;
        }

        OnSelectionChanged?.Invoke(_current);
        Debug.Log($"[TARGET] Selected {_current.name}");
    }

    public void Clear()
    {
        if (_current == null) return;
        Unhighlight();
        _current = null;
        _lastSR = null;
        OnSelectionChanged?.Invoke(null);
        Debug.Log("[TARGET] Cleared");
    }

    private void Unhighlight()
    {
        if (_lastSR) _lastSR.color = _lastColor;
    }
}
