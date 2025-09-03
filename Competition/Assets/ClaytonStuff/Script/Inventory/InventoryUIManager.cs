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
    private List<GameObject> mainSlots = new List<GameObject>();
    private List<GameObject> subSlots = new List<GameObject>();
    private bool subInventoryOpen = false;

    private int selectedMainSlot = -1; // -1 = none selected

    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();

        // Create UI slots (6 main, 60 sub)
        for (int i = 0; i < 6; i++)
        {
            var slot = Instantiate(slotPrefab, mainInventoryPanel.transform);
            slot.transform.Find("Highlight").gameObject.SetActive(false); // ensure off
            mainSlots.Add(slot);
        }

        for (int i = 0; i < 60; i++)
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
        // Toggle sub inventory with I key
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleSubInventory();
        }

        // Number key selection (1–6)
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectMainSlot(i);
            }
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

        // Turn off all highlights first
        for (int i = 0; i < mainSlots.Count; i++)
        {
            var highlight = mainSlots[i].transform.Find("Highlight").gameObject;
            highlight.SetActive(i == index);
        }
    }

    public void RefreshUI()
    {
        // Clear all slots
        foreach (var slot in mainSlots) ClearSlot(slot);
        foreach (var slot in subSlots) ClearSlot(slot);

        // Fill Main Inventory
        for (int i = 0; i < inventoryManager.playerInventory.mainInventory.Count && i < mainSlots.Count; i++)
        {
            UpdateSlot(mainSlots[i], inventoryManager.playerInventory.mainInventory[i]);
        }

        // Fill Sub Inventory
        for (int i = 0; i < inventoryManager.playerInventory.subInventory.Count && i < subSlots.Count; i++)
        {
            UpdateSlot(subSlots[i], inventoryManager.playerInventory.subInventory[i]);
        }
    }

    private void ClearSlot(GameObject slotObj)
    {
        Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
        TextMeshProUGUI qtyText = slotObj.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

        icon.sprite = null;
        icon.enabled = false;
        qtyText.text = "";

        // leave Highlight state as is
    }

    private void UpdateSlot(GameObject slotObj, InventorySlot slotData)
    {
        Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
        TextMeshProUGUI qtyText = slotObj.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

        icon.sprite = slotData.item.icon;
        icon.enabled = true;

        qtyText.text = slotData.item.isStackable && slotData.quantity > 1
            ? slotData.quantity.ToString()
            : "";
    }
}
