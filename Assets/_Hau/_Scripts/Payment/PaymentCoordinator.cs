/*using UnityEngine;

// Coordinator giữ reference 2 provider và bật đúng UI theo lựa chọn
public class PaymentCoordinator : MonoBehaviour
{
    [Header("Providers")]
    public Payment payOSProvider;                  // script Payment (flow Browser PayOS/Zalo)
    public GameObject zaloPaymentQRPanel;         // panel chứa QR (RawImage, v.v.)
    public PaymentZaloPayWithQR zaloQRComponent;  // script tạo đơn + show QR

    [Header("Optional UI")]
    public GameObject embeddedBrowserRoot;        // RawImage chứa ZFBrowser (ẩn/hiện cho gọn)

    // Gọi khi chọn PayOS
    public void PayWithPayOS(int amountVnd, string description)
    {
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(true);

        // Payment.cs đã có SetPaymentContext + PayWithPayOS
        payOSProvider.SetPaymentContext(amountVnd, description);
        payOSProvider.PayWithPayOS();  // Hiển thị Browser (RawImage)
        // Tham chiếu: :contentReference[oaicite:0]{index=0}
    }

    // Gọi khi chọn ZaloPay (hiển thị QR panel custom)
    public void PayWithZaloQR(int amountVnd, string description)
    {
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(true);

        // Truyền amount/desc cho component Zalo QR (bạn có thể mở public set hoặc method)
        zaloQRComponent.amount = amountVnd;
        zaloQRComponent.description = description;

        // Bắt đầu tạo đơn → show QR → polling trạng thái
        zaloQRComponent.Payment();
        // Tham chiếu: :contentReference[oaicite:1]{index=1}, :contentReference[oaicite:2]{index=2}
    }

    // Nếu bạn muốn dùng Zalo ngay trong embedded browser của Payment.cs,
    // có thể thay bằng:
    // payOSProvider.SetPaymentContext(amountVnd, description);
    // payOSProvider.PayWithZalo();
    // (khi đó không cần zaloPaymentQRPanel)
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
