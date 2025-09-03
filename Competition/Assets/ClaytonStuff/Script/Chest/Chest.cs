using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public List<GameObject> itemPrefabs;   // Prefabs of items that can spawn
    public int minItems = 1;               // Minimum number of items to spawn
    public int maxItems = 5;               // Maximum number of items to spawn
    public float explosionForce = 5f;      // Force applied when spawning items
    public float delayBetweenItems = 0.2f; // Delay between spawning each item

    private bool opened = false;

    private void Update()
    {
        if (!opened)
        {
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
    }

    private void OpenChest()
    {
        opened = true;
        int itemCount = Random.Range(minItems, maxItems + 1);

        // Start coroutine to spawn items one by one
        StartCoroutine(SpawnItems(itemCount));

        // Optionally, play chest open animation
    }

    private IEnumerator SpawnItems(int itemCount)
    {
        for (int i = 0; i < itemCount; i++)
        {
            // Pick a random prefab
            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];

            // Spawn it at chest position
            GameObject spawnedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);

            // Apply explosion force
            Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 forceDir = (Vector2.up + Random.insideUnitCircle * 0.5f).normalized * explosionForce;
                rb.AddForce(forceDir, ForceMode2D.Impulse);
            }

            // Wait a short time before spawning next item
            yield return new WaitForSeconds(delayBetweenItems);
        }

        // Optionally destroy chest after spawning all items
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f); // Interaction radius
    }
}
