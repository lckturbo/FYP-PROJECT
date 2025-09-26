using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // The item to give when picked up
    private bool playerInRange = false; // Track if player is near

    private void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
            playerInRange = true;
            // Optionally, show UI prompt like "Press F to collect"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Hide UI prompt if any
        }
    }

    private void CollectItem()
    {
        // Add item to inventory
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
        {
            inv.AddItemToInventory(item, 1);
        }

        // Update active collection quests
        QuestManager qm = FindObjectOfType<QuestManager>();
        // Create a snapshot to avoid modifying while iterating
        var questsCopy = new List<Quest>(qm.activeQuests);

        foreach (Quest quest in questsCopy)
        {
            if (quest is CollectionQuestRuntime collectionQuest)
            {
                collectionQuest.AddItem(item.itemName);
            }
        }


        // Destroy the item from the world
        Destroy(gameObject);
    }
}
