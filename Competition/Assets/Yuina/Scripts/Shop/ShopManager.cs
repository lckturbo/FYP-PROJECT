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
    /// �A�C�e���{�^���𐶐�
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

        // Optional: also hook up UI manager if it isn�t on the player anymore
        if (inventoryUIManager == null)
        {
            inventoryUIManager = FindObjectOfType<InventoryUIManager>();
        }
    }
    private void PopulateShop()
    {
        // �����̎q�v�f���폜
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // �A�C�e�����ƂɃ{�^���𐶐�
        foreach (var item in shopItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, contentTransform);

            // �SGraphic������ON
            Graphic[] graphics = buttonObj.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics) g.enabled = true;

            // �SButton������ON
            Button[] buttons = buttonObj.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) b.enabled = true;

            // �A�C�R���A���O�A�l�i���擾���Ĕ��f
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

            // �{�^���ɍw��������o�^
            Button buttonComp = buttonObj.GetComponentInChildren<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() => BuyItem(item));
                buttonComp.onClick.AddListener(() => PurchaseItem(item, 1));

            }
        }
    }

    /// <summary>
    /// �I�����ꂽUI��ScrollRect���Ɏ��߂�
    /// </summary>
    private void ScrollToSelected(RectTransform target)
    {
        // Content��Viewport���擾
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        // �r���[�|�[�g���W�ɕϊ�
        Vector3 viewportLocalPos = viewport.InverseTransformPoint(viewport.position);
        Vector3 childLocalPos = viewport.InverseTransformPoint(target.position);

        float viewportHeight = viewport.rect.height;
        float contentHeight = content.rect.height;

        // �{�^���̏�[�E���[���v�Z
        float childTop = childLocalPos.y + target.rect.height * 0.1f;
        float childBottom = childLocalPos.y - target.rect.height * 0.1f;

        // ���݂̃X�N���[���ʒu
        float normalizedPos = scrollRect.verticalNormalizedPosition;

        // ��ɂ͂ݏo�������ɃX�N���[��
        if (childTop > viewport.rect.height * 0.1f)
        {
            normalizedPos += 0.05f; // �X�N���[�����x�i�����j
        }
        // ���ɂ͂ݏo�����牺�ɃX�N���[��
        else if (childBottom < -viewport.rect.height * 0.1f)
        {
            normalizedPos -= 0.05f;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
    }

    /// <summary>
    /// �A�C�e���w������
    /// </summary>
    public void BuyItem(Item item)
    {
        // ���������s�A�C�e���͏����ς݂Ȃ�w���s��
        if (item.type == ItemTypes.Unique /*&& PlayerInventory.Instance.HasItem(item)*/)
        {
            ShowMessage("You already have this Unique Item!");
            return;
        }

        // ���ʑI���p�l�����J��
        purchasePanel.Open(item);
    }

    /// <summary>
    /// �J�[�g�܂Ƃ߂čw��
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
    /// ���ۂ̍w�������iPurchasePanel����Ă΂��j
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
    /// ���b�Z�[�W�\��
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
    /// �V���b�v���J��
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
    /// �V���b�v�����
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
