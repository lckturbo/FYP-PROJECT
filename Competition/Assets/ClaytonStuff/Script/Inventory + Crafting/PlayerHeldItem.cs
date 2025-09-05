using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Attachment Point")]
    [SerializeField] private Transform handPoint; // assign in inspector (empty GameObject at hand)

    private GameObject currentHeld;

    public void DisplayItem(Item item)
    {
        // Clear old item
        if (currentHeld != null) Destroy(currentHeld);

        if (item == null || item.worldPrefab == null) return;

        // Spawn the item's prefab at hand
        currentHeld = Instantiate(item.worldPrefab, handPoint.position, Quaternion.identity, handPoint);

        // Reset transform
        currentHeld.transform.localPosition = Vector3.zero;
        currentHeld.transform.localRotation = Quaternion.identity;
    }
}
