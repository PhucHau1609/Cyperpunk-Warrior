/*using System;
using UnityEngine;

public class PaymentCoordinator : MonoBehaviour
{
    [Header("Providers")]
    public Payment payOSProvider;                  // có flow Browser
    public GameObject zaloPaymentQRPanel;         // panel QR nếu bạn dùng QR riêng
    public PaymentZaloPayWithQR zaloQRComponent;  // script QR (tùy chọn)
    public GameObject embeddedBrowserRoot;        // RawImage Browser (ẩn/hiện)

    public event Action<bool, ShopItemView> OnPaymentFinished;

    private ShopItemView _pendingItem;

    // ======== PAYOS ========
    public void PayWithPayOS(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(true);

        // Đặt context & gọi flow PayOS
        payOSProvider.SetPaymentContext(amountVnd, description);

        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
        payOSProvider.OnPaymentCompleted += OnPayOSCompleted;

        payOSProvider.PayWithPayOS();

    }

    private void OnPayOSCompleted(Payment.PaymentResult result)
    {
        var item = _pendingItem;
        _pendingItem = null;

        if (embeddedBrowserRoot)
            embeddedBrowserRoot.SetActive(false);

        // Gọi OnPaymentFinished với true/false dựa vào result.status
        bool success = (result.status == Payment.PaymentStatus.Success);
        OnPaymentFinished?.Invoke(success, item);

        // Hủy đăng ký để tránh leak
        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
    }


    // ======== ZALO QR ========
    public void PayWithZaloQR(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(true);

        zaloQRComponent.amount = amountVnd;
        zaloQRComponent.description = description;

        // Nếu script QR có event riêng, bạn cũng có thể subscribe tương tự:
        // zaloQRComponent.OnPaymentSuccess -= OnZaloQRSuccess;
        // zaloQRComponent.OnPaymentSuccess += OnZaloQRSuccess;

        zaloQRComponent.Payment();
    }

    // Gọi hàm này từ script Zalo QR khi status == PAID
    public void NotifyZaloPaymentSuccess()
    {
        var item = _pendingItem;
        _pendingItem = null;
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);

        OnPaymentFinished?.Invoke(true, item);
    }

    // (Nếu bạn muốn bắt cả fail/cancel từ QR, có thể thêm NotifyZaloPaymentFailed() tương tự)
}
*/

using System;
using UnityEngine;

public class PaymentCoordinator : MonoBehaviour
{
    [Header("Providers")]
    public Payment payOSProvider;                  // có flow Browser
    public GameObject zaloPaymentQRPanel;         // panel QR nếu bạn dùng QR riêng
    public PaymentZaloPayWithQR zaloQRComponent;  // script QR (tùy chọn)
    public GameObject embeddedBrowserRoot;        // RawImage Browser (ẩn/hiện)

    /// <summary>
    /// success = true khi thanh toán thành công; item = item đã mua (để UI "Đã mua")
    /// </summary>
    public event Action<bool, ShopItemView> OnPaymentFinished;

    [Header("Result Panel")]
    public PurchaseResultPanel resultPanel;

    private ShopItemView _pendingItem;

    // ======== PAYOS ========
    public void PayWithPayOS(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(true);

        // Đặt context & gọi flow PayOS
        payOSProvider.SetPaymentContext(amountVnd, description);

        // Tránh đăng ký trùng lặp
        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
        payOSProvider.OnPaymentCompleted += OnPayOSCompleted;

        payOSProvider.PayWithPayOS();
    }

    private void OnPayOSCompleted(Payment.PaymentResult result)
    {
        var item = _pendingItem;   // giữ tham chiếu trước khi reset
        _pendingItem = null;

        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);

        bool success = (result.status == Payment.PaymentStatus.Success);

        if (success)
        {
            GrantRewardFor(item);
        }

        // Hiển thị panel
        if (resultPanel)
        {
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                result.amount,  // thêm field amount vào PaymentResult nếu chưa có
                success,
                result.provider,
                result.orderCode
            );
        }

        OnPaymentFinished?.Invoke(success, item);

        // Hủy đăng ký để tránh leak
        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
    }

    // ======== ZALO QR ========
    public void PayWithZaloQR(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(true);

        if (zaloQRComponent != null)
        {
            zaloQRComponent.amount = amountVnd;
            zaloQRComponent.description = description;
            zaloQRComponent.Payment();
        }
        else
        {
            Debug.LogWarning("[PaymentCoordinator] zaloQRComponent chưa được gán.");
        }
    }

    /// <summary>
    /// Gọi hàm này từ script Zalo QR khi status == PAID (đã thanh toán)
    /// </summary>
    public void NotifyZaloPaymentSuccess()
    {
        var item = _pendingItem;
        _pendingItem = null;

        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);

        GrantRewardFor(item);

        if (resultPanel)
        {
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                item != null ? item.priceVnd : 0,
                true,
                "ZaloPay",
                1
            );
        }

        OnPaymentFinished?.Invoke(true, item);
    }

    /// <summary>
    /// (Tùy chọn) Gọi nếu QR báo lỗi/hủy
    /// </summary>
    public void NotifyZaloPaymentFailed()
    {
        var item = _pendingItem;
        _pendingItem = null;

        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);

        OnPaymentFinished?.Invoke(false, item);
    }

    // ======== REWARD CENTER ========
    private void GrantRewardFor(ShopItemView item)
    {
        if (item == null)
        {
            Debug.LogWarning("[PaymentCoordinator] GrantRewardFor: item null");
            return;
        }

        int qty = Mathf.Max(1, item.rewardQuantity);
        InventoryManager.Instance.AddItem(item.rewardItemCode, qty);
        // Nếu muốn chống cấp trùng theo orderCode/appTransId: thêm HashSet kiểm tra ở đây.
    }
}
