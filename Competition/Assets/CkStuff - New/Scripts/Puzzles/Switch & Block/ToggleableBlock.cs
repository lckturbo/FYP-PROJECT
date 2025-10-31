using UnityEngine;

[DisallowMultipleComponent]
public class ToggleableBlock : MonoBehaviour
{
    [Header("Linking")]
    public SwitchChannelData channelData;
    public int channelIndex = 0;

    [Header("Input Sources")]
    public bool useSwitchBus = true;

    [Header("State")]
    public bool startOpen = false;

    [Tooltip("If true, inverts the meaning: OPEN behaves as CLOSED.")]
    public bool invert = false;

    [Header("Animation")]
    public Animator animator;

    [Header("Collider (blocks the player when CLOSED)")]
    public Collider2D[] blockColliders;

    [Header("Visuals (hide when OPEN)")]
    public Renderer[] visuals;

    private bool _isOpen;
    public bool IsOpen => _isOpen;
    private int _resolvedChannel;

    private void OnEnable()
    {
        _resolvedChannel = (channelData != null) ? channelData.channelIndex : channelIndex;
        if (useSwitchBus)
            SwitchBus.Subscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void Start()
    {
        if (!SaveLoadSystem.instance || SaveLoadSystem.instance.IsNewGame)
            SetOpenInternal(startOpen, applyInvert: true);
    }

    private void OnDisable()
    {
        if (useSwitchBus)
            SwitchBus.Unsubscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void OnToggle()
    {
        SetOpenInternal(!_isOpen, applyInvert: false);
    }

    private void OnSetState(bool open)
    {
        SetOpenInternal(open, applyInvert: false);
    }

    public void Open()
    {
        SetOpenInternal(true, applyInvert: false);
    }

    public void Close()
    {
        SetOpenInternal(false, applyInvert: false);
    }

    public void SetOpen(bool open)
    {
        SetOpenInternal(open, applyInvert: false);
    }

    // ===== Core =====
    private void SetOpenInternal(bool open, bool applyInvert)
    {
        _isOpen = open;
        bool effectiveOpen = invert ? !open : open;
        ApplyState(effectiveOpen);
    }

    private void ApplyState(bool effectiveOpen)
    {
        if (blockColliders != null)
        {
            for (int i = 0; i < blockColliders.Length; i++)
            {
                if (blockColliders[i] != null)
                    blockColliders[i].enabled = !effectiveOpen;
            }
        }

        if (visuals != null)
        {
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] != null)
                    visuals[i].enabled = !effectiveOpen;
            }
        }

        if (animator != null)
        {
            if (effectiveOpen) animator.SetTrigger("Open");
            else animator.SetTrigger("Close");
        }
    }
    public void LoadFromSave(bool openState)
    {
        _isOpen = openState;
        bool effectiveOpen = invert ? !openState : openState;
        ApplyState(effectiveOpen);
    }

    //public void LoadData(GameData data)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void SaveData(ref GameData data)
    //{
    //    throw new System.NotImplementedException();
    //}

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (channelData != null) channelIndex = channelData.channelIndex;
    }
#endif
}
