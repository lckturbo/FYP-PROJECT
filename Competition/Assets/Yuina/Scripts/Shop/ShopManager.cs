using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;          // �V���b�v�S�̂�UI
    [SerializeField] private Transform itemContainer;    // Content (ScrollView�̒�)
    [SerializeField] private GameObject itemButtonPrefab; // �A�C�e���{�^��Prefab
    [SerializeField] private TextMeshProUGUI moneyText;  // �������\��
    [SerializeField] private TextMeshProUGUI messageText; // ���b�Z�[�W�\�� (��: ����������܂���)

    [Header("Shop Items")]
    [SerializeField] private ShopItem[] shopItems;       // �̔��A�C�e���ꗗ

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

        // Esc�L�[�ŃV���b�v����
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseShop();
        }
    }

    /// <summary>
    /// �A�C�e���{�^���𐶐�
    /// </summary>
    private void PopulateShop()
    {
        // �����̎q�v�f���폜
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // �A�C�e�����ƂɃ{�^���𐶐�
        foreach (var item in shopItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemContainer);

            // �A�C�R���A���O�A�l�i���擾���Ĕ��f    (UI Name)
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
            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                buttonComp.onClick.AddListener(() => BuyItem(item));
            }
        }
    }

    /// <summary>
    /// �A�C�e���w������
    /// </summary>
    private void BuyItem(ShopItem item)
    {
        // ����͏����ς݂Ȃ�w���s��
        if (item.type == ItemType.Weapon && PlayerInventory.Instance.HasItem(item))
        {
            ShowMessage("You already have this weapon!");
            return;
        }

        // ����������
        if (PlayerInventory.Instance.TrySpendMoney(item.price))
        {
            PlayerInventory.Instance.AddItem(item);
            UpdateMoneyUI();
            ShowMessage($"{item.itemName} purchased!");
            Debug.Log($"�w��: {item.itemName}");
        }
        else
        {
            ShowMessage("Not enough money!");
            Debug.Log("����������܂���");
        }
    }

    /// <summary>
    /// ������UI�X�V
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: {PlayerInventory.Instance.Money} G";
    }

    /// <summary>
    /// ���b�Z�[�W�\��
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
