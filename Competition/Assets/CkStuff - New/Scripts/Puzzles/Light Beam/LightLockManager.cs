using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LightLockManager : MonoBehaviour
{
    public List<BeamReceiver> receivers = new();
    public ToggleableBlock doorBlock;   // or:
    public Animator doorAnimator;
    public bool latchOpen = true;       // keep open once solved

    private bool opened;

    private void OnEnable()
    {
        foreach (var r in receivers)
        {
            if (!r) continue;
            r.OnActivated += OnChanged;
            r.OnDeactivated += OnChanged;
        }
    }
    private void OnDisable()
    {
        foreach (var r in receivers)
        {
            if (!r) continue;
            r.OnActivated -= OnChanged;
            r.OnDeactivated -= OnChanged;
        }
    }

    private void Start() => Evaluate();

    private void OnChanged(BeamReceiver _) => Evaluate();

    private void Evaluate()
    {
        for (int i = 0; i < receivers.Count; i++)
        {
            if (!receivers[i] || !receivers[i].IsActive)
            {
                if (!latchOpen) CloseDoor();
                return;
            }
        }
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (opened && latchOpen) return;
        opened = true;
        if (doorBlock) doorBlock.SendMessage("SetOpen", true, SendMessageOptions.DontRequireReceiver);
        if (doorAnimator) doorAnimator.SetTrigger("Open");
    }
    private void CloseDoor()
    {
        if (!opened || latchOpen) return;
        opened = false;
        if (doorBlock) doorBlock.SendMessage("SetOpen", false, SendMessageOptions.DontRequireReceiver);
        if (doorAnimator) doorAnimator.SetTrigger("Close");
    }
}
