using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using ISystem_Actions;  // Namespace for using InputSystem_Actions

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    private bool playerInRange = false;

    private InputSystem_Actions iSystemActions;

    void Awake()
    {
        iSystemActions = new InputSystem_Actions();
        iSystemActions.Player.Enable();
    }


    void Update()
    {
        if (playerInRange && iSystemActions.Player.Interact.WasPressedThisFrame())
        {
            if (!DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.StartDialogue(dialogue);
            }
            else
            {
                DialogueManager.Instance.DisplayNextLine();
            }
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
