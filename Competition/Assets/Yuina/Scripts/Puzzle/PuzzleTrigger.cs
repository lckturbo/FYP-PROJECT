using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private PuzzleGameManager puzzleManager;

    private bool playerInRange = false;
    private NewPlayerMovement playerMovement;

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
            // STOP player movement
            if (playerMovement != null)
                playerMovement.enabled = false;

            // Start the puzzle
            puzzleManager.StartSequence();
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
