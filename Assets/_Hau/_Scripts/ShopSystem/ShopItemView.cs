/*using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("Item Info")]
    public string itemId;
    public string displayName;
    public int priceVnd;

    [Header("UI")]
    public Button buyButton;

    // Cho phép ShopManager “đăng ký” hành vi khi ấn Buy
    public void BindBuy(System.Action<ShopItemView> onBuy)
    {
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke(this));
    }
}
*/

using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class ShopItemView : MonoBehaviour
{
    [Header("Item Info")]
    public string itemId;
    public string displayName;
    public int priceVnd;

    [Header("UI")]
    [SerializeField] Button buyButton;

    // Kéo đúng component text bạn đang dùng (một trong hai là đủ)
    [SerializeField] GameObject buyLabelGO; // GameObject con chứa text "Mua"
#if TMP_PRESENT
    [SerializeField] TextMeshProUGUI buyLabelTMP;
#endif
    [SerializeField] Text buyLabelUGUI;

    [Header("Colors")]
    [SerializeField] Color purchasedColor = Color.green;

    public bool IsPurchased { get; private set; }

    public void BindBuy(System.Action<ShopItemView> onBuy)
    {
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke(this));
    }

    public void ApplyPurchasedUI()
    {
        IsPurchased = true;
        if (buyButton) buyButton.interactable = false;

        // Tìm label theo thứ tự ưu tiên (đã gán sẵn) → fallback tự tìm trong buyLabelGO
#if TMP_PRESENT
        if (buyLabelTMP != null)
        {
            buyLabelTMP.text = "Đã mua";
            buyLabelTMP.color = purchasedColor;
            return;
        }
#endif
        if (buyLabelUGUI != null)
        {
            buyLabelUGUI.text = "Đã mua";
            buyLabelUGUI.color = purchasedColor;
            return;
        }

        if (buyLabelGO != null)
        {
#if TMP_PRESENT
            var tmp = buyLabelGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = "Đã mua";
                tmp.color = purchasedColor;
                return;
            }
#endif
            var ugui = buyLabelGO.GetComponentInChildren<Text>(true);
            if (ugui != null)
            {
                ugui.text = "Đã mua";
                ugui.color = purchasedColor;
            }
        }
    }
}
