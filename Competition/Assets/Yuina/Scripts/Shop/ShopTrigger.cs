using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private ShopDialogueUI dialogueUI;

    void Awake()
    {
        dialogueUI = FindObjectOfType<ShopDialogueUI>();

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

    void Update()
    {
        if (!playerInRange || dialogueUI == null) return;

        // Block input while dialogue or shop is open
        if (dialogueUI.IsActive() || ShopManager.Instance.IsShopActive || ShopManager.Instance.isSellOpen)
            return;

        if (playerInput == null)
        {
            var movement = FindObjectOfType<NewPlayerMovement>();
            if (movement != null)
                playerInput = movement.GetComponent<PlayerInput>();
        }

        if (playerInput != null)
        {
            var action = playerInput.actions["Interaction"];
            if (action.WasPressedThisFrame())
            {
                dialogueUI.Show(); // now has built-in Buy / Sell / Cancel handling
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (ShopManager.Instance)
            {
                if (ShopManager.Instance.IsShopActive && !ShopManager.Instance.isSellOpen)
                    ShopManager.Instance.CloseShop();
                else
                    ShopManager.Instance.CloseSellMenu();
            }
        }
    }
}
