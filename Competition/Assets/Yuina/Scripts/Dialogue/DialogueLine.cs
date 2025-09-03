using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;   // キャラクター名
    [TextArea(2, 5)]
    public string text;          // セリフ
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
//Renaming a text file after creation will corrupt it, so please think carefully about the name before creating it.

public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines; // ページ単位でセリフを格納
}
