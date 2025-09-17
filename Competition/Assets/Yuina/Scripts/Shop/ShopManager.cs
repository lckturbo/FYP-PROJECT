using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using ISystem_Actions;  // Namespace for using InputSystem_Actions
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    private InputSystem_Actions iSystemActions;

    public static ShopManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;                 // ショップ全体のUI
    [SerializeField] private RectTransform contentTransform;    // Content (ScrollViewの中身)
    [SerializeField] private GameObject itemButtonPrefab;       // アイテムボタンPrefab
    [SerializeField] private TextMeshProUGUI moneyText;         // 所持金表示
    [SerializeField] private TextMeshProUGUI messageText;       // メッセージ表示
    [SerializeField] private ScrollRect scrollRect;

    [Header("Shop Items")]
    [SerializeField] private ShopItem[] shopItems;           // 販売アイテム一覧

    [Header("Panels")]
    [SerializeField] private PurchasePanel purchasePanel;   // 数量選択パネル

    private bool isOpen = false;

    public bool IsShopActive => shopUI.activeSelf;

    private void Awake()
    {
        iSystemActions = new InputSystem_Actions();
        iSystemActions.UI.Enable();

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (shopUI != null)
            shopUI.SetActive(false);

        if (purchasePanel != null)
            purchasePanel.gameObject.SetActive(false);

    }

    private void Start()
    {
        PopulateShop();
        UpdateMoneyUI();
        ClearMessage();
    }

    private void Update()
    {
        if (!isOpen) return;

        // Close the shop using the key corresponding to Cancel
        if (iSystemActions.UI.Cancel.WasPressedThisFrame())
        {
            if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
            {
                // If the PurchasePanel is open, close it instead.
                purchasePanel.Close();
                return; // The shop will not close.
            }
            else
            {
                // If there is no PurchasePanel, close the entire shop.
                CloseShop();
                return;
            }
        }

        // Cursor Control
        if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
        {
            // Disable cursor movement while the PurchasePanel is open
            EventSystem.current.sendNavigationEvents = false;
        }
        else
        {
            // Normally enabled
            EventSystem.current.sendNavigationEvents = true;

            // Prevent the selected item from moving outside the view
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(contentTransform))
            {
                ScrollToSelected(selected.GetComponent<RectTransform>());
            }
        }
    }

    /// <summary>
    /// Generate item button
    /// </summary>
    private void PopulateShop()
    {
        // Remove existing child elements
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // Generate a button for each item
        foreach (var item in shopItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, contentTransform);

            // Force all graphics ON
            Graphic[] graphics = buttonObj.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics) g.enabled = true;

            // Force all buttons ON
            Button[] buttons = buttonObj.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) b.enabled = true;

            // Retrieve and display the icon, name, and price.
            Transform iconTrans = buttonObj.transform.Find("Image");
            Transform nameTrans = buttonObj.transform.Find("Name");
            Transform priceTrans = buttonObj.transform.Find("Price");

            if (iconTrans != null && item.icon != null)
            {
                Image iconImage = iconTrans.GetComponent<Image>();
                if (iconImage != null)
                    iconImage.sprite = item.icon;
            }

            if (nameTrans != null)
            {
                TextMeshProUGUI nameText = nameTrans.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = item.itemName;
            }

            if (priceTrans != null)
            {
                TextMeshProUGUI priceText = priceTrans.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                    priceText.text = item.price + " G";
            }

            // ボタンに購入処理を登録
            Button buttonComp = buttonObj.GetComponentInChildren<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() => BuyItem(item));
            }
        }
    }

    /// <summary>
    /// Fit the selected UI within the ScrollRect
    /// </summary>
    private void ScrollToSelected(RectTransform target)
    {
        // Get the Content and Viewport
        RectTransform viewport = scrollRect.viewport;

        // Convert to viewport coordinates
        Vector3 childLocalPos = viewport.InverseTransformPoint(target.position);

        // Calculate the top and bottom edges of the button
        float childTop = childLocalPos.y + target.rect.height * 0.1f;
        float childBottom = childLocalPos.y - target.rect.height * 0.1f;

        // Current scroll position
        float normalizedPos = scrollRect.verticalNormalizedPosition;

        // If it extends beyond the top, scroll up.
        if (childTop > viewport.rect.height * 0.1f)
        {
            normalizedPos += 0.05f; // Scroll speed
        }
        // If it extends below, scroll down
        else if (childBottom < -viewport.rect.height * 0.1f)
        {
            normalizedPos -= 0.05f;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }

    /// <summary>
    /// Item Purchase Processing
    /// </summary>
    public void BuyItem(ShopItem item)
    {
        // Items that cannot be held in multiple quantities cannot be purchased if already held.
        if (item.type == ItemType.Unique && PlayerInventory.Instance.HasItem(item))
        {
            ShowMessage("You already have this Unique Item!");
            return;
        }

        purchasePanel.Open(item);
    }

    /// <summary>
    /// Actual purchase process (called from PurchasePanel)
    /// </summary>
    public void PurchaseItem(ShopItem item, int quantity)
    {
        int totalCost = item.price * quantity;

        // Confirm that you are not attempting to purchase two or more of the same unique item.
        if (item.type == ItemType.Unique && quantity > 1)
        {
            ShowMessage("You cannot purchase more than two Unique Items!");
            return;
        }

        // Check if you have enough money
        if (PlayerInventory.Instance.Money < totalCost)
        {
            ShowMessage("Not enough money!");
            return;
        }

        // Payment
        if (PlayerInventory.Instance.TrySpendMoney(totalCost))
        {
            for (int i = 0; i < quantity; i++)
            {
                PlayerInventory.Instance.AddItem(item);
            }

            ShowMessage($"Purchased {quantity} x {item.itemName}!");
            UpdateMoneyUI();
        }
    }


    /// <summary>
    /// Cash Balance UI Update
    /// </summary>
    private void UpdateMoneyUI()
    {
        moneyText.text = $"Money: {PlayerInventory.Instance.Money} G";
    }

    /// <summary>
    /// Message Display
    /// </summary>
    private void ShowMessage(string msg)
    {
        messageText.text = msg;
    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }

    public void OpenShop()
    {
        if (shopUI != null)
            shopUI.SetActive(true);

        isOpen = true;
        UpdateMoneyUI();
        ClearMessage();
        PlayerController.Instance.SetCanMove(false); // Player Stop
    }

    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.SetActive(false);

        isOpen = false;
        ClearMessage();
        PlayerController.Instance.SetCanMove(true); // Player can move
    }

}
