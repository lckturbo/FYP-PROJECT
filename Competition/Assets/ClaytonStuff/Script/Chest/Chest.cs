using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChestItem
{
    public GameObject itemPrefab;         // Prefab of the item to spawn
    [Range(0f, 1f)] public float dropChance = 1f; // Drop probability
}

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public List<ChestItem> possibleItems; // Items with drop chances
    public int minItems = 1;              // Minimum number of items to drop
    public int maxItems = 5;              // Maximum number of items to drop
    public float explosionForce = 5f;     // Force applied when spawning items
    public float delayBetweenItems = 0.2f;// Delay between each spit

    private bool opened = false;

    private void Update()
    {
        if (opened) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
            {
                OpenChest();
                break;
            }
        }
    }

    private void OpenChest()
    {
        if (opened) return;
        opened = true;

        int itemCount = Random.Range(minItems, maxItems + 1);
        List<GameObject> drops = RollDrops(itemCount);

        StartCoroutine(SpawnItems(drops));
    }

    private List<GameObject> RollDrops(int itemCount)
    {
        List<GameObject> results = new List<GameObject>();

        while (results.Count < itemCount)
        {
            foreach (var chestItem in possibleItems)
            {
                if (results.Count >= itemCount)
                    break;

                if (Random.value <= chestItem.dropChance)
                {
                    results.Add(chestItem.itemPrefab);
                }
            }

            // safeguard: always at least one item
            if (results.Count == 0 && possibleItems.Count > 0)
            {
                results.Add(possibleItems[Random.Range(0, possibleItems.Count)].itemPrefab);
            }
        }

        return results;
    }

    private IEnumerator SpawnItems(List<GameObject> itemsToSpawn)
    {
        foreach (var prefab in itemsToSpawn)
        {
            GameObject spawnedItem = Instantiate(prefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Random spread, biased upwards for "spit out" effect
                Vector2 forceDir = (Vector2.up + Random.insideUnitCircle * 0.75f).normalized;
                rb.AddForce(forceDir * explosionForce, ForceMode2D.Impulse);
            }

            yield return new WaitForSeconds(delayBetweenItems);
        }

        // Optionally destroy chest object after all items spawned
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f); // Interaction radius
    }
}
