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
    private PlayerMovement playerMovement;
    private List<GameObject> mainSlots = new List<GameObject>();
    private List<GameObject> subSlots = new List<GameObject>();
    private bool subInventoryOpen = false;

    private int selectedMainSlot = -1; // -1 = none selected

    private int selectedSubSlot = -1; // -1 = none selected
    private int subColumns = 6;       // grid width
    private int subRows = 5;         // grid height


    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Create UI slots (6 main, 60 sub)
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
        // Toggle sub inventory with I key
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleSubInventory();
        }

        if (!subInventoryOpen)
        {
            // Main inventory number key selection
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectMainSlot(i);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                UseSelectedItem();
            }
        }
        else
        {
            // Sub-inventory navigation with arrows
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSubSelection(1, 0);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSubSelection(-1, 0);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSubSelection(0, -1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSubSelection(0, 1);

            if (Input.GetKeyDown(KeyCode.E))
            {
                UseSelectedItemSub();
            }
        }
    }

    private void MoveSubSelection(int dx, int dy)
    {
        // Initialize to (0,0) if none selected yet
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
        if (selectedSubSlot < 0 || selectedSubSlot >= inventoryManager.playerInventory.subInventory.Count)
        {
            Debug.Log("No valid sub item selected.");
            return;
        }

        var slot = inventoryManager.playerInventory.subInventory[selectedSubSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            Debug.Log("Selected sub slot is empty.");
            return;
        }

        if (slot.item.itemName == "Heal Potion")
        {
            //Debug.Log("Using Heal Potion from Sub Inventory...");
            //var player = FindObjectOfType<Player>(); // replace with your health script
            //if (player != null) player.Heal(50);

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
    }


    private void UseSelectedItem()
    {
        if (selectedMainSlot < 0 || selectedMainSlot >= inventoryManager.playerInventory.mainInventory.Count)
        {
            Debug.Log("No valid item selected.");
            return;
        }

        var slot = inventoryManager.playerInventory.mainInventory[selectedMainSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            Debug.Log("Selected slot is empty.");
            return;
        }

        // Example: Check if item is Heal Potion
        if (slot.item.itemName == "Heal Potion")
        {
            Debug.Log("Using Heal Potion...");

            //// Call player heal logic here
            //var player = FindObjectOfType<Player>(); // replace with your player health script
            //if (player != null)
            //{
            //    player.Heal(50); // heal amount example
            //}

            // Remove one from inventory
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

        // Turn off all highlights first
        for (int i = 0; i < mainSlots.Count; i++)
        {
            var highlight = mainSlots[i].transform.Find("Highlight").gameObject;
            highlight.SetActive(i == index);
        }

        // === NEW: Tell player to display item ===
        if (index >= 0 && index < inventoryManager.playerInventory.mainInventory.Count)
        {
            var slot = inventoryManager.playerInventory.mainInventory[index];
            var heldItem = FindObjectOfType<PlayerHeldItem>();
            if (heldItem != null) heldItem.DisplayItem(slot.item);
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
