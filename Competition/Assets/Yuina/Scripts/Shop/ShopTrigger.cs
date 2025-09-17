using UnityEngine;
using UnityEngine.InputSystem;
using ISystem_Actions;  // Namespace for using InputSystem_Actions


public class ShopTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    private InputSystem_Actions iSystemActions;

    void Awake()
    {
        iSystemActions = new InputSystem_Actions();
        iSystemActions.Player.Enable();
    }

    void Update()
    {
        if (playerInRange && iSystemActions.Player.Interact.WasPressedThisFrame())
        {
            if (!ShopManager.Instance) return;

            if (!ShopManager.Instance.IsShopActive)
            {
                ShopManager.Instance.OpenShop();
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
