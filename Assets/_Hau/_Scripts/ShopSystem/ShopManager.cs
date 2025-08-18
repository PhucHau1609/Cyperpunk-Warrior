/*using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Items trong shop")]
    public ShopItemView[] items;

    [Header("UI chọn phương thức")]
    public PaymentMethodPanel methodPanel;

    [Header("Coordinator")]
    public PaymentCoordinator paymentCoordinator;

    // Cache item đang chọn
    private ShopItemView _currentSelected;

    void Start()
    {
        // Gắn hành vi Buy cho từng item
        foreach (var item in items)
        {
            if (item != null)
                item.BindBuy(OnItemBuyClicked);
        }
    }

    private void OnItemBuyClicked(ShopItemView item)
    {
        _currentSelected = item;

        // Mở panel chọn phương thức + truyền callback cho 2 nút
        methodPanel.Show(
            onPayOS: () =>
            {
                if (_currentSelected == null) return;
                paymentCoordinator.PayWithPayOS(_currentSelected.priceVnd, _currentSelected.displayName);
            },
            onZalo: () =>
            {
                if (_currentSelected == null) return;

                // Nếu thích QR tùy biến:
                paymentCoordinator.PayWithZaloQR(_currentSelected.priceVnd, _currentSelected.displayName);

                // Hoặc nếu dùng ZFBrowser của Payment.cs cho Zalo:
                // paymentCoordinator.payOSProvider.SetPaymentContext(_currentSelected.priceVnd, _currentSelected.displayName);
                // paymentCoordinator.payOSProvider.PayWithZalo();
            }
        );
    }
}
*/

using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Items trong shop")]
    public ShopItemView[] items;

    [Header("UI chọn phương thức")]
    public PaymentMethodPanel methodPanel;

    [Header("Coordinator")]
    public PaymentCoordinator paymentCoordinator;

    private ShopItemView _currentSelected;

    void Start()
    {
        foreach (var item in items)
            if (item != null) item.BindBuy(OnItemBuyClicked);

        // Nghe kết quả thanh toán
        paymentCoordinator.OnPaymentFinished += OnPaymentFinished;
    }

    private void OnDestroy()
    {
        if (paymentCoordinator != null)
            paymentCoordinator.OnPaymentFinished -= OnPaymentFinished;
    }

    // ShopManager.cs (đoạn quan trọng)
    private void OnItemBuyClicked(ShopItemView item)
    {
        _currentSelected = item;

        methodPanel.Show(
            onPayOS: () =>
            {
                if (_currentSelected == null) return;
                paymentCoordinator.PayWithPayOS(
                    _currentSelected.priceVnd,
                    _currentSelected.displayName,
                    _currentSelected   // <= chuyền item để biết reward
                );
            },
            onZalo: () =>
            {
                if (_currentSelected == null) return;
                paymentCoordinator.PayWithZaloQR(
                    _currentSelected.priceVnd,
                    _currentSelected.displayName,
                    _currentSelected
                );
            }
        );
    }

    private void OnPaymentFinished(bool success, ShopItemView item)
    {
        if (!success || item == null) return;
        item.ApplyPurchasedUI();
        // (Optional) PlayerPrefs.SetInt($"purchased_{item.itemId}", 1);
    }


    /*private void OnItemBuyClicked(ShopItemView item)
    {
        _currentSelected = item;

        methodPanel.Show(
            onPayOS: () =>
            {
                if (_currentSelected == null) return;
                paymentCoordinator.PayWithPayOS(_currentSelected.priceVnd, _currentSelected.displayName, _currentSelected);
            },
            onZalo: () =>
            {
                if (_currentSelected == null) return;
                paymentCoordinator.PayWithZaloQR(_currentSelected.priceVnd, _currentSelected.displayName, _currentSelected);
            }
        );
    }

    private void OnPaymentFinished(bool success, ShopItemView item)
    {
        if (!success || item == null) return;

        // Cập nhật UI "Đã mua"
        item.ApplyPurchasedUI();

        // (Tùy chọn) Lưu trạng thái đã mua (PlayerPrefs, save file…)
        // PlayerPrefs.SetInt($"purchased_{item.itemId}", 1);
    }*/

    public void ToggleShopItem()
    {
        this.gameObject.SetActive(!gameObject.activeSelf);
    }
}
