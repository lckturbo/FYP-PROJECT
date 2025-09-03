using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public Item item;
        public int quantity;
    }

    public Ingredient[] ingredients;   // items required
    public Item result;                // crafted item
    public int resultQuantity = 1;     // how many crafted
}
