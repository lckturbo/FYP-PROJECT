using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class BeamReceiver : MonoBehaviour
{
    [Header("Door/Target")]
    public ToggleableBlock doorBlock;   // or use doorAnimator
    public Animator doorAnimator;

    [Header("Behavior")]
    public bool latchOpen = false;      // true = open once and stay
    [Min(0f)] public float graceSeconds = 0.15f;

    [Header("Visual")]
    public SpriteRenderer indicator;
    public Color activeColor = Color.cyan;
    public Color idleColor = Color.gray;

    // ==== Cinematic Camera (same style as SwitchInteract) ====
    [Header("Cinematic Camera")]
    public Transform blockFocus;             // door or mechanism focus
    public Animator blockAnimator;
    public Transform returnFocus;            // usually player; auto-filled

    [Header("First Pan (Receiver)")]
    [Tooltip("If null, will use this receiver's transform.")]
    public Transform receiverFocus;          // new: where to pan first (the receiver)
    public float receiverHold = 0.15f;       // small hold on the receiver
    public float receiverAnimFallback = 0.25f;

    [Header("Timings")]
    public float panDuration = 0.6f;
    public float holdOnBlock = 0.2f;
    public float blockAnimFallback = 0.45f;

    [Header("Spam Control")]
    public float cooldown = 0.25f;

    public bool IsActive => _active;

    public event System.Action<BeamReceiver> OnActivated;
    public event System.Action<BeamReceiver> OnDeactivated;

    // runtime
    private bool _active;
    private float _timer;

    private bool _coolingDown;
    private bool _playedOnce;

    private PlayerInput _playerInput;
    private NewPlayerMovement _playerMove;

    private void Awake()
    {
        TryBindPlayerInput();
        if (receiverFocus == null) receiverFocus = transform; // default to this receiver
    }

    private void Start()
    {
        SetVisual(false);
    }

    // Call this every frame the beam is hitting the receiver
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

        if (doorBlock) doorBlock.Open();
        else if (doorAnimator) doorAnimator.SetTrigger("Open");

        // Start cinematic once
        if (!_coolingDown && !_playedOnce)
            StartCoroutine(UseSequence());

        OnActivated?.Invoke(this);
    }

    private void Deactivate()
    {
        _active = false;
        SetVisual(false);

        if (!latchOpen)
        {
            if (doorBlock) doorBlock.Close();
            else if (doorAnimator) doorAnimator.SetTrigger("Close");
        }

        OnDeactivated?.Invoke(this);
    }

    private IEnumerator UseSequence()
    {
        _coolingDown = true;

        SetPlayerControl(false);

        NewCameraController cam = FindObjectOfType<NewCameraController>();

        // 1) PAN TO RECEIVER FIRST
        if (cam && receiverFocus)
        {
            yield return cam.PanTo(receiverFocus, panDuration);
            yield return new WaitForSeconds(panDuration);

            // optional receiver wait (use fallback timing; no animator by default)
            if (receiverAnimFallback > 0f)
                yield return new WaitForSeconds(receiverAnimFallback);

            if (receiverHold > 0f)
                yield return new WaitForSeconds(receiverHold);
        }

        // 2) THEN PAN TO BLOCK/DOOR FOCUS (same as switch flow)
        if (cam && blockFocus)
        {
            yield return cam.PanTo(blockFocus, panDuration);
            yield return new WaitForSeconds(panDuration);
        }

        if (blockAnimator)
            yield return WaitForCurrentAnimOrFallback(blockAnimator, blockAnimFallback);
        else
            yield return new WaitForSeconds(blockAnimFallback);

        if (holdOnBlock > 0f)
            yield return new WaitForSeconds(holdOnBlock);

        // 3) PAN BACK TO PLAYER
        if (cam)
        {
            yield return cam.ReturnToPlayer(panDuration);
            yield return new WaitForSeconds(panDuration);
        }

        SetPlayerControl(true);

        if (cooldown > 0f)
            yield return new WaitForSeconds(cooldown);

        _coolingDown = false;
        _playedOnce = true; // play once per activation lifecycle
    }

    private void SetPlayerControl(bool enabled)
    {
        if (_playerInput) _playerInput.enabled = enabled;
    }

    private IEnumerator WaitForCurrentAnimOrFallback(Animator anim, float fallbackSeconds)
    {
        if (!anim) { yield return new WaitForSeconds(fallbackSeconds); yield break; }

        float clipLen = fallbackSeconds;
        var info = anim.GetCurrentAnimatorClipInfo(0);
        if (info != null && info.Length > 0 && info[0].clip)
            clipLen = Mathf.Max(0.05f, info[0].clip.length);

        float elapsed = 0f;
        while (elapsed < clipLen)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void TryBindPlayerInput()
    {
        if (_playerMove == null)
            _playerMove = FindObjectOfType<NewPlayerMovement>();

        if (_playerMove != null)
        {
            if (_playerInput == null)
                _playerInput = _playerMove.GetComponent<PlayerInput>();

            if (!returnFocus) returnFocus = _playerMove.transform;
        }
    }

    private void SetVisual(bool on)
    {
        if (indicator) indicator.color = on ? activeColor : idleColor;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = IsActive ? new Color(0f, 1f, 1f, 0.8f) : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
#endif
}
