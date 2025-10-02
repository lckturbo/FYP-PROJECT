using Unity.VisualScripting;
using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Attachment Point")]
    [SerializeField] private Transform handPoint; // assign in inspector (empty GameObject at hand)

    private GameObject currentHeld;
    private Item equippedItem;
    private NewPlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
    }

    private void Update()
    {
        if (currentHeld != null && playerMovement != null)
        {
            //  Flip the held item depending on facing direction
            //if (playerMovement)
            //{
            //    currentHeld.transform.localRotation = Quaternion.Euler(0, 180, 0); // facing left
            //}
            //else
            //{
            //    currentHeld.transform.localRotation = Quaternion.identity; // facing right
            //}
        }
    }
    public void DisplayItem(Item item)
    {
        // If no item, just hide what we're holding
        if (item == null || item.worldPrefab == null)
        {
            HideItem();
            return;
        }

        // If we’re swapping to a different item, remove the old one
        if (currentHeld != null)
        {
            Destroy(currentHeld);
            currentHeld = null;
        }

        equippedItem = item;

        // Spawn the item
        currentHeld = Instantiate(item.worldPrefab, handPoint.position, Quaternion.identity, handPoint);
        currentHeld.transform.localPosition = Vector3.zero;
        currentHeld.transform.localRotation = Quaternion.identity;
    }


    public void RemoveItem()
    {
        if (currentHeld != null)
        {
            Destroy(currentHeld);
            currentHeld = null;
        }

        equippedItem = null;
    }

    public void HideItem()
    {
        if (currentHeld != null)
        {
            currentHeld.SetActive(false);
        }
        equippedItem = null;
    }



    public Item GetEquippedItem()
    {
        return equippedItem;
    }
}
