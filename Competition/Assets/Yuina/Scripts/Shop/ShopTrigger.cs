using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    private bool playerInRange = false;

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
                //Debug.Log("Interact");
                interactAction = playerInput.actions["Interaction"];
                interactAction.Enable();
            }
        }
    }

    void Update()
    {
        if (playerInRange && interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (!ShopManager.Instance) return;

            if (!ShopManager.Instance.IsShopActive)
            {
                Debug.Log("interaction");
                ShopManager.Instance.ToggleShop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
