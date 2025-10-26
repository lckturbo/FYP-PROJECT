using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;      // �e�L�X�g�{�b�N�X�̐e�iPanel�j
    [SerializeField] private TMP_Text nameText;          // �L�����N�^�[���\��
    [SerializeField] private TMP_Text dialogueText;      // ��b�e�L�X�g�\��

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed;  // �^�C�v���C�^�[���x

    private Queue<DialogueLine> linesQueue;             // ��b�s�����ԂɊi�[
    private Coroutine typingCoroutine;                  // �^�C�s���O�R���[�`��
    private bool isTyping = false;                      // �^�C�s���O���t���O
    private DialogueLine currentLine;                   // ���ݕ\�����̍s

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
    /// ��b�J�n
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
    /// ���̍s��\��
    /// </summary>
    public void DisplayNextLine()
    {
        if (isTyping)
        {
            // �^�C�s���O�r���Ȃ�S���\��
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
    /// �^�C�s���O���Ȃ�S���\��
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
    /// ��b�I��
    /// </summary>
    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        UIManager.instance.canPause = true;

        if (playerMovement != null)
            playerMovement.GetComponent<PlayerInput>().enabled = true;
    }


}
