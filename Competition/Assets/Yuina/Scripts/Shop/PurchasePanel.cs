using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using ISystem_Actions;  // Namespace for using InputSystem_Actions


public class PurchasePanel : MonoBehaviour
{
    private InputAction moveAction;
    private InputSystem_Actions iSystemActions;


    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Repeat Settings")]
    [SerializeField] private float repeatDelay = 0.3f;   // 長押し開始までの遅延
    [SerializeField] private float repeatRate = 0.1f;    // 長押し中の変化速度

    private ShopItem currentItem;
    private int quantity = 1;
    private float nextChangeTime;
    private Vector2 navInput;

    public bool IsPurchasePanelActive => gameObject.activeSelf; // 作ってみた。使える？


    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();

        iSystemActions = new InputSystem_Actions();
        iSystemActions.UI.Enable();


        gameObject.SetActive(false);
    }

    // ===============================
    // Open / Close
    // ===============================
    public void Open(ShopItem item)
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
    // 数量変更
    // ===============================
    private void OnEnable()
    {
        quantity = 1;
        UpdateQuantityText();
        nextChangeTime = 0f;
    }

    private void Update()
    {
        if (iSystemActions.UI.Submit.WasPressedThisFrame())
        {
            OnConfirm();
        }


        // Moveの入力を取得
        Vector2 input = moveAction.ReadValue<Vector2>();

        if (input == Vector2.zero) return;

        // リピート判定
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

        // 長押し開始までの猶予
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
            // ShopManager に購入確定を通知
            ShopManager.Instance.PurchaseItem(currentItem, quantity);
        }
        Close();
    }

    public void OnCancel()
    {
        Close();
    }
}
