using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class CartEntry
{
    public Item item;
    public int quantity;


    public CartEntry(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Shop Items")]
    [SerializeField] private Item[] shopItems;

    [Header("Panels")]
    [SerializeField] private PurchasePanel purchasePanel;

    private bool isOpen = false;
    private List<CartEntry> cart = new();

    private PlayerInput playerInput;
    private InputAction cancelAction;
    public bool IsShopActive => isOpen;

    // Reference to Inventory
    private Inventory playerInventory;
    private InventoryUIManager inventoryUIManager;

    private Coroutine messageRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (shopUI != null)
            shopUI.SetActive(false);

        if (purchasePanel != null)
            purchasePanel.gameObject.SetActive(false);

        // Get PlayerInput & Inventory
        NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
        {
            playerInput = playerMovement.GetComponent<PlayerInput>();
            playerInventory = playerMovement.GetComponent<Inventory>(); //  attach Inventory to player
            inventoryUIManager = playerMovement.GetComponent<InventoryUIManager>();

            if (playerInput != null)
                cancelAction = playerInput.actions["Interaction"];
        }

        AssignInventory();

    }

    private void Start()
    {
        PopulateShop();
        UpdateMoneyUI();
        ClearMessage();
    }

    private void Update()
    {

        if (!isOpen) return; // only run update logic when shop is open

        if (cancelAction != null && cancelAction.WasPressedThisFrame())
        {
            if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
            {
                purchasePanel.Close();
                return;
            }
            else
            {
                //ToggleShop();
                return;
            }
        }

        if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
        {
            EventSystem.current.sendNavigationEvents = false;
        }
        else
        {
            EventSystem.current.sendNavigationEvents = true;

            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(contentTransform))
            {
                ScrollToSelected(selected.GetComponent<RectTransform>());
            }
        }
    }


    /// <summary>
    /// アイテムボタンを生成
    /// </summary>

    private void AssignInventory()
    {
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
            {
                Debug.Log($"[ShopManager] Linked to inventory on {playerInventory.gameObject.name}");
            }
            else
            {
                Debug.LogError("[ShopManager] No Inventory found in the scene!");
            }
        }

        // Optional: also hook up UI manager if it isn稚 on the player anymore
        if (inventoryUIManager == null)
        {
            inventoryUIManager = FindObjectOfType<InventoryUIManager>();
        }
    }
    private void PopulateShop()
    {
        // 既存の子要素を削除
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // アイテムごとにボタンを生成
        foreach (var item in shopItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, contentTransform);

            // 全Graphicを強制ON
            Graphic[] graphics = buttonObj.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics) g.enabled = true;

            // 全Buttonを強制ON
            Button[] buttons = buttonObj.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) b.enabled = true;

            // アイコン、名前、値段を取得して反映
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
                buttonComp.onClick.AddListener(() => PurchaseItem(item, 1));

            }
        }
    }

    /// <summary>
    /// 選択されたUIをScrollRect内に収める
    /// </summary>
    private void ScrollToSelected(RectTransform target)
    {
        // ContentとViewportを取得
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        // ビューポート座標に変換
        Vector3 viewportLocalPos = viewport.InverseTransformPoint(viewport.position);
        Vector3 childLocalPos = viewport.InverseTransformPoint(target.position);

        float viewportHeight = viewport.rect.height;
        float contentHeight = content.rect.height;

        // ボタンの上端・下端を計算
        float childTop = childLocalPos.y + target.rect.height * 0.1f;
        float childBottom = childLocalPos.y - target.rect.height * 0.1f;

        // 現在のスクロール位置
        float normalizedPos = scrollRect.verticalNormalizedPosition;

        // 上にはみ出したら上にスクロール
        if (childTop > viewport.rect.height * 0.1f)
        {
            normalizedPos += 0.05f; // スクロール速度（調整可）
        }
        // 下にはみ出したら下にスクロール
        else if (childBottom < -viewport.rect.height * 0.1f)
        {
            normalizedPos -= 0.05f;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }

    /// <summary>
    /// アイテム購入処理
    /// </summary>
    public void BuyItem(Item item)
    {
        // 複数所持不可アイテムは所持済みなら購入不可
        if (item.type == ItemTypes.Unique /*&& PlayerInventory.Instance.HasItem(item)*/)
        {
            ShowMessage("You already have this Unique Item!");
            return;
        }

        // 数量選択パネルを開く
        purchasePanel.Open(item);
    }

    /// <summary>
    /// カートまとめて購入
    /// </summary>
    public void OnBuyButtonPressed()
    {
        int totalCost = 0;
        foreach (var entry in cart)
            totalCost += entry.item.price * entry.quantity;

        if (totalCost == 0)
        {
            ShowMessage("Cart is empty!");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("No player inventory found!");
            return;
        }

        if (playerInventory.Money < totalCost)
        {
            ShowMessage("Not enough money!");
            return;
        }

        if (playerInventory.TrySpendMoney(totalCost))
        {
            foreach (var entry in cart)
                playerInventory.AddItem(entry.item, entry.quantity);

            ShowMessage("Purchase successful!");
            UpdateMoneyUI();
            cart.Clear();
        }
    }

    /// <summary>
    /// 実際の購入処理（PurchasePanelから呼ばれる）
    /// </summary>
    public void PurchaseItem(Item item, int quantity)
    {
        int totalCost = item.price * quantity;

        if (item.type == ItemTypes.Unique && playerInventory.HasItem(item))
        {
            ShowMessage("You already own this Unique Item!");
            return;
        }

        if (item.type == ItemTypes.Unique && quantity > 1)
        {
            ShowMessage("You cannot purchase more than one Unique Item!");
            return;
        }

        if (playerInventory.Money < totalCost)
        {
            ShowMessage("Not enough money!");
            return;
        }

        if (playerInventory.TrySpendMoney(totalCost))
        {
            playerInventory.AddItem(item, quantity);
            ShowMessage($"Purchased {quantity} x {item.itemName}!");
            UpdateMoneyUI();

            // Refresh Inventory UI
            var ui = FindObjectOfType<InventoryUIManager>();
            if (ui != null) ui.RefreshUI();
        }
    }

    private void UpdateMoneyUI()
    {
        if (playerInventory != null && moneyText != null)
            moneyText.text = $"Money: {playerInventory.Money} G";
    }

    /// <summary>
    /// メッセージ表示
    /// </summary>
    private void ShowMessage(string msg)
    {
        messageText.text = msg;

        // stop old fade coroutine if one is running
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(FadeMessageOut());

    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }

    private IEnumerator FadeMessageOut()
    {
        yield return new WaitForSeconds(1.5f); // wait 2 seconds before fading

        float duration = 1f; // fade duration
        float elapsed = 0f;

        Color startColor = messageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            messageText.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        messageText.text = ""; // clear text fully after fade
        messageText.color = new Color(startColor.r, startColor.g, startColor.b, 1f); // reset alpha for next time

        if (purchasePanel != null)
            purchasePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// ショップを開く
    /// </summary>
    public void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(true);

        }
        isOpen = true;
        UpdateMoneyUI();
        ClearMessage();

    }

    /// <summary>
    /// ショップを閉じる
    /// </summary>
    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.SetActive(false);

        isOpen = false;
        ClearMessage();
    }

    public void ToggleShop()
    {
        if (isOpen)
            CloseShop();
        else
            OpenShop();
    }


}
