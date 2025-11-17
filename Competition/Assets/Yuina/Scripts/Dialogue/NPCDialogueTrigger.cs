using UnityEngine;
using System.Collections;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    private bool playerInRange = false;

    private bool Dialogiscomplete = false;

    void Update()
    {
        //if (InteractionLock.IsLocked) return; // block all interaction

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && Dialogiscomplete == false)
        {
            Debug.Log("is in range");
            if (!DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.StartDialogue(dialogue);
                Dialogiscomplete = true;
            }
            else
            {
                //DialogueManager.Instance.DisplayNextLine();
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
