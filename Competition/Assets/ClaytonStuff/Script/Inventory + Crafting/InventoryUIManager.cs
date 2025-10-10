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

        //  Highlight the first slot by default
        SelectMainSlot(0);
    }

    void Update()
    {
        // --- Handle Dialogue Lock ---
        bool dialogueActive = DialogueManager.Instance?.IsDialogueActive == true;

        // When dialogue starts, force-close sub-inventory
        if (dialogueActive)
        {
            if (subInventoryOpen)
            {
                subInventoryOpen = false;
                subInventoryPanel.SetActive(false);
                mainInventoryPanel.SetActive(false);
                descriptionPanel.SetActive(false);
            }
            else
            {
                mainInventoryPanel.SetActive(false);
            }

            return; // completely stop inventory updates during dialogue
        }
        else
        {
            mainInventoryPanel.SetActive(!subInventoryOpen);
            subInventoryPanel.SetActive(subInventoryOpen);
        }

        // --- Input Handling ---
        if (Input.GetKeyDown(KeyCode.I))
            ToggleSubInventory();

        if (!subInventoryOpen)
        {
            // Main inventory selection
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectMainSlot(i);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
                UseSelectedItem();

            descriptionPanel.SetActive(false);
        }
        else
        {
            // Sub-inventory navigation
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSubSelection(1, 0);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSubSelection(-1, 0);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSubSelection(0, -1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSubSelection(0, 1);

            if (Input.GetKeyDown(KeyCode.E))
                UseSelectedItemSub();

            ShowSubItemDescription();
        }

        // --- Always check the highlighted slot contents ---
        if (!subInventoryOpen)
            CheckHighlightedMainSlot();
        else
            CheckHighlightedSubSlot();
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

        //  Ensure valid range
        if (inv.subInventory == null || inv.subInventory.Count == 0)
        {
            Debug.Log("Sub-inventory is empty.");
            return;
        }

        if (selectedSubSlot < 0 || selectedSubSlot >= inv.subInventory.Count)
        {
            Debug.LogWarning($"Invalid sub slot index: {selectedSubSlot}. Count = {inv.subInventory.Count}");
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
            Debug.Log("Using Heal Potion from sub inventory...");
            inventoryManager.RemoveItemFromInventory(slot.item, 1);
        }
        else
        {
            Debug.Log($"Selected sub item: {slot.item.itemName} (not usable here).");
        }

        RefreshUI();
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
        if (DialogueManager.Instance?.IsDialogueActive == true)
        {
            Debug.Log("Cannot open inventory during dialogue.");
            return;
        }

        subInventoryOpen = !subInventoryOpen;

        if (subInventoryOpen)
        {
            selectedSubSlot = 0;
            UpdateSubHighlight();
            mainInventoryPanel.SetActive(false);
            subInventoryPanel.SetActive(true);
            descriptionPanel.SetActive(true);
        }
        else
        {
            selectedSubSlot = -1;
            subInventoryPanel.SetActive(false);
            mainInventoryPanel.SetActive(true);
            descriptionPanel.SetActive(false);
        }

        RefreshUI();
    }


    private void SelectMainSlot(int index)
    {
        selectedMainSlot = index;

        // Toggle highlight
        for (int i = 0; i < mainSlots.Count; i++)
        {
            var highlight = mainSlots[i].transform.Find("Highlight").gameObject;
            highlight.SetActive(i == index);
        }

        var heldItem = FindObjectOfType<PlayerHeldItem>();

        if (IsHighlightedMainSlotEmpty())
        {
            Debug.Log($"Selected slot {index}: EMPTY");
            heldItem?.HideItem();
        }
        else
        {
            var slot = inventoryManager.PlayerInventory.mainInventory[index];
            Debug.Log($"Selected slot {index}: {slot.item.itemName} (qty {slot.quantity})");
            heldItem?.DisplayItem(slot.item);
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

    private bool IsHighlightedMainSlotEmpty()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return true;
        if (selectedMainSlot < 0 || selectedMainSlot >= inv.mainInventory.Count) return true;

        var slot = inv.mainInventory[selectedMainSlot];
        return slot.item == null || slot.quantity <= 0;
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

    private void CheckHighlightedMainSlot()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return;

        // Make sure the index exists in the inventory list
        if (selectedMainSlot < 0 || selectedMainSlot >= inv.mainInventory.Count)
        {
            // Slot is empty
            FindObjectOfType<PlayerHeldItem>()?.HideItem();
            return;
        }

        var slot = inv.mainInventory[selectedMainSlot];
        var heldItem = FindObjectOfType<PlayerHeldItem>();

        if (slot.item == null || slot.quantity <= 0)
        {
            heldItem?.HideItem();
            Debug.Log($"Main slot {selectedMainSlot} is empty.");
        }
        else
        {
            if (heldItem?.GetEquippedItem() != slot.item)
            {
                heldItem?.DisplayItem(slot.item);
                //Debug.Log($"Main slot {selectedMainSlot}: showing {slot.item.itemName}");
            }
        }
    }

    private void CheckHighlightedSubSlot()
    {
        var inv = inventoryManager.PlayerInventory;
        if (inv == null) return;

        //  Ensure selectedSubSlot is within BOTH UI and inventory bounds
        if (selectedSubSlot < 0 ||
            selectedSubSlot >= subSlots.Count ||
            selectedSubSlot >= inv.subInventory.Count)
        {
            descriptionPanel.SetActive(false);
            return;
        }

        var slot = inv.subInventory[selectedSubSlot];
        if (slot.item == null || slot.quantity <= 0)
        {
            descriptionPanel.SetActive(false);
            Debug.Log($"Sub slot {selectedSubSlot} is empty or invalid.");
            return;
        }

        if (!descriptionPanel.activeSelf)
            descriptionPanel.SetActive(true);

        descriptionText.text = $"{slot.item.itemName}\n\n{slot.item.description}";
    }


}
