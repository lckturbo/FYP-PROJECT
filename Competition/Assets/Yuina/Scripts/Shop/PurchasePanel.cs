using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PurchasePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Repeat Settings")]
    [SerializeField] private float repeatDelay = 0.3f;   // delay before repeating
    [SerializeField] private float repeatRate = 0.1f;    // speed of repeat when held

    private Item currentItem;
    private int quantity = 1;

    private float nextChangeTime;
    private bool inputHeld;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    [Header("Confirm UI")]
    [SerializeField] private GameObject confirmButton; // Button shown with panel

    public bool IsPurchasePanelActive => gameObject.activeSelf;

    private void Awake()
    {
        // Find PlayerInput via NewPlayerMovement
        NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
        {
            playerInput = playerMovement.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                moveAction = playerInput.actions["Move"];
                submitAction = playerInput.actions["Submit"];
                cancelAction = playerInput.actions["Cancel"];

                moveAction?.Enable();
                submitAction?.Enable();
                cancelAction?.Enable();
            }
        }

        gameObject.SetActive(false);
    }

    // ===============================
    // Open / Close
    // ===============================
    public void Open(Item item)
    {
        currentItem = item;
        quantity = 1;
        UpdateQuantityText();

        if (itemNameText) itemNameText.text = item.itemName;
        if (priceText) priceText.text = item.price + " G";
        if (iconImage && item.icon) iconImage.sprite = item.icon;
        if (descriptionText) descriptionText.text = item.description;

        gameObject.SetActive(true);

        if (confirmButton != null)
            confirmButton.SetActive(true); // show confirm button
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (confirmButton != null)
            confirmButton.SetActive(false); // hide confirm button
        currentItem = null;
    }


    // ===============================
    // Quantity Control
    // ===============================
    private void OnEnable()
    {
        quantity = 1;
        UpdateQuantityText();
        nextChangeTime = 0f;
        inputHeld = false;
    }

    private void Update()
    {
        if (submitAction != null && submitAction.WasPressedThisFrame())
        {
            OnConfirm();
        }

        if (cancelAction != null && cancelAction.WasPressedThisFrame())
        {
            OnCancel();
        }

        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        HandleQuantityInput(input);
    }

    private void HandleQuantityInput(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            inputHeld = false;
            nextChangeTime = 0f;
            return;
        }

        if (!inputHeld)
        {
            ApplyChange(input);
            inputHeld = true;
            nextChangeTime = Time.time + repeatDelay;
        }
        else if (Time.time >= nextChangeTime)
        {
            ApplyChange(input);
            nextChangeTime = Time.time + repeatRate;
        }
    }

    private void ApplyChange(Vector2 input)
    {
        int change = 0;

        if (input.y > 0.5f) change = +1;
        else if (input.y < -0.5f) change = -1;
        else if (input.x > 0.5f) change = +10;
        else if (input.x < -0.5f) change = -10;

        if (change != 0)
        {
            quantity = Mathf.Max(1, quantity + change);
            UpdateQuantityText();
        }
    }

    private void UpdateQuantityText()
    {
        if (quantityText != null)
            quantityText.text = "x" + quantity;
    }

    // ===============================
    // Confirm / Cancel
    // ===============================
    public void OnConfirm()
    {
        if (currentItem != null)
        {
            ShopManager.Instance.PurchaseItem(currentItem, quantity);
        }
        Close();
    }

    public void OnCancel()
    {
        Close();
    }
}
