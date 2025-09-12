using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class PuzzleGoal : MonoBehaviour
{
    public bool requireExactBlock = false; // if true, checks block type by tag or component
    public string requiredBlockTag = "PushableBlock"; // optional tag match

    public bool IsOccupied { get; private set; } = false;
    public PushableBlock occupyingBlock;

    public event Action<PuzzleGoal, PushableBlock> OnOccupied;
    public event Action<PuzzleGoal> OnEmptied;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var block = other.GetComponent<PushableBlock>();
        if (block == null) return;

        if (requireExactBlock && !other.CompareTag(requiredBlockTag)) return;

        IsOccupied = true;
        occupyingBlock = block;
        OnOccupied?.Invoke(this, block);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var block = other.GetComponent<PushableBlock>();
        if (block == null) return;
        if (occupyingBlock == block)
        {
            IsOccupied = false;
            occupyingBlock = null;
            OnEmptied?.Invoke(this);
        }
    }
}
