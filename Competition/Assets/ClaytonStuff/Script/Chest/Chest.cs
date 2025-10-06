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
    public float scatterSpeed = 5f;
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
        if (opened)
        {
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
        }
        else
        {
            var movement = FindObjectOfType<NewPlayerMovement>();
            if (movement != null)
                playerInput = movement.GetComponent<PlayerInput>();

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

        SpawnItems(drops);

        // Stop spinning and destroy chest after a moment
        opened = false;
        Destroy(gameObject);
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

    private void SpawnItems(List<GameObject> itemsToSpawn)
    {
        int count = itemsToSpawn.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            GameObject spawnedItem = Instantiate(itemsToSpawn[i], spawnPoint.position, Quaternion.identity);

            // Calculate direction evenly spread in circle
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            StartCoroutine(MoveToRadius(spawnedItem.transform, dir));
        }
    }

    private IEnumerator MoveToRadius(Transform item, Vector2 dir)
    {
        Vector3 chestPos = transform.position;
        float distance = 0f;

        while (item != null && distance < maxScatterRadius)
        {
            float step = scatterSpeed * Time.deltaTime;
            item.position += (Vector3)(dir * step);
            distance = Vector3.Distance(chestPos, item.position);

            if (distance >= maxScatterRadius)
            {
                item.position = chestPos + (Vector3)(dir.normalized * maxScatterRadius);
                break;
            }

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
