using UnityEngine;

public class CraftRecipeClaimUI : MonoBehaviour
{
    [Header("Ref")]
    public SimplePanelController1 panelController; // panel A
    public RectTransform targetInventoryIcon;
    public Sprite recipeSprite;
    public ItemCode recipeCode = ItemCode.CraftingRecipe;

    private bool hasClaimedReward = false;

    public void OnClaimRecipeButtonClicked()
    {
        // Nếu đã nhận rồi thì chỉ tắt panel
        if (hasClaimedReward)
        {
            //Debug.Log("Only Close Panel Craft");
            PanelCraft.Instance.TogglePanel();
            return;
        }

        // Đánh dấu đã nhận reward
        hasClaimedReward = true;

        // 1. Tắt UI bảng công thức
        panelController.ClosePanel();

        // 2. Bay icon lên inventory
        if (ItemPickupFlyUI.Instance != null)
        {
            Vector3 worldCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f));
            ItemPickupFlyUI.Instance.SetTarget(targetInventoryIcon);
            ItemPickupFlyUI.Instance.Play(recipeCode, worldCenter, recipeSprite);
        }

        // 3. Thêm item vào inventory
        InventoryManager.Instance.AddItem(recipeCode, 1);
    }
}
