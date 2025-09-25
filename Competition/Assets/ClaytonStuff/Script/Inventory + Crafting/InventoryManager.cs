using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public Inventory PlayerInventory { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reassign if missing
        if (PlayerInventory == null)
        {
            PlayerInventory = FindObjectOfType<Inventory>();
            Debug.Log($"Reassigned Inventory to: {PlayerInventory}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (PlayerInventory == null)
            {
                Debug.LogWarning("No PlayerInventory found!");
                return;
            }

            Debug.Log("=== Main Inventory ===");
            foreach (var slot in PlayerInventory.mainInventory)
            {
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");
            }

            Debug.Log("=== Sub Inventory ===");
            foreach (var slot in PlayerInventory.subInventory)
            {
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameManager.instance.ChangeScene("ClaytonTestScene");
        }
    }

    public void AddItemToInventory(Item item, int amount = 1)
    {
        if (PlayerInventory == null)
        {
            Debug.LogError("No PlayerInventory assigned!");
            return;
        }

        PlayerInventory.AddItem(item, amount);
        Debug.Log($"Added {amount} {item.itemName} to {(item.category == ItemCategory.Main ? "Main" : "Sub")} Inventory");

        // Update UI after adding
        var ui = FindObjectOfType<InventoryUIManager>();
        if (ui != null) ui.RefreshUI();
    }

    public void RemoveItemFromInventory(Item item, int amount = 1)
    {
        if (PlayerInventory == null)
        {
            Debug.LogError("No PlayerInventory assigned!");
            return;
        }

        PlayerInventory.RemoveItem(item, amount);
        Debug.Log($"Removed {amount} {item.itemName}");

        // Update UI after removing
        var ui = FindObjectOfType<InventoryUIManager>();
        if (ui != null) ui.RefreshUI();
    }
}
