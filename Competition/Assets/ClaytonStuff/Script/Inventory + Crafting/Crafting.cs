using UnityEngine;

public class Crafting : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public CraftingRecipe[] recipes; // assign in Inspector

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void TryCraft(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Length)
        {
            Debug.Log("Invalid recipe index.");
            return;
        }

        var recipe = recipes[recipeIndex];
        var inv = inventoryManager.PlayerInventory;

        // 1? Check if player has all ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inv.HasItem(ingredient.item, ingredient.quantity))
            {
                Debug.Log($" Missing ingredient: {ingredient.item.itemName}");
                return;
            }
        }

        // 2? Check if there's enough space to add the crafted item
        if (!inv.CanAddItem(recipe.result))
        {
            Debug.LogWarning($" Cannot craft {recipe.result.itemName} — no space or duplicate restriction!");
            return;
        }

        // 3? Remove ingredients only after confirming space
        foreach (var ingredient in recipe.ingredients)
        {
            inventoryManager.RemoveItemFromInventory(ingredient.item, ingredient.quantity);
        }

        // 4? Add crafted item
        inventoryManager.AddItemToInventory(recipe.result, recipe.resultQuantity);
        Debug.Log($" Crafted {recipe.resultQuantity}x {recipe.result.itemName}!");
    }
}
