using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;      // テキストボックスの親（Panel）
    [SerializeField] private TMP_Text nameText;          // キャラクター名表示
    [SerializeField] private TMP_Text dialogueText;      // 会話テキスト表示

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed;  // タイプライター速度

    private Queue<DialogueLine> linesQueue;             // 会話行を順番に格納
    private Coroutine typingCoroutine;                  // タイピングコルーチン
    private bool isTyping = false;                      // タイピング中フラグ
    private DialogueLine currentLine;                   // 現在表示中の行

    public bool IsDialogueActive => dialogueUI.activeSelf;
    public static bool IsDialogueActiveGlobal => Instance != null && Instance.IsDialogueActive;

    private NewPlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = FindObjectOfType<NewPlayerMovement>();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        linesQueue = new Queue<DialogueLine>();
        dialogueUI.SetActive(false);
    }

    /// <summary>
    /// 会話開始
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        dialogueUI.SetActive(true);
        UIManager.instance.canPause = false;

        if (playerMovement != null)
            playerMovement.GetComponent<PlayerInput>().enabled = false;

        linesQueue.Clear();
        foreach (var line in dialogueData.lines)
            linesQueue.Enqueue(line);

        DisplayNextLine();
    }

    /// <summary>
    /// 次の行を表示
    /// </summary>
    public void DisplayNextLine()
    {
        if (isTyping)
        {
            // タイピング途中なら全文表示
            CompleteCurrentLine();
            return;
        }

        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = linesQueue.Dequeue();
        nameText.text = currentLine.speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// タイピング中なら全文表示
    /// </summary>
    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialogueText.text = currentLine.text;
        isTyping = false;
    }

    /// <summary>
    /// 会話終了
    /// </summary>
    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        UIManager.instance.canPause = true;

        if (playerMovement != null)
            playerMovement.GetComponent<PlayerInput>().enabled = true;
    }


}
