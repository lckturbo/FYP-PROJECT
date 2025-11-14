using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    public Item item; // The item to give when picked up
    public float moveSpeed = 5f;   // how fast the item moves toward the player
    public float collectDistance = 0.5f; // how close before auto-collect
    public float followDuration = 3f;    // how long the item will follow before stopping

    private Transform player;
    private float followTimer;
    private bool isFollowing = true;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        followTimer = followDuration; // start countdown
    }

    private void Update()
    {
        if (player == null) return;

        if (isFollowing)
        {
            // countdown
            followTimer -= Time.deltaTime;
            if (followTimer <= 0f)
            {
                isFollowing = false; // stop chasing
                return;
            }

            // Smoothly move toward the player
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            // Auto-collect if close enough
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= collectDistance)
            {
                CollectItem();
            }
        }
    }

    // ?? Allow pickup by collision after follow ends
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFollowing && other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        AudioManager.instance.PlaySFXAtPoint("ding-411634", transform.position);
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null && inv.PlayerInventory != null)
        {
            if (!inv.PlayerInventory.CanAddItem(item))
            {
                Debug.Log("Main inventory full! Cannot pick up item.");
                return;
            }

            inv.AddItemToInventory(item, 1);
        }

        // Quest updates
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

        Destroy(gameObject);
    }
}
