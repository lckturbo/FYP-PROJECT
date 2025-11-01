using UnityEngine;

public class ChessClues : MonoBehaviour
{
    [SerializeField] private int signIndex;

    public void Interact()
    {
        if (!PieceManager.instance) return;

        var activeSolution = PieceManager.instance.GetActiveSolution();
        if (activeSolution == null || activeSolution.clues == null) return;

        DialogueData dialogue = signIndex switch
        {
            0 => activeSolution.clues.sign1Clue,
            1 => activeSolution.clues.sign2Clue,
            2 => activeSolution.clues.sign3Clue,
            _ => null
        };

        if (!DialogueManager.Instance.IsDialogueActive)
            DialogueManager.Instance.StartDialogue(dialogue);
        else
            DialogueManager.Instance.DisplayNextLine();
    }
}
