using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public Inventory PlayerInventory { get; private set; }

    void Awake()
    {
        // If this GameObject is a child
        if (transform.parent != null)
        {
            // Detach from its parent first
            transform.SetParent(null);
        }
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
        StartCoroutine(EnsureInventoryAssigned()); // ensure assignment at startup
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(EnsureInventoryAssigned());
    }

    /// <summary>
    /// Keeps checking until PlayerInventory is found.
    /// Works across scene loads and async spawns.
    /// </summary>
    private IEnumerator EnsureInventoryAssigned()
    {
        while (PlayerInventory == null)
        {
            PlayerInventory = FindObjectOfType<Inventory>();
            if (PlayerInventory != null)
            {
                Debug.Log($" PlayerInventory assigned: {PlayerInventory.name}");
                yield break;
            }

            // wait a bit before retrying
            yield return new WaitForSeconds(0.25f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (PlayerInventory == null)
            {
                Debug.LogWarning(" No PlayerInventory found!");
                return;
            }

            Debug.Log("=== Main Inventory ===");
            foreach (var slot in PlayerInventory.mainInventory)
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");

            Debug.Log("=== Sub Inventory ===");
            foreach (var slot in PlayerInventory.subInventory)
                Debug.Log($"{slot.item.itemName} x{slot.quantity}");
        }

        //if (Input.GetKeyDown(KeyCode.O))
        //    GameManager.instance.ChangeScene("jas");

        //if (Input.GetKeyDown(KeyCode.L))
        //    GameManager.instance.ChangeScene("ClaytonTestScene");
    }

    public void AddItemToInventory(Item item, int amount = 1)
    {
        if (EnsureInventoryExists() == false) return;

        PlayerInventory.AddItem(item, amount);
        Debug.Log($"Added {amount} {item.itemName} to {(item.category == ItemCategory.Main ? "Main" : "Sub")} Inventory");

        RefreshUI();
    }

    public void RemoveItemFromInventory(Item item, int amount = 1)
    {
        if (EnsureInventoryExists() == false) return;

        PlayerInventory.RemoveItem(item, amount);
        Debug.Log($"Removed {amount} {item.itemName}");

        RefreshUI();
    }

    public bool HasItem(Item item)
    {
        if (EnsureInventoryExists() == false) return false;

        foreach (var slot in PlayerInventory.mainInventory)
            if (slot.item && slot.item.itemName == item.itemName)
                return true;

        foreach (var slot in PlayerInventory.subInventory)
            if (slot.item && slot.item.itemName == item.itemName)
                return true;

        return false;
    }

    private bool EnsureInventoryExists()
    {
        if (PlayerInventory != null) return true;

        PlayerInventory = FindObjectOfType<Inventory>();
        if (PlayerInventory == null)
        {
            Debug.LogError(" PlayerInventory not found in scene!");
            return false;
        }

        Debug.Log($" PlayerInventory reattached: {PlayerInventory.name}");
        return true;
    }

    private void RefreshUI()
    {
        var ui = FindObjectOfType<InventoryUIManager>();
        if (ui != null) ui.RefreshUI();
    }
}
