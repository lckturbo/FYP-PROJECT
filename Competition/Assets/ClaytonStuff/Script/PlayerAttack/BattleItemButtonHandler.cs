using TMPro; // if using TextMeshPro
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleItemButtonHandler : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button healButton;
    public Button buffButton;
    public Button defenseButton;

    [Header("UI Count Texts")]
    public TMP_Text healCountText;
    public TMP_Text buffCountText;
    public TMP_Text defenseCountText;

    [Header("Item References")]
    public Item healItem;
    public Item attackBuffItem;
    public Item defenseBuffItem;

    private PlayerBuffHandler playerBuffHandler;
    private NewCharacterStats playerStats;
    private NewHealth playerHealth;

    private void Start()
    {
        var playerParty = PlayerParty.instance;
        if (playerParty != null)
        {
            var leader = playerParty.GetLeader();
            if (leader != null)
            {
                playerBuffHandler = leader.GetComponent<PlayerBuffHandler>();
                var levelApplier = leader.GetComponent<PlayerLevelApplier>();
                playerHealth = leader.GetComponent<NewHealth>();
                playerStats = levelApplier?.runtimeStats;
            }
        }

        if (healButton) healButton.onClick.AddListener(UseHealItem);
        if (buffButton) buffButton.onClick.AddListener(UseAttackBuff);
        if (defenseButton) defenseButton.onClick.AddListener(UseDefenseBuff);

        UpdateItemCounts(); // initialize display
    }

    private void OnEnable()
    {
        BattleSystem.OnLeaderSpawned += HandleLeaderSpawned;
        UpdateItemCounts(); // refresh counts when menu opens
    }

    private void OnDisable()
    {
        BattleSystem.OnLeaderSpawned -= HandleLeaderSpawned;
    }

    private void HandleLeaderSpawned(GameObject leaderObj)
    {
        playerBuffHandler = leaderObj.GetComponent<PlayerBuffHandler>();
        var applier = leaderObj.GetComponent<PlayerLevelApplier>();
        playerHealth = leaderObj.GetComponent<NewHealth>();
        playerStats = applier?.runtimeStats;

    }

    //  Update all item count texts
    private void UpdateItemCounts()
    {
        var inv = InventoryManager.Instance?.PlayerInventory;
        if (inv == null) return;

        if (healCountText && healItem)
            healCountText.text = $"x{GetItemCount(inv, healItem)}";

        if (buffCountText && attackBuffItem)
            buffCountText.text = $"x{GetItemCount(inv, attackBuffItem)}";

        if (defenseCountText && defenseBuffItem)
            defenseCountText.text = $"x{GetItemCount(inv, defenseBuffItem)}";
    }

    private int GetItemCount(Inventory inventory, Item item)
    {
        var slot = inventory.mainInventory.Find(s => s.item == item);
        if (slot != null) return slot.quantity;

        slot = inventory.subInventory.Find(s => s.item == item);
        if (slot != null) return slot.quantity;

        return 0;
    }

    public void UseHealItem()
    {
        if (healItem == null || playerHealth == null)
        {
            Debug.LogWarning("[HealItem] Missing heal item or player reference!");
            return;
        }

        if (InventoryManager.Instance.PlayerInventory.HasItem(healItem))
        {
            playerHealth.Heal(healItem.healing);
            InventoryManager.Instance.PlayerInventory.RemoveItem(healItem);
            Debug.Log($"[HealItem] Used {healItem.itemName} and healed {healItem.healing} HP.");
            UpdateItemCounts();
        }
        else
        {
            Debug.Log("No healing item available!");
        }
    }



    public void UseAttackBuff()
    {
        if (attackBuffItem == null || playerBuffHandler == null)
        {
            Debug.LogWarning("Attack buff item or buff handler missing.");
            return;
        }

        if (InventoryManager.Instance.PlayerInventory.HasItem(attackBuffItem))
        {
            if (BuffData.instance.hasAttackBuff) return;
            playerBuffHandler.ApplyAttackBuff(attackBuffItem.attackBuffAmount, 10000);
            InventoryManager.Instance.RemoveItemFromInventory(attackBuffItem);
            UpdateItemCounts();
        }
        else
        {
            Debug.Log("No attack buff item available!");
        }
    }

    public void UseDefenseBuff()
    {
        if (defenseBuffItem == null || playerBuffHandler == null)
        {
            Debug.LogWarning("Defense buff item or buff handler missing.");
            return;
        }

        if (InventoryManager.Instance.PlayerInventory.HasItem(defenseBuffItem))
        {
            if (BuffData.instance.hasDefenseBuff) return;
            playerBuffHandler.ApplyDefenseBuff(defenseBuffItem.defenseBuffAmount, 10000);
            InventoryManager.Instance.RemoveItemFromInventory(defenseBuffItem);
            UpdateItemCounts();
        }
        else
        {
            Debug.Log("No defense buff item available!");
        }
    }
}
