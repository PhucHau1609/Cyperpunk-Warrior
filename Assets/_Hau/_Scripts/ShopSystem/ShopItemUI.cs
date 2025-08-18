/*using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyPayOSButton;
    public Button buyZaloButton;

    private ShopItemSO data;
    private ShopManager owner;

    public void Setup(ShopItemSO data, ShopManager owner)
    {
        this.data = data;
        this.owner = owner;

        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.displayName;
        if (priceText) priceText.text = $"{data.price:N0} đ";

        buyPayOSButton.onClick.RemoveAllListeners();
        buyZaloButton.onClick.RemoveAllListeners();
        buyPayOSButton.onClick.AddListener(OnBuyPayOS);
        buyZaloButton.onClick.AddListener(OnBuyZalo);
    }

    void OnBuyPayOS()
    {
        if (data == null || owner == null) { Debug.LogError("[ShopItemUI] Missing data/owner"); return; }
        owner.StartPurchase(data, ShopManager.Provider.PayOS);
    }

    void OnBuyZalo()
    {
        if (data == null || owner == null) { Debug.LogError("[ShopItemUI] Missing data/owner"); return; }
        owner.StartPurchase(data, ShopManager.Provider.ZaloPay);
    }
}
*/