using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleItemButtonHandler : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button healButton;
    public Button buffButton;
    public Button defenseButton;

    [Header("Item References")]
    public Item healItem;
    public Item attackBuffItem;
    public Item defenseBuffItem;

    private PlayerBuffHandler playerBuffHandler;
    private NewCharacterStats playerStats;

    private void Start()
    {
        // Get Player's Buff Handler and Stats
        var playerParty = PlayerParty.instance;
        if (playerParty != null)
        {
            var leader = playerParty.GetLeader();
            if (leader != null)
            {
                playerBuffHandler = leader.GetComponent<PlayerBuffHandler>();
                var levelApplier = leader.GetComponent<PlayerLevelApplier>();
                playerStats = levelApplier?.runtimeStats;
            }
        }

        // Hook up buttons
        if (healButton) healButton.onClick.AddListener(UseHealItem);
        if (buffButton) buffButton.onClick.AddListener(UseAttackBuff);
        if (defenseButton) defenseButton.onClick.AddListener(UseDefenseBuff);
    }

    private void UseHealItem()
    {
        //if (healItem == null || playerStats == null)
        //{
        //    Debug.LogWarning("Heal item or player stats missing.");
        //    return;
        //}

        //if (InventoryManager.Instance.PlayerInventory.HasItem(healItem))
        //{
        //    playerStats. = Mathf.Min(playerStats.hp + healItem.healing, playerStats.maxHealth);
        //    InventoryManager.Instance.RemoveItemFromInventory(healItem);
        //    Debug.Log($"Healed {healItem.healing} HP!");
        //}
        //else
        //{
        //    Debug.Log("No healing item available!");
        //}
    }

    private void UseAttackBuff()
    {
        if (attackBuffItem == null || playerBuffHandler == null)
        {
            Debug.LogWarning("Attack buff item or buff handler missing.");
            return;
        }

        if (InventoryManager.Instance.PlayerInventory.HasItem(attackBuffItem))
        {
            playerBuffHandler.ApplyAttackBuff(attackBuffItem.attackBuffAmount, attackBuffItem.buffDuration);
            InventoryManager.Instance.RemoveItemFromInventory(attackBuffItem);
        }
        else
        {
            Debug.Log("No attack buff item available!");
        }
    }

    private void UseDefenseBuff()
    {
        if (defenseBuffItem == null || playerBuffHandler == null)
        {
            Debug.LogWarning("Defense buff item or buff handler missing.");
            return;
        }

        if (InventoryManager.Instance.PlayerInventory.HasItem(defenseBuffItem))
        {
            playerBuffHandler.ApplyDefenseBuff(defenseBuffItem.defenseBuffAmount, defenseBuffItem.defenseBuffDuration);
            InventoryManager.Instance.RemoveItemFromInventory(defenseBuffItem);
        }
        else
        {
            Debug.Log("No defense buff item available!");
        }
    }
}
