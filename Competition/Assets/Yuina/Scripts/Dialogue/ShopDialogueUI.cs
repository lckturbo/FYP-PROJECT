using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button cancelButton;

    private NewPlayerMovement playerMovement;
    private PlayerInput playerInput;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
            playerInput = playerMovement.GetComponent<PlayerInput>();
    }
    private void Update()
    {
        playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
            playerInput = playerMovement.GetComponent<PlayerInput>();
    }

    public void Show()
    {
        dialogueText.text = "Welcome! What would you like to do?";
        dialoguePanel.SetActive(true);

        // Disable player movement during shop dialogue
        if (playerInput != null)
            playerInput.enabled = false;

        buyButton.onClick.RemoveAllListeners();
        sellButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // --- BUY ---
        buyButton.onClick.AddListener(() =>
        {
            dialoguePanel.SetActive(false);
            //EnableMovement();
            ShopManager.Instance.OpenShop(); // Open normal shop
        });

        // --- SELL ---
        sellButton.onClick.AddListener(() =>
        {
            dialoguePanel.SetActive(false);
            //EnableMovement();
            ShopManager.Instance.OpenSellMenu(); // Open sell menu
        });

        // --- CANCEL ---
        cancelButton.onClick.AddListener(() =>
        {
            dialoguePanel.SetActive(false);
            EnableMovement();
            Debug.Log("Player canceled shop interaction.");
        });
    }

    private void EnableMovement()
    {
        if (playerInput != null)
            playerInput.enabled = true;
    }

    public bool IsActive() => dialoguePanel != null && dialoguePanel.activeSelf;
}
