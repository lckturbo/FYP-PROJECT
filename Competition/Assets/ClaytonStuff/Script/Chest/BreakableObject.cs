using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableObject : MonoBehaviour, IDataPersistence
{
    [Header("Breakable Object Settings")]
    [SerializeField] private string breakableID;
    public List<GameObject> dropItems;   // Items to spawn on break
    public float dropChance = 1f;        // Overall chance to drop each item
    public float scatterSpeed = 5f;
    public float maxScatterRadius = 2f;

    [Header("Break Settings")]
    public bool destroyOnHit = true;     // Destroy immediately on hit
    public int requiredHits = 1;         // Number of hits to break
    private int currentHits = 0;
    private bool destroyed = false;

    private Transform spawnPoint;

    private void Awake()
    {
        spawnPoint = transform;
    }

    /// <summary>
    /// Call this when hit by player weapon or attack
    /// </summary>
    public void TakeHit()
    {
        if(destroyed) return;
        currentHits++;

        if (currentHits >= requiredHits)
        {
            Break();
        }
    }

    private void Break()
    {
        destroyed = true;

        SpawnDrops();
        if (destroyOnHit)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void SpawnDrops()
    {
        if (dropItems == null || dropItems.Count == 0) return;

        foreach (var item in dropItems)
        {
            if (Random.value <= dropChance)
            {
                GameObject spawned = Instantiate(item, spawnPoint.position, Quaternion.identity);

                // Scatter randomly around the object
                Vector2 dir = Random.insideUnitCircle.normalized;
                StartCoroutine(MoveToRadius(spawned.transform, dir));
            }
        }
    }

    private IEnumerator MoveToRadius(Transform item, Vector2 dir)
    {
        Vector3 origin = spawnPoint.position;
        float distance = 0f;

        while (item != null && distance < maxScatterRadius)
        {
            float step = scatterSpeed * Time.deltaTime;
            item.position += (Vector3)(dir * step);
            distance = Vector3.Distance(origin, item.position);

            if (distance >= maxScatterRadius)
            {
                item.position = origin + (Vector3)(dir.normalized * maxScatterRadius);
                break;
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxScatterRadius);
    }
    public void LoadData(GameData data)
    {
        var entry = data.brokenObjects.Find(o => o.id == breakableID);
        if (entry != null && entry.destroyed)
        {
            destroyed = true;
            Destroy(gameObject);
        }
    }

    public void SaveData(ref GameData data)
    {
        var existing = data.brokenObjects.Find(o => o.id == breakableID);

        if (existing != null)
        {
            existing.destroyed = destroyed;
        }
        else
        {
            data.brokenObjects.Add(new GameData.BreakableSaveEntry
            {
                id = breakableID,
                destroyed = destroyed
            });
        }
    }
}
