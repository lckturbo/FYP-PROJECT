using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    private System.Action onDialogueEndCallback;

    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonA;
    [SerializeField] private Button choiceButtonB;

    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;

    private bool waitingForChoice = false;


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

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Allow ANY key OR left mouse click to continue
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            DisplayNextLine();
        }
    }


    /// <summary>
    /// 会話開始
    /// </summary>
    public void StartDialogue(DialogueData dialogueData, System.Action onEnd = null)
    {
        dialogueUI.SetActive(true);
        UIManager.instance.canPause = false;

        onDialogueEndCallback = onEnd;

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
        if (waitingForChoice) return; // prevent skipping during choices

        if (isTyping)
        {
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
        UpdatePortrait(currentLine);

        // If this line has choices  show choice UI instead of typing.
        if (currentLine.hasChoices)
        {
            ShowChoices(currentLine);
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLine.text));
    }


    private void ShowChoices(DialogueLine line)
    {
        waitingForChoice = true;

        choicePanel.SetActive(true);

        choiceAText.text = line.choiceA.text;
        choiceBText.text = line.choiceB.text;

        // Remove old listeners
        choiceButtonA.onClick.RemoveAllListeners();
        choiceButtonB.onClick.RemoveAllListeners();

        // Add new listeners
        choiceButtonA.onClick.AddListener(() => ChooseBranch(line.choiceA.nextDialogue));
        choiceButtonB.onClick.AddListener(() => ChooseBranch(line.choiceB.nextDialogue));
    }

    private void ChooseBranch(DialogueData nextDialogue)
    {
        choicePanel.SetActive(false);
        waitingForChoice = false;

        // Branch into the selected dialogue
        StartDialogue(nextDialogue, onDialogueEndCallback);
    }


    private void UpdatePortrait(DialogueLine line)
    {
        // Hide both first
        leftPortraitImage.gameObject.SetActive(false);
        rightPortraitImage.gameObject.SetActive(false);

        if (line.portrait == null) return;

        if (line.portraitSide == DialogueLine.Side.Left)
        {
            leftPortraitImage.sprite = line.portrait;
            leftPortraitImage.gameObject.SetActive(true);
        }
        else
        {
            rightPortraitImage.sprite = line.portrait;
            rightPortraitImage.gameObject.SetActive(true);
        }
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

        onDialogueEndCallback?.Invoke();  // <-- DO THE NEXT ACTION (enter battle)
        onDialogueEndCallback = null;
    }

}
