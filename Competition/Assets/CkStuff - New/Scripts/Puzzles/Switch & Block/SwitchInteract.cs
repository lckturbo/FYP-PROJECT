using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SwitchInteract : MonoBehaviour
{
    [Header("Linking")]
    public SwitchChannelData channelData;
    public int channelIndex = 0;

    [Header("Feedback")]
    public Animator switchAnimator;

    [Header("Cinematic Camera")]
    public Transform blockFocus;
    public Animator blockAnimator;
    public Transform returnFocus;

    [Header("Timings")]
    public float panDuration = 0.6f;
    public float holdOnBlock = 0.2f;
    public float switchAnimFallback = 0.35f;
    public float blockAnimFallback = 0.45f;

    [Header("Spam Control")]
    public float cooldown = 0.25f;

    // runtime
    private bool _coolingDown;
    private int _resolvedChannel;
    private bool _playerInRange;

    private PlayerInput _playerInput;
    private NewPlayerMovement _playerMove;
    private InputAction _interactAction;

    private float _nextRebindCheckTime;

    private void Reset()
    {
        var col2d = GetComponent<Collider2D>();
        if (col2d) col2d.isTrigger = true;
    }

    private void Awake()
    {
        _resolvedChannel = (channelData != null) ? channelData.channelIndex : channelIndex;
        TryBindPlayerInput();
    }

    private void Update()
    {
        if (_interactAction == null && Time.unscaledTime >= _nextRebindCheckTime)
        {
            _nextRebindCheckTime = Time.unscaledTime + 0.25f;
            TryBindPlayerInput();
        }

        if (_playerInRange && _interactAction != null && _interactAction.WasPressedThisFrame())
        {
            Interact();
        }
    }

    private void OnEnable()
    {
        SubscribeAction();
    }

    private void OnDisable()
    {
        UnsubscribeAction();
        _playerInput = null;
        _playerMove = null;
        _interactAction = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<NewPlayerMovement>() != null)
            _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<NewPlayerMovement>() != null)
            _playerInRange = false;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_playerInRange) Interact();
    }

    public void Interact()
    {
        if (_coolingDown) return;
        StartCoroutine(UseSequence());
    }

    private IEnumerator UseSequence()
    {
        _coolingDown = true;
        SetPlayerControl(false);

        //TO DO IN ANIMATOR
        if (switchAnimator)
        {
            switchAnimator.ResetTrigger("Use");
            switchAnimator.SetTrigger("Use");
            yield return WaitForCurrentAnimOrFallback(switchAnimator, switchAnimFallback);
        }

        NewCameraController cam = FindObjectOfType<NewCameraController>();
        if (cam && blockFocus)
        {
            yield return cam.PanTo(blockFocus, panDuration);
            yield return new WaitForSeconds(panDuration);
        }

        SwitchBus.Toggle(_resolvedChannel);

        if (blockAnimator)
            yield return WaitForCurrentAnimOrFallback(blockAnimator, blockAnimFallback);
        else
            yield return new WaitForSeconds(blockAnimFallback);

        if (holdOnBlock > 0f)
            yield return new WaitForSeconds(holdOnBlock);

        if (cam)
        {
            yield return cam.ReturnToPlayer(panDuration);
            yield return new WaitForSeconds(panDuration);
        }

        SetPlayerControl(true);

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

            if (_playerInput != null)
            {
                UnsubscribeAction();

                _interactAction = _playerInput.actions != null ? _playerInput.actions["Interaction"] : null;
                if (_interactAction != null)
                {
                    _interactAction.Enable();
                    SubscribeAction();
                }

                if (!returnFocus) returnFocus = _playerMove.transform;
            }
        }
    }

    private void SubscribeAction()
    {
        if (_interactAction != null)
            _interactAction.performed += OnInteractPerformed;
    }

    private void UnsubscribeAction()
    {
        if (_interactAction != null)
            _interactAction.performed -= OnInteractPerformed;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (channelData != null) channelIndex = channelData.channelIndex;
        var col2d = GetComponent<Collider2D>();
        if (col2d) col2d.isTrigger = true;
    }
#endif
}
