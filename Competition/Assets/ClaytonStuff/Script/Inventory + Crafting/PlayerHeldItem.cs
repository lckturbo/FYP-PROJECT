using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [Header("Attachment Point")]
    [SerializeField] private Transform handPoint; // assign in inspector (empty GameObject at hand)

    private GameObject currentHeld;
    private Item equippedItem;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (currentHeld != null && playerMovement != null)
        {
            //  Flip the held item depending on facing direction
            if (playerMovement.SRFlipX)
            {
                currentHeld.transform.localRotation = Quaternion.Euler(0, 180, 0); // facing left
            }
            else
            {
                currentHeld.transform.localRotation = Quaternion.identity; // facing right
            }
        }
    }

    public void DisplayItem(Item item)
    {
        if (currentHeld != null) Destroy(currentHeld);

        equippedItem = item;

        if (item == null || item.worldPrefab == null) return;

        currentHeld = Instantiate(item.worldPrefab, handPoint.position, Quaternion.identity, handPoint);

        currentHeld.transform.localPosition = Vector3.zero;
        currentHeld.transform.localRotation = Quaternion.identity; // will get corrected in Update
    }

    public Item GetEquippedItem()
    {
        return equippedItem;
    }
}
