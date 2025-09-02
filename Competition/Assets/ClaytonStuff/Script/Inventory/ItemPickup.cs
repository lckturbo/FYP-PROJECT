using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // The item to give when picked up

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add item to inventory
            InventoryManager inv = other.GetComponent<InventoryManager>();
            if (inv != null)
            {
                inv.AddItemToInventory(item, 1);
            }

            // Check all active quests and update collection quests
            QuestManager qm = FindObjectOfType<QuestManager>();
            if (qm != null && qm.activeQuests != null)
            {
                foreach (Quest quest in qm.activeQuests)
                {
                    if (quest is CollectionQuestRuntime collectionQuest)
                    {
                        collectionQuest.AddItem(item.itemName); // Pass the item name to the quest
                    }
                }
            }

            // Destroy the item from the world
            Destroy(gameObject);
        }
    }
}
