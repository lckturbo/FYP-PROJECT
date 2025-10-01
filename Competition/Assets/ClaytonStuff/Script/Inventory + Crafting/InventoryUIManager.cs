using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Inventory Panels")]
    [SerializeField] private GameObject mainInventoryPanel;
    [SerializeField] private GameObject subInventoryPanel;

    [Header("Slot Prefab")]
    [SerializeField] private GameObject slotPrefab;

    private InventoryManager inventoryManager;
    private NewPlayerMovement playerMovement;
    private List<GameObject> mainSlots = new List<GameObject>();
    private List<GameObject> subSlots = new List<GameObject>();
    private bool subInventoryOpen = false;

    private int selectedMainSlot = -1; // -1 = none selected
    private int selectedSubSlot = -1; // -1 = none selected
    private int subColumns = 6;       // grid width
    private int subRows = 5;          // grid height

    [Header("Item Info UI")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject descriptionPanel;

    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        playerMovement = FindObjectOfType<NewPlayerMovement>();

        // Create UI slots (6 main, 30 sub)
        for (int i = 0; i < 6; i++)
        {
            var slot = Instantiate(slotPrefab, mainInventoryPanel.transform);
            slot.transform.Find("Highlight").gameObject.SetActive(false); // ensure off
            mainSlots.Add(slot);
        }

        for (int i = 0; i < 30; i++)
        {
            var slot = Instantiate(slotPrefab, subInventoryPanel.transform);
            subSlots.Add(slot);
        }

        mainInventoryPanel.SetActive(true);
        subInventoryPanel.SetActive(false);

        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleSubInventory();

        if (!subInventoryOpen)
        {
            // Main inventory selection...
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectMainSlot(i);
                }
            }

            if (Input.GetKeyDown(KeyCode.E)) UseSelectedItem();

            // Hide description when not in sub-inventory
            descriptionPanel.SetActive(false);
        }
        else
        {
            // Sub-inventory navigation
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSubSelection(1, 0);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSubSelection(-1, 0);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSubSelection(0, -1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSubSelection(0, 1);

            if (Input.GetKeyDown(KeyCode.E)) UseSelectedItemSub();

            // Show description if valid item selected
            ShowSubItemDescription();
        }
    }


    private void MoveSubSelection(int dx, int dy)
    {
        if (selectedSubSlot == -1) selectedSubSlot = 0;

        int x = selectedSubSlot % subColumns;
        int y = selectedSubSlot / subColumns;

        x = Mathf.Clamp(x + dx, 0, subColumns - 1);
        y = Mathf.Clamp(y + dy, 0, subRows - 1);

        int newIndex = y * subColumns + x;
        if (newIndex != selectedSubSlot)
        {
            selectedSubSlot = newIndex;
            UpdateSubHighlight();
        }
    }

    private void UseSelectedItemSub()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return;

        if (selectedSubSlot < 0 || selectedSubSlot >= inv.subInventory.Count)
        {
            Debug.Log("No valid sub item selected.");
            return;
        }

        var slot = inv.subInventory[selectedSubSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            Debug.Log("Selected sub slot is empty.");
            return;
        }

        if (slot.item.itemName == "Heal Potion")
        {
            inventoryManager.RemoveItemFromInventory(slot.item, 1);
        }
        else
        {
            Debug.Log($"Selected sub item: {slot.item.itemName} (not usable here).");
        }
    }

    private void UpdateSubHighlight()
    {
        for (int i = 0; i < subSlots.Count; i++)
        {
            var highlight = subSlots[i].transform.Find("Highlight").gameObject;
            highlight.SetActive(i == selectedSubSlot);
        }

        ShowSubItemDescription();
    }

    private void ShowSubItemDescription()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null || selectedSubSlot < 0 || selectedSubSlot >= inv.subInventory.Count)
        {
            descriptionPanel.SetActive(false);
            return;
        }

        var slot = inv.subInventory[selectedSubSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            descriptionPanel.SetActive(false);
            return;
        }

        descriptionPanel.SetActive(true);
        descriptionText.text = $"{slot.item.itemName}\n\n{slot.item.description}";
    }


    private void UseSelectedItem()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return;


        if (selectedMainSlot < 0 || selectedMainSlot >= inv.mainInventory.Count)
        {
            Debug.Log("No valid item selected.");
            return;
        }

        var slot = inv.mainInventory[selectedMainSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            Debug.Log("Selected slot is empty.");
            return;
        }

        if (slot.item.isBuff)
        {
            var buffHandler = FindObjectOfType<PlayerBuffHandler>();
            if (buffHandler != null)
            {
                //  Check if buff is already active
                if (!buffHandler.IsBuffActive)
                {
                    buffHandler.ApplyAttackBuff(slot.item.attackBuffAmount, slot.item.buffDuration);
                    inventoryManager.RemoveItemFromInventory(slot.item, 1);
                }
                else
                {
                    Debug.Log("Buff already active — cannot consume another until it expires!");
                    // (Optional) Show UI message to the player instead of just logging
                }
            }
        }


        if (slot.item.itemName == "Heal Potion")
        {
            Debug.Log("Using Heal Potion...");
            inventoryManager.RemoveItemFromInventory(slot.item, 1);
        }
        else
        {
            Debug.Log($"Selected item: {slot.item.itemName} (not usable here).");
        }


    }

    private void ToggleSubInventory()
    {
        subInventoryOpen = !subInventoryOpen;

        mainInventoryPanel.SetActive(!subInventoryOpen);
        subInventoryPanel.SetActive(subInventoryOpen);

        RefreshUI();
    }

    private void SelectMainSlot(int index)
    {
        selectedMainSlot = index;

        for (int i = 0; i < mainSlots.Count; i++)
        {
            var highlight = mainSlots[i].transform.Find("Highlight").gameObject;
            highlight.SetActive(i == index);
        }

        var inv = inventoryManager.PlayerInventory;
        if (inv != null && index >= 0 && index < inv.mainInventory.Count)
        {
            var slot = inv.mainInventory[index];
            var heldItem = FindObjectOfType<PlayerHeldItem>();
            if (heldItem != null) heldItem.DisplayItem(slot.item);
        }
    }

    public void RefreshUI()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return;

        foreach (var slot in mainSlots) ClearSlot(slot);
        foreach (var slot in subSlots) ClearSlot(slot);

        for (int i = 0; i < inv.mainInventory.Count && i < mainSlots.Count; i++)
        {
            UpdateSlot(mainSlots[i], inv.mainInventory[i]);
        }

        for (int i = 0; i < inv.subInventory.Count && i < subSlots.Count; i++)
        {
            UpdateSlot(subSlots[i], inv.subInventory[i]);
        }
    }

    private void ClearSlot(GameObject slotObj)
    {
        Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
        TextMeshProUGUI qtyText = slotObj.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

        icon.sprite = null;
        icon.enabled = false;
        qtyText.text = "";
    }

    private void UpdateSlot(GameObject slotObj, InventorySlot slotData)
    {
        Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
        TextMeshProUGUI qtyText = slotObj.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

        if (slotData.item != null && slotData.item.icon != null)
        {
            icon.sprite = slotData.item.icon;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        qtyText.text = slotData.item != null && slotData.item.isStackable && slotData.quantity > 1
            ? slotData.quantity.ToString()
            : "";
    }
}
