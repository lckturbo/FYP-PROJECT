using UnityEngine;

[DisallowMultipleComponent]
public class BeamReceiver : MonoBehaviour
{
    [Header("Door / Target")]
    public ToggleableBlock doorBlock;
    public Animator doorAnimator;

    [Header("Behavior")]
    [Tooltip("If true, once activated it stays active (no auto-deactivate).")]
    public bool latchOpen = false;

    [Tooltip("How long after beam stops before auto-deactivate (prevents flicker).")]
    [Min(0f)] public float graceSeconds = 0.15f;

    [Header("Visual")]
    public SpriteRenderer indicator;
    public Color activeColor = Color.cyan;
    public Color idleColor = Color.gray;

    public bool IsActive => _active;

    public event System.Action<BeamReceiver> OnActivated;
    public event System.Action<BeamReceiver> OnDeactivated;

    // runtime
    private bool _active;
    private float _timer;

    private void Start()
    {
        SetVisual(false);
    }

    public void OnBeamStay()
    {
        _timer = Mathf.Max(_timer, graceSeconds);
        if (!_active) Activate();
    }

    private void Update()
    {
        if (!_active || latchOpen) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            Deactivate();
    }

    private void Activate()
    {
        _active = true;
        SetVisual(true);

        if (doorBlock != null) doorBlock.Open();
        else if (doorAnimator != null) doorAnimator.SetTrigger("Open");

        OnActivated?.Invoke(this);
    }

    private void Deactivate()
    {
        _active = false;
        SetVisual(false);

        if (doorBlock != null) doorBlock.Close();
        else if (doorAnimator != null) doorAnimator.SetTrigger("Close");

        OnDeactivated?.Invoke(this);
    }

    private void SetVisual(bool on)
    {
        if (indicator != null)
            indicator.color = on ? activeColor : idleColor;
    }

    public void ForceReset()
    {
        _timer = 0f;
        _active = false;
        SetVisual(false);

        if (doorBlock != null) doorBlock.Close();
        else if (doorAnimator != null) doorAnimator.SetTrigger("Close");

        OnDeactivated?.Invoke(this);
    }

    public void SetDoorDirect(bool open)
    {
        if (doorBlock != null) doorBlock.SetOpen(open);
        else if (doorAnimator != null) doorAnimator.SetTrigger(open ? "Open" : "Close");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Cyan when active, gray when idle
        Color c = IsActive ? new Color(0f, 1f, 1f, 0.8f) : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
#endif
}
