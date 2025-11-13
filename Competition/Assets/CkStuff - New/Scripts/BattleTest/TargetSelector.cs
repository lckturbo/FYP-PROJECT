using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] private LayerMask selectableLayers;
    [SerializeField] private Color highlight;
    [SerializeField] private bool clickEmptyClears = true;

    private bool _active;
    private Combatant _current;

    private readonly List<SpriteRenderer> _renderers = new();
    private readonly List<Color> _originalColors = new();

    public Combatant Current => _current;
    public event Action<Combatant> OnSelectionChanged;

    public void EnableForPlayerUnit(Combatant actingUnit)
    {
        _active = true;
    }

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
        if (c == null) return;
        if (c.isPlayerTeam) return;
        if (!c.IsAlive) return;
        if (c == _current) return;

        Unhighlight();

        _current = c;
        CacheAndHighlight(_current);

        OnSelectionChanged?.Invoke(_current);
    }

    public void Clear()
    {
        if (_current == null) return;
        Unhighlight();
        _current = null;
        OnSelectionChanged?.Invoke(null);
    }

    private void CacheAndHighlight(Combatant c)
    {
        _renderers.Clear();
        _originalColors.Clear();

        c.GetComponentsInChildren(true, _renderers);
        if (_renderers.Count == 0) return;

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
