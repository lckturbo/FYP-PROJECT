using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // The item to give when picked up
    private bool playerInRange = false; // Track if player is near

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            CollectItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
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
        if (qm != null && qm.activeQuests != null)
        {
            foreach (Quest quest in qm.activeQuests)
            {
                if (quest is CollectionQuestRuntime collectionQuest)
                {
                    collectionQuest.AddItem(item.itemName);
                }
            }
        }

        // Destroy the item from the world
        Destroy(gameObject);
    }
}
