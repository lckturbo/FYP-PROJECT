using UnityEngine;
using static GameData;

public class AreaDialogTrigger : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;
    [SerializeField] private DialogueData dialogue;

    private bool playerInRange = false;

    private bool Dialogiscomplete = false;

    [SerializeField]private GameObject MiniMapUi;

    private bool hasExitArea = false;

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
        if (!hasExitArea)
        {
            if (other.CompareTag("Player"))
                playerInRange = true;
            if (MiniMapUi != null)
                MiniMapUi.gameObject.SetActive(false);
            hasExitArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasExitArea)
        {
            if (other.CompareTag("Player"))
                playerInRange = false;
            if(MiniMapUi != null)
            MiniMapUi.gameObject.SetActive(true);

        }
    }

    public void LoadData(GameData data)
    {
        foreach (var state in data.dialogueTriggerStates)
        {
            if (state.id == id)
            {
                Dialogiscomplete = state.completed;

                if (Dialogiscomplete)
                    this.enabled = false;

                return;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        foreach (var state in data.dialogueTriggerStates)
        {
            if (state.id == id)
            {
                state.completed = Dialogiscomplete;
                return;
            }
        }

        data.dialogueTriggerStates.Add(new DialogueTriggerState(id, Dialogiscomplete));
    }
}
