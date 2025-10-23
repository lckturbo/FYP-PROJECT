using UnityEngine;

[DisallowMultipleComponent]
public class ToggleableBlock : MonoBehaviour
{
    [Header("Linking")]
    public SwitchChannelData channelData;
    public int channelIndex = 0;

    [Header("State")]
    public bool startOpen = false;

    [Tooltip("If true, inverts the meaning: OPEN will behave as CLOSED.")]
    public bool invert = false;

    [Header("Animation")]
    public Animator animator;

    [Header("Collider")]
    public Collider2D[] blockColliders;

    [Header("Visuals")]
    public Renderer[] visuals;

    private bool _isOpen;
    private int _resolvedChannel;

    private void OnEnable()
    {
        _resolvedChannel = (channelData != null) ? channelData.channelIndex : channelIndex;
        SwitchBus.Subscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void Start()
    {
        SetOpen(startOpen, applyInvert: true);
    }

    private void OnDisable()
    {
        SwitchBus.Unsubscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void OnToggle()
    {
        SetOpen(!_isOpen, applyInvert: false);
    }

    private void OnSetState(bool open)
    {
        SetOpen(open, applyInvert: false);
    }

    private void SetOpen(bool open, bool applyInvert)
    {
        _isOpen = open;
        bool effectiveOpen = invert ? !open : open;
        ApplyState(effectiveOpen);
    }

    private void ApplyState(bool effectiveOpen)
    {
        // Colliders enabled when CLOSED (so they block), disabled when OPEN
        if (blockColliders != null)
        {
            for (int i = 0; i < blockColliders.Length; i++)
            {
                if (blockColliders[i])
                    blockColliders[i].enabled = !effectiveOpen;
            }
        }

        //visible when CLOSED by default; hidden when OPEN
        if (visuals != null)
        {
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i])
                    visuals[i].enabled = !effectiveOpen;
            }
        }

        // Animator triggers
        if (animator)
        {
            if (effectiveOpen) animator.SetTrigger("Open");
            else animator.SetTrigger("Close");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (channelData != null) channelIndex = channelData.channelIndex;
    }
#endif
}
