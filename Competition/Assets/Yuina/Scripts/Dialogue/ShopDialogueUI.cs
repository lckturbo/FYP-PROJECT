using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private System.Action onConfirm;
    private System.Action onCancel;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void Show(System.Action confirmAction, System.Action cancelAction)
    {
        onConfirm = confirmAction;
        onCancel = cancelAction;

        dialogueText.text = "Do you want to browse the shop?";
        dialoguePanel.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            dialoguePanel.SetActive(false);
            onConfirm?.Invoke();
        });

        noButton.onClick.AddListener(() =>
        {
            dialoguePanel.SetActive(false);
            onCancel?.Invoke();
        });
    }

    public bool IsActive() => dialoguePanel != null && dialoguePanel.activeSelf;
}
