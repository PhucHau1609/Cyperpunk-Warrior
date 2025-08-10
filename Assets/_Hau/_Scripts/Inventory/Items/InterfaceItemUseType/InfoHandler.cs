using UnityEngine;

public class InfoHandler : IItemUseHandler
{
    public void Use(ItemInventory itemInventory)
    {
        //Debug.Log($"📦 Item {itemInventory.ItemProfileSO.itemCode} hiển thị bảng công thức chế tạo.");
        PanelCraft.Instance.TogglePanel();
    }
}
