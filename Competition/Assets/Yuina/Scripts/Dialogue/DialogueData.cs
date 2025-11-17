using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Lines")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;   // Name of the speaker
    [TextArea(2, 5)]
    public string text;          // Dialogue text

    [Header("Portrait")]
    public Sprite portrait;     // The character image

    public enum Side { Left, Right }
    public Side portraitSide;   // Which side this line uses
}
