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
    [SerializeField] private float repeatDelay = 0.3f;
    [SerializeField] private float repeatRate = 0.1f;

    private Item currentItem;
    private int quantity = 1;
    private float nextChangeTime;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction submitAction;

    public bool IsPurchasePanelActive => gameObject.activeSelf;

    private void Awake()
    {
        // Get PlayerInput from NewPlayerMovement
        NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
        {
            playerInput = playerMovement.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                moveAction = playerInput.actions["Move"];
                moveAction.Enable();

                submitAction = playerInput.actions["Submit"];
                submitAction.Enable();
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

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (priceText != null) priceText.text = item.price.ToString() + " G";
        if (iconImage != null) iconImage.sprite = item.icon;
        if (descriptionText != null) descriptionText.text = item.description;

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ===============================
    // Quantity Control
    // ===============================
    private void OnEnable()
    {
        quantity = 1;
        UpdateQuantityText();
        nextChangeTime = 0f;
    }

    private void Update()
    {
        if (submitAction != null && submitAction.WasPressedThisFrame())
        {
            OnConfirm();
        }

        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input == Vector2.zero) return;

        if (Time.time >= nextChangeTime)
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
                nextChangeTime = Time.time + repeatRate;
            }
        }

        if (input != Vector2.zero && nextChangeTime == 0f)
        {
            nextChangeTime = Time.time + repeatDelay;
            ApplyImmediateChange(input);
        }
    }

    private void ApplyImmediateChange(Vector2 input)
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
            quantityText.text = "x" + quantity.ToString();
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
