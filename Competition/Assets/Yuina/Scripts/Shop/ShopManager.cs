using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using ISystem_Actions;  // Namespace for using InputSystem_Actions
using UnityEngine.EventSystems;
using System.Collections.Generic;

[System.Serializable]
public class CartEntry
{
    public ShopItem item;
    public int quantity;

    public CartEntry(ShopItem item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class ShopManager : MonoBehaviour
{
    private InputSystem_Actions iSystemActions;

    public static ShopManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;             // �V���b�v�S�̂�UI
    [SerializeField] private RectTransform contentTransform; // Content (ScrollView�̒��g)
    [SerializeField] private GameObject itemButtonPrefab;    // �A�C�e���{�^��Prefab
    [SerializeField] private TextMeshProUGUI moneyText;      // �������\��
    [SerializeField] private TextMeshProUGUI messageText;    // ���b�Z�[�W�\��

    [Header("Shop Items")]
    [SerializeField] private ShopItem[] shopItems;           // �̔��A�C�e���ꗗ

    [SerializeField] private ScrollRect scrollRect;

    [Header("Panels")]
    [SerializeField] private PurchasePanel purchasePanel;   // ���ʑI���p�l��

    private bool isOpen = false;

    // �J�[�g�֘A
    private List<CartEntry> cart = new();
    //private ShopItem currentItem = null;
    //private int currentQuantity = 0;

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

        // Cancel�ɊY������L�[�ŃV���b�v�����
        if (iSystemActions.UI.Cancel.WasPressedThisFrame())
        {
            if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
            {
                // PurchasePanel���J���Ă���΂����������
                purchasePanel.Close();
                return; // �V���b�v�͕��Ȃ�
            }
            else
            {
                // �ʏ펞�̓V���b�v�S�̂����
                CloseShop();
                return;
            }
        }

        // �J�[�\�����쐧��
        if (purchasePanel != null && purchasePanel.IsPurchasePanelActive)
        {
            // PurchasePanel���J���Ă���Ԃ̓J�[�\���ړ��𖳌���
            EventSystem.current.sendNavigationEvents = false;
        }
        else
        {
            // �ʏ펞�͗L����
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
    public void BuyItem(ShopItem item)
    {
        // ���������s�A�C�e���͏����ς݂Ȃ�w���s��
        if (item.type == ItemType.Unique && PlayerInventory.Instance.HasItem(item))
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
        {
            totalCost += entry.item.price * entry.quantity;
        }

        if (totalCost == 0)
        {
            ShowMessage("Cart is empty!");
            return;
        }

        if (PlayerInventory.Instance.Money < totalCost)
        {
            ShowMessage("Not enough money!");
            return;
        }

        if (PlayerInventory.Instance.TrySpendMoney(totalCost))
        {
            foreach (var entry in cart)
            {
                for (int i = 0; i < entry.quantity; i++)
                {
                    PlayerInventory.Instance.AddItem(entry.item);
                }
            }

            ShowMessage("Purchase successful!");
            UpdateMoneyUI();
            cart.Clear();
        }
    }

    /// <summary>
    /// ���ۂ̍w�������iPurchasePanel����Ă΂��j
    /// </summary>
    public void PurchaseItem(ShopItem item, int quantity)
    {
        int totalCost = item.price * quantity;

        // ���j�[�N�A�C�e���`�F�b�N
        if (item.type == ItemType.Unique && PlayerInventory.Instance.HasItem(item))
        {
            ShowMessage("You already have this Unique Item!");
            return;
        }

        // ����̃��j�[�N�A�C�e����2�ȏ㔃�����Ƃ��Ă��Ȃ����m�F
        if(item.type == ItemType.Unique && quantity > 1)
        {
            ShowMessage("You cannot purchase more than two Unique Items!");
            return;
        }

        // ��������邩�m�F
        if (PlayerInventory.Instance.Money < totalCost)
        {
            ShowMessage("Not enough money!");
            return;
        }

        // �x����
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
    /// ������UI�X�V
    /// </summary>
    private void UpdateMoneyUI()
    {
        moneyText.text = $"Money: {PlayerInventory.Instance.Money} G";
    }

    /// <summary>
    /// ���b�Z�[�W�\��
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

    /// <summary>
    /// �V���b�v���J��
    /// </summary>
    public void OpenShop()
    {
        if (shopUI != null)
            shopUI.SetActive(true);

        isOpen = true;
        UpdateMoneyUI();
        ClearMessage();
        PlayerController.Instance.SetCanMove(false); // �v���C���[��~
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
        PlayerController.Instance.SetCanMove(true); // �v���C���[�ĊJ
    }

}
