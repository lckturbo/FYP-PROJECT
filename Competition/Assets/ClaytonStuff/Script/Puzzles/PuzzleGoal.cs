using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class PuzzleGoal : MonoBehaviour
{
    [Header("Block Requirements")]
    public bool requireExactBlock = false;         // If true, must match blockName
    public string requiredBlockName = "PushableBlock"; // Must match GameObject.name

    public bool IsOccupied { get; private set; } = false;
    public PushableBlock occupyingBlock;

    public event Action<PuzzleGoal, PushableBlock> OnOccupied;
    public event Action<PuzzleGoal> OnEmptied;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var block = other.GetComponent<PushableBlock>();
        if (block == null) return;

        // If requirement is enabled, check by GameObject name
        if (requireExactBlock && !other.gameObject.name.Equals(requiredBlockName, StringComparison.OrdinalIgnoreCase))
            return;

        // If already occupied by this block, ignore
        if (occupyingBlock == block) return;

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
