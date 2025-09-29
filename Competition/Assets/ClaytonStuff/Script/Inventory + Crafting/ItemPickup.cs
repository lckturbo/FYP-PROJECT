using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // The item to give when picked up
    public float moveSpeed = 5f;   // how fast the item moves toward the player
    public float collectDistance = 0.5f; // how close before auto-collect

    private Transform player;

    private void Start()
    {
        // Find player by tag once
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Smoothly move toward the player
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        // Check if close enough to auto-collect
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= collectDistance)
        {
            CollectItem();
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
        if (qm != null)
        {
            var questsCopy = new List<Quest>(qm.activeQuests);
            foreach (Quest quest in questsCopy)
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
