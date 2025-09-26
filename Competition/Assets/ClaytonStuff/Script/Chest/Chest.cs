using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ChestItem
{
    public GameObject itemPrefab;
    [Range(0f, 1f)] public float dropChance = 1f;
}

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public List<ChestItem> possibleItems;
    public int minItems = 1;
    public int maxItems = 5;
    public float explosionForce = 5f;
    public float delayBetweenItems = 0.2f;
    public float maxScatterRadius = 3f;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Chest Spin")]
    public float spinSpeed = 360f; // degrees per second

    private bool opened = false;

    private PlayerInput playerInput;
    private InputAction interactAction;

    void Awake()
    {
        // Get PlayerInput from NewPlayerMovement
        NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null)
        {
            playerInput = playerMovement.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interaction"];
                interactAction.Enable();
            }
        }
    }

    private void Update()
    {
        // Spin chest if opened
        if (opened)
        {
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
        }
        else
        {
            var movement = FindObjectOfType<NewPlayerMovement>();
            if (movement != null)
                playerInput = movement.GetComponent<PlayerInput>();
            // Player interaction
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var hit in hits)
            {
                var action = playerInput.actions["Interaction"];
                if (hit.CompareTag("Player") && action.WasPressedThisFrame())
                {
                    OpenChest();
                    break;
                }
            }
        }
    }

    private void OpenChest()
    {
        if (opened) return;
        opened = true;

        if (spawnPoint == null) spawnPoint = transform;

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
                if (results.Count >= itemCount) break;

                if (Random.value <= chestItem.dropChance)
                    results.Add(chestItem.itemPrefab);
            }

            if (results.Count == 0 && possibleItems.Count > 0)
                results.Add(possibleItems[Random.Range(0, possibleItems.Count)].itemPrefab);
        }

        return results;
    }

    private IEnumerator SpawnItems(List<GameObject> itemsToSpawn)
    {
        foreach (var prefab in itemsToSpawn)
        {
            GameObject spawnedItem = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0;

                Vector2 forwardDir = spawnPoint.right;
                Vector2 scatterDir = (forwardDir + Random.insideUnitCircle * 0.5f).normalized;

                rb.AddForce(scatterDir * explosionForce, ForceMode2D.Impulse);

                StartCoroutine(ClampToRadius(spawnedItem.transform, maxScatterRadius));
                StartCoroutine(DisablePhysics(rb, 1f));
            }

            yield return new WaitForSeconds(delayBetweenItems);
        }

        // Stop spinning and destroy chest
        opened = false;
        Destroy(gameObject, 1f);
    }

    private IEnumerator DisablePhysics(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private IEnumerator ClampToRadius(Transform item, float radius)
    {
        Vector3 chestPos = transform.position;
        float timer = 1f;

        while (timer > 0f && item != null)
        {
            Vector3 offset = item.position - chestPos;
            if (offset.magnitude > radius)
                item.position = chestPos + offset.normalized * radius;

            timer -= Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxScatterRadius);

        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, spawnPoint.position);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
        }
    }
}
