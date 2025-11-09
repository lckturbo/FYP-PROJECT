using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class BeamReceiver : MonoBehaviour, IDataPersistence
{
    [Header("Door/Target")]
    public ToggleableBlock doorBlock;
    public Animator doorAnimator;

    [Header("Behavior")]
    public bool latchOpen = false;
    [Min(0f)] public float graceSeconds = 0.15f;

    [Header("Visual")]
    public SpriteRenderer indicator;
    public Color activeColor = Color.cyan;
    public Color idleColor = Color.gray;

    [Header("Cinematic Camera")]
    public Transform blockFocus;
    public Animator blockAnimator;
    public Transform returnFocus;

    [Header("First Pan (Receiver)")]
    public Transform receiverFocus;
    public float receiverHold = 0.15f;
    public float receiverAnimFallback = 0.25f;

    [Header("Timings")]
    public float panDuration = 0.6f;
    public float holdOnBlock = 0.2f;
    public float blockAnimFallback = 0.45f;

    [Header("Spam Control")]
    public float cooldown = 0.25f;

    [Header("Timing Options")]
    public bool openDuringCinematic = true;

    public bool IsActive => _active;

    public event System.Action<BeamReceiver> OnActivated;
    public event System.Action<BeamReceiver> OnDeactivated;

    private bool _active;
    private float _timer;
    private bool _coolingDown;

    private PlayerInput _playerInput;
    private NewPlayerMovement _playerMove;

    private bool solved;
    private bool skipCinematicOnce;

    private void Awake()
    {
        TryBindPlayerInput();
        if (receiverFocus == null) receiverFocus = transform;
    }

    private void Start() { SetVisual(false); }

    public void OnBeamStay()
    {
        _timer = Mathf.Max(_timer, graceSeconds);
        if (!_active) Activate();
    }

    private void Update()
    {
        if (!_active || latchOpen) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) Deactivate();
    }

    private void Activate()
    {
        _active = true;
        SetVisual(true);

        if (!solved) solved = true;

        if (skipCinematicOnce)
        {
            skipCinematicOnce = false;

            if (doorBlock) doorBlock.Open();
            else if (doorAnimator) doorAnimator.SetTrigger("Open");

            OnActivated?.Invoke(this);
            return;
        }
        
        if (!openDuringCinematic)
        {
            if (doorBlock) doorBlock.Open();
            else if (doorAnimator) doorAnimator.SetTrigger("Open");
        }

        if (!_coolingDown)
            StartCoroutine(UseSequence(true));

        OnActivated?.Invoke(this);
    }

    private void Deactivate()
    {
        _active = false;
        SetVisual(false);

        if (solved) solved = false;

        if (!latchOpen)
        {
            if (doorBlock) doorBlock.Close();
            else if (doorAnimator) doorAnimator.SetTrigger("Close");

            if (!_coolingDown)
                StartCoroutine(UseSequence(false));
        }

        OnDeactivated?.Invoke(this);
    }

    private IEnumerator UseSequence(bool opening)
    {
        _coolingDown = true;

        SetPlayerControl(false);
        GameInputLock.inputLocked = true;

        NewCameraController cam = FindObjectOfType<NewCameraController>();

        Transform receiverAnchor = receiverFocus ? CreateTempAnchor(receiverFocus.position, "CamTemp_Receiver") : null;
        Transform blockAnchor = blockFocus ? CreateTempAnchor(blockFocus.position, "CamTemp_Block") : null;

        try
        {
            if (cam && receiverAnchor)
                yield return cam.PanTo(receiverAnchor, panDuration);

            if (receiverAnimFallback > 0f)
                yield return new WaitForSeconds(receiverAnimFallback);
            if (receiverHold > 0f)
                yield return new WaitForSeconds(receiverHold);

            if (cam && blockAnchor)
                yield return cam.PanTo(blockAnchor, panDuration);

            if (doorBlock)
            {
                if (opening) doorBlock.Open();
                else doorBlock.Close();
            }
            else if (doorAnimator)
            {
                doorAnimator.SetTrigger(opening ? "Open" : "Close");
            }

            if (blockAnimator)
                yield return WaitForCurrentAnimOrFallback(blockAnimator, blockAnimFallback);
            else
                yield return new WaitForSeconds(blockAnimFallback);

            if (holdOnBlock > 0f)
                yield return new WaitForSeconds(holdOnBlock);

            if (cam)
                yield return cam.ReturnToPlayer(panDuration);
        }
        finally
        {
            if (receiverAnchor) Destroy(receiverAnchor.gameObject);
            if (blockAnchor) Destroy(blockAnchor.gameObject);

            // Re-enable inputs
            SetPlayerControl(true);
            GameInputLock.inputLocked = false;
        }
        if (cooldown > 0f)
            yield return new WaitForSeconds(cooldown);

        _coolingDown = false;
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

    private Transform CreateTempAnchor(Vector3 worldPos, string name)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        return go.transform;
    }
    public void LoadData(GameData data)
    {
        solved = data.beamReceiverSolved;

        if (solved)
            skipCinematicOnce = true;
    }

    public void SaveData(ref GameData data)
    {
        data.beamReceiverSolved = solved;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = IsActive ? new Color(0f, 1f, 1f, 0.8f) : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
#endif
}
