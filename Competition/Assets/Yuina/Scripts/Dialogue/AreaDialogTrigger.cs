using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDialogTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    private bool playerInRange = false;

    private bool Dialogiscomplete = false;

    [SerializeField]private GameObject MiniMapUi;

    private void Update()
    {
        if(playerInRange && Dialogiscomplete == false)
        {
            if (!DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.StartDialogue(dialogue);
                Dialogiscomplete = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
        MiniMapUi.gameObject.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
        MiniMapUi.gameObject.SetActive(true);
    }
}
