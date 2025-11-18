using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private PuzzleGameManager puzzleManager;

    private bool playerInRange = false;
    private NewPlayerMovement playerMovement;

    private bool used = false;  // <<< NEW — single-use flag

    public Vector3 SpawnPosition => transform.position;

    private void Start()
    {
        playerMovement = FindObjectOfType<NewPlayerMovement>();
    }

    void Update()
    {
        if (used) return;                // <<< prevents re-use
        if (!playerInRange) return;
        playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerMovement != null)
                playerMovement.enabled = false;

            used = true;                // <<< mark as consumed

            puzzleManager.StartSequenceFromTrigger(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
