using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private Transform contentPanel;  // inside ScrollView Content
    [SerializeField] private Button craftButton;
    [SerializeField] private TextMeshProUGUI selectedRecipeText;
    [SerializeField] private GameObject craftingPanel; // toggle with a key

    [Header("Crafting")]
    [SerializeField] private Crafting craftingSystem;

    private int selectedRecipeIndex = -1;
    private List<GameObject> recipeButtons = new List<GameObject>();

    private void Start()
    {
        if (craftingSystem == null)
            craftingSystem = FindObjectOfType<Crafting>();

        // Generate buttons for each recipe
        for (int i = 0; i < craftingSystem.recipes.Length; i++)
        {
            var recipe = craftingSystem.recipes[i];
            var buttonObj = Instantiate(recipeButtonPrefab, contentPanel);
            recipeButtons.Add(buttonObj);

            // Set UI
            buttonObj.transform.Find("Icon").GetComponent<Image>().sprite = recipe.result.icon;
            buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = recipe.result.itemName;

            // Ingredients text
            string ingText = "Requires: ";
            foreach (var ing in recipe.ingredients)
            {
                ingText += $"{ing.item.itemName} x{ing.quantity}, ";
            }
            ingText = ingText.TrimEnd(',', ' ');
            buttonObj.transform.Find("IngredientsText").GetComponent<TextMeshProUGUI>().text = ingText;

            // Add click listener
            int index = i; // copy for closure
            buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectRecipe(index));
        }
        // After for-loop
        AdjustContentSize();

        craftButton.onClick.AddListener(OnCraftButtonClicked);
        craftingPanel.SetActive(false);
    }

    private void Update()
    {
        if (craftingSystem == null)
            craftingSystem = FindObjectOfType<Crafting>();

        if (ShopManager.Instance != null && ShopManager.Instance.IsShopActive)
            return; // Shop open => crafting disabled

        // Toggle UI with C key
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (UIManager.instance != null && UIManager.instance.IsPaused()) return;

            bool newState = !craftingPanel.activeSelf;
            craftingPanel.SetActive(newState);

            if (UIManager.instance != null)
                UIManager.instance.canPause = !newState;
        }
    }


    private void SelectRecipe(int index)
    {
        selectedRecipeIndex = index;
        selectedRecipeText.text = $"Selected: {craftingSystem.recipes[index].result.itemName}";
    }

    private void OnCraftButtonClicked()
    {
        if (selectedRecipeIndex == -1)
        {
            Debug.Log("No recipe selected.");
            return;
        }

        craftingSystem.TryCraft(selectedRecipeIndex);
    }

    private void AdjustContentSize()
    {
        var layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();

        int childCount = contentPanel.childCount;
        if (childCount == 0) return;

        // Each button height = 120
        float elementHeight = 130f;

        // Spacing between buttons
        float spacing = layoutGroup.spacing;

        // Total height = (button height * count) + (spacing * (count - 1)) + top + bottom padding
        float totalHeight = (elementHeight * childCount);

        // Clamp so it never goes below 300
        float targetHeight = Mathf.Max(300, totalHeight);

        // Apply to RectTransform
        var rt = contentPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, targetHeight);
    }


}
