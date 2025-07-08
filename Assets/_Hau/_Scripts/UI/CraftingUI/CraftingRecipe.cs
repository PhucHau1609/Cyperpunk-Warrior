using UnityEngine;

// 4. Đảm bảo CraftingRecipe có đủ thông tin
[System.Serializable]
public class CraftingRecipe
{
    [Header("Required Items")]
    public ItemCode[] requiredItems;

    [Header("Output Item")]
    public ItemCode outputItemCode;  // Dropdown ItemCode - đơn giản
    public int outputItemCount = 1;  // Số lượng output

    [Header("Optional")] // Không bắt buộc phải điền
    public string recipeName;        // Tên công thức (để dễ nhớ)
    public string description;       // Mô tả công thức
}
