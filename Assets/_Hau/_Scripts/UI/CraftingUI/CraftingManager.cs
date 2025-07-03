using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CraftingManager : HauSingleton<CraftingManager>
{
    [SerializeField] private List<CraftingRecipe> recipes;

    public CraftingRecipe FindMatchingRecipe(ItemCode[] inputItems)
    {
        foreach (CraftingRecipe recipe in recipes)
        {
            if (recipe.requiredItems.OrderBy(i => i).SequenceEqual(inputItems.OrderBy(i => i)))
                return recipe;
        }
        return null;
    }
}
