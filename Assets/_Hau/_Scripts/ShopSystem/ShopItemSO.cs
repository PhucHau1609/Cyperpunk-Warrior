using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemSO", menuName = "Shop/Item", order = 0)]
public class ShopItemSO : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;
    public int price; // VNĐ theo đơn vị của PayOS (int)
    [TextArea] public string descriptionForPayment; // mô tả gửi sang Payment (tùy chọn)
}
