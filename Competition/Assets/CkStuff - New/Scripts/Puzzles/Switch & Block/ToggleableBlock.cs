using UnityEngine;

[DisallowMultipleComponent]
public class ToggleableBlock : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string blockID;

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

    [Header("Visuals (SpriteRenderer or MeshRenderer)")]
    public Renderer[] visuals;

    private bool _isOpen;
    private int _resolvedChannel;

    private float openHideDelay = 0.25f;

    private void OnEnable()
    {
        _resolvedChannel = (channelData != null) ? channelData.channelIndex : channelIndex;
        if (useSwitchBus)
            SwitchBus.Subscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void Start()
    {
        if (!SaveLoadSystem.instance || SaveLoadSystem.instance.IsNewGame)
            SetOpenInternal(startOpen, instant: true);
    }

    private void OnDisable()
    {
        if (useSwitchBus)
            SwitchBus.Unsubscribe(_resolvedChannel, OnToggle, OnSetState);
    }

    private void OnToggle()
    {
        SetOpenInternal(!_isOpen, instant: false);
    }

    private void OnSetState(bool open)
    {
        SetOpenInternal(open, instant: false);
    }

    public void Open() => SetOpenInternal(true, instant: false);
    public void Close() => SetOpenInternal(false, instant: false);
    public void SetOpen(bool open) => SetOpenInternal(open, instant: false);

    private void SetOpenInternal(bool open, bool instant)
    {
        _isOpen = open;
        AudioManager.instance.PlaySFXAtPoint("irongate2-85045", transform.position);
        bool effectiveOpen = invert ? !open : open;
        ApplyState(effectiveOpen, instant);
    }

    private void ApplyState(bool effectiveOpen, bool instant)
    {
        if (blockColliders != null)
        {
            foreach (var col in blockColliders)
                if (col) col.enabled = !effectiveOpen;
        }

        if (visuals != null)
        {
            foreach (var v in visuals)
                if (v) v.enabled = true;
        }

        if (animator != null)
        {
            animator.ResetTrigger("OpenDoor");
            animator.ResetTrigger("CloseDoor");

            if (instant)
            {
                if (effectiveOpen)
                {
                    animator.Play("DoorOpen", 0, 1f);

                    if (visuals != null)
                    {
                        foreach (var v in visuals)
                            if (v) v.enabled = false;
                    }
                }
                else
                {
                    animator.Play("DoorClosed", 0, 1f);

                    if (visuals != null)
                    {
                        foreach (var v in visuals)
                            if (v) v.enabled = true;
                    }
                }
            }
            else
            {
                if (effectiveOpen)
                {
                    animator.SetTrigger("OpenDoor");

                    StopAllCoroutines();
                    StartCoroutine(HideAfterDelay(openHideDelay));
                }
                else
                {
                    if (visuals != null)
                    {
                        foreach (var v in visuals)
                            if (v) v.enabled = true;
                    }

                    animator.SetTrigger("CloseDoor");
                }
            }
        }
        else
        {
            if (visuals != null)
            {
                foreach (var v in visuals)
                    if (v) v.enabled = !effectiveOpen;
            }
        }
    }

    private System.Collections.IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (_isOpen && visuals != null)
        {
            foreach (var v in visuals)
                if (v) v.enabled = false;
        }
    }

    public void LoadFromSave(bool openState)
    {
        SetOpenInternal(openState, instant: true);
    }

    public void LoadData(GameData data)
    {
        var entry = data.savedBlocks.Find(b => b.id == blockID);
        if (entry != null)
        {
            LoadFromSave(entry.isOpen);
        }
        else
        {
            LoadFromSave(startOpen);
        }
    }

    public void SaveData(ref GameData data)
    {
        var existing = data.savedBlocks.Find(b => b.id == blockID);
        if (existing != null)
        {
            existing.isOpen = _isOpen;
        }
        else
        {
            data.savedBlocks.Add(new GameData.BlockSaveEntry
            {
                id = blockID,
                isOpen = _isOpen
            });
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (channelData != null) channelIndex = channelData.channelIndex;
    }
#endif
}
