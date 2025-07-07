using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CraftingManager : HauSingleton<CraftingManager>
{
    [SerializeField] private List<CraftingRecipe> recipes;

    public CraftingRecipe FindMatchingRecipe(ItemCode[] inputItems)
    {
        if (inputItems == null || inputItems.Length == 0)
            return null;

        foreach (CraftingRecipe recipe in recipes)
        {
            if (DoesRecipeMatch(recipe, inputItems))
                return recipe;
        }
        return null;
    }

    private bool DoesRecipeMatch(CraftingRecipe recipe, ItemCode[] inputItems)
    {
        // Kiểm tra số lượng
        if (recipe.requiredItems.Length != inputItems.Length)
            return false;

        // Sắp xếp và so sánh
        var sortedRecipe = recipe.requiredItems.OrderBy(i => i).ToArray();
        var sortedInput = inputItems.OrderBy(i => i).ToArray();

        for (int i = 0; i < sortedRecipe.Length; i++)
        {
            if (sortedRecipe[i] != sortedInput[i])
                return false;
        }

        return true;
    }

    // Method để lấy tất cả recipes (useful for debugging)
    public List<CraftingRecipe> GetAllRecipes()
    {
        return recipes;
    }
}
