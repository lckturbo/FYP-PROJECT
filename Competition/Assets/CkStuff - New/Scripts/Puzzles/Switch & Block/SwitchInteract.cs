using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SwitchInteract : MonoBehaviour, IDataPersistence
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
    private bool _isActivated;    // false = OFF (left), true = ON (right)

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
        if (!returnFocus) returnFocus = transform;
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
        if (SaveLoadSystem.instance != null)
            SaveLoadSystem.instance.RegisterDataPersistenceObjects(this);

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

        NewCameraController cam = FindObjectOfType<NewCameraController>();

        Vector3 switchPos = (returnFocus ? returnFocus.position : transform.position);
        Transform switchAnchor = CreateTempAnchor(switchPos, "CamTemp_Switch");

        Transform blockAnchor = null;
        if (blockFocus)
            blockAnchor = CreateTempAnchor(blockFocus.position, "CamTemp_Block");

        // cache current state BEFORE toggling
        bool turningOn = !_isActivated;   // false -> true  (OFF -> ON)  or true -> false (ON -> OFF)

        try
        {
            // 1) SHOW SWITCH ANIM (camera settles on switch)
            if (cam && switchAnchor)
                yield return cam.PanTo(switchAnchor, panDuration);

            if (switchAnimator)
            {
                // clear both direction triggers first
                switchAnimator.ResetTrigger("UseOn");
                switchAnimator.ResetTrigger("UseOff");

                // choose which flip animation to play
                if (turningOn)
                {
                    // OFF -> ON (flip to the right)
                    switchAnimator.SetTrigger("UseOn");
                }
                else
                {
                    // ON -> OFF (flip back to the left)
                    switchAnimator.SetTrigger("UseOff");
                }

                yield return WaitForCurrentAnimOrFallback(switchAnimator, switchAnimFallback);
            }

            // 2) PAN TO BLOCK
            if (cam && blockAnchor)
                yield return cam.PanTo(blockAnchor, panDuration);

            // 3) Toggle AFTER we've framed the block (target can safely disable now)
            SwitchBus.Toggle(_resolvedChannel);
            _isActivated = !_isActivated;

            // 4) Wait for block anim or fallback, then optional hold
            if (blockAnimator)
                yield return WaitForCurrentAnimOrFallback(blockAnimator, blockAnimFallback);
            else
                yield return new WaitForSeconds(blockAnimFallback);

            if (holdOnBlock > 0f)
                yield return new WaitForSeconds(holdOnBlock);

            // 5) PAN BACK TO SWITCH
            if (cam && switchAnchor)
                yield return cam.PanTo(switchAnchor, panDuration);

            // 6) RETURN / RECONNECT TO PLAYER
            if (cam)
                yield return cam.ReturnToPlayer(panDuration);
        }
        finally
        {
            if (switchAnchor) Destroy(switchAnchor.gameObject);
            if (blockAnchor) Destroy(blockAnchor.gameObject);
            SetPlayerControl(true);
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

    private Transform CreateTempAnchor(Vector3 worldPos, string name)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        return go.transform;
    }

    public void SaveData(ref GameData data)
    {
        bool found = false;
        foreach (var state in data.switchChannelStates)
        {
            if (state.channelIndex == _resolvedChannel)
            {
                state.isActivated = _isActivated;
                found = true;
                break;
            }
        }

        if (!found)
            data.switchChannelStates.Add(new GameData.ChannelState
            {
                channelIndex = _resolvedChannel,
                isActivated = _isActivated
            });
    }

    public void LoadData(GameData data)
    {
        foreach (var state in data.switchChannelStates)
        {
            if (state.channelIndex == _resolvedChannel)
            {
                _isActivated = state.isActivated;
                SwitchBus.SetState(_resolvedChannel, _isActivated);
                break;
            }
        }

        foreach (var block in FindObjectsOfType<ToggleableBlock>())
        {
            if (!block.useSwitchBus) continue;

            foreach (var state in data.switchChannelStates)
            {
                if (state.channelIndex == block.channelIndex)
                {
                    block.LoadFromSave(state.isActivated);
                    break;
                }
            }
        }

        if (switchAnimator)
        {
            switchAnimator.ResetTrigger("UseOn");
            switchAnimator.ResetTrigger("UseOff");
            switchAnimator.ResetTrigger("On");
            switchAnimator.ResetTrigger("Off");

            if (_isActivated)
                switchAnimator.Play("On", 0, 1f); 
            else
                switchAnimator.Play("Off", 0, 1f);
        }
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
