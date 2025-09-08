using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;          // ショップ全体のUI
    [SerializeField] private Transform itemContainer;    // Content (ScrollViewの中)
    [SerializeField] private GameObject itemButtonPrefab; // アイテムボタンPrefab
    [SerializeField] private TextMeshProUGUI moneyText;  // 所持金表示
    [SerializeField] private TextMeshProUGUI messageText; // メッセージ表示 (例: お金が足りません)

    [Header("Shop Items")]
    [SerializeField] private ShopItem[] shopItems;       // 販売アイテム一覧

    private bool isOpen = false;

    public bool IsShopActive => shopUI.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (shopUI != null)
            shopUI.SetActive(false);
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

        // Escキーでショップ閉じる
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseShop();
        }
    }

    /// <summary>
    /// アイテムボタンを生成
    /// </summary>
    private void PopulateShop()
    {
        // 既存の子要素を削除
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // アイテムごとにボタンを生成
        foreach (var item in shopItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemContainer);

            // アイコン、名前、値段を取得して反映    (UI Name)
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
            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() => BuyItem(item));
            }
        }
    }

    /// <summary>
    /// アイテム購入処理
    /// </summary>
    private void BuyItem(ShopItem item)
    {
        // 武器は所持済みなら購入不可
        if (item.type == ItemType.Weapon && PlayerInventory.Instance.HasItem(item))
        {
            ShowMessage("You already have this weapon!");
            return;
        }

        // 所持金判定
        if (PlayerInventory.Instance.TrySpendMoney(item.price))
        {
            PlayerInventory.Instance.AddItem(item);
            UpdateMoneyUI();
            ShowMessage($"{item.itemName} purchased!");
            Debug.Log($"購入: {item.itemName}");
        }
        else
        {
            ShowMessage("Not enough money!");
            Debug.Log("お金が足りません");
        }
    }

    /// <summary>
    /// 所持金UI更新
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: {PlayerInventory.Instance.Money} G";
    }

    /// <summary>
    /// メッセージ表示
    /// </summary>
    private void ShowMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }

    /// <summary>
    /// ショップを開く
    /// </summary>
    public void OpenShop()
    {
        if (shopUI != null)
            shopUI.SetActive(true);

        isOpen = true;
        UpdateMoneyUI();
        ClearMessage();
        PlayerController.Instance.SetCanMove(false); // プレイヤー停止
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
        PlayerController.Instance.SetCanMove(true); // プレイヤー再開
    }
}
