using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResetPlatform : MonoBehaviour
{
    [Header("Reset Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float resetDelay = 0.2f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            triggered = true;
            StartCoroutine(ResetAllBlocksAfterDelay());
        }
    }

    private System.Collections.IEnumerator ResetAllBlocksAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        PushableBlock[] allBlocks = FindObjectsOfType<PushableBlock>();
        foreach (var block in allBlocks)
        {
            block.ResetToOriginal();
        }

        triggered = false;
    }
}
