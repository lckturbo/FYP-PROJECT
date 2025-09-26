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
                interactAction = playerInput.actions["Interaction"];
                interactAction.Enable();
            }
        }
    }

    void Update()
    {
        if (playerInRange)
        {
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
                    ShopManager.Instance.ToggleShop();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Collide with player");
        }


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Out of Collide with player");


            //  Auto close when leaving range
            if (ShopManager.Instance && ShopManager.Instance.IsShopActive)
            {
                ShopManager.Instance.CloseShop();
            }
        }
    }
}
