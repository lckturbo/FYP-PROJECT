using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private PuzzleGameManager puzzleManager;

    private bool playerInRange = false;
    private NewPlayerMovement playerMovement;

    public Vector3 SpawnPosition => transform.position;  // << Add this

    private void Start()
    {
        playerMovement = FindObjectOfType<NewPlayerMovement>();
    }

    void Update()
    {
        if (!playerInRange) return;
        playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerMovement != null)
                playerMovement.enabled = false;

            // Pass this trigger to manager
            puzzleManager.StartSequenceFromTrigger(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
