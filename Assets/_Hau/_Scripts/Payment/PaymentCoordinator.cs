/*using System;
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
*/

using System;
using UnityEngine;

public class PaymentCoordinator : MonoBehaviour
{
    [Header("Providers (cũ)")]
    public Payment payOSProvider;                  // flow WebView cũ
    public GameObject embeddedBrowserRoot;        // RawImage Browser (ẩn/hiện)

    [Header("ZaloPay QR (cũ)")]
    public GameObject zaloPaymentQRPanel;
    public PaymentZaloPayWithQR zaloQRComponent;

    [Header("PayOS QR (mới)")]
    public GameObject payOSQRPanel;               // Panel chứa Image QR/Text…
    public PaymentPayOSWithQR payOSQRComponent;   // Script mới ở trên

    /// <summary> success = true khi thanh toán thành công; item = item đã mua </summary>
    public event Action<bool, ShopItemView> OnPaymentFinished;

    [Header("Result Panel")]
    public PurchaseResultPanel resultPanel;

    private ShopItemView _pendingItem;

    // ========= FLOW CŨ: WEBVIEW =========
    public void PayWithPayOS(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        if (payOSQRPanel) payOSQRPanel.SetActive(false);
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(true);

        payOSProvider.SetPaymentContext(amountVnd, description);
        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
        payOSProvider.OnPaymentCompleted += OnPayOSCompleted;
        payOSProvider.PayWithPayOS();
    }

    private void OnPayOSCompleted(Payment.PaymentResult result)
    {
        var item = _pendingItem; _pendingItem = null;
        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);

        bool success = (result.status == Payment.PaymentStatus.Success);
        if (success) GrantRewardFor(item);

        if (resultPanel)
        {
            // đảm bảo PaymentResult có provider/amount/orderCode; nếu chưa có, bạn có thể gán tạm
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                result.amount,
                success,
                string.IsNullOrEmpty(result.provider) ? "PayOS (Web)" : result.provider,
                result.orderCode.ToString()
            );
        }

        OnPaymentFinished?.Invoke(success, item);
        payOSProvider.OnPaymentCompleted -= OnPayOSCompleted;
    }

    // ========= FLOW MỚI: PAYOS QR =========
    public void PayWithPayOSQR(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        if (payOSQRPanel) payOSQRPanel.SetActive(true);

        if (payOSQRComponent == null)
        {
            Debug.LogWarning("[PaymentCoordinator] payOSQRComponent chưa được gán.");
            return;
        }

        // Hủy đăng ký trước đó để tránh đăng ký trùng
        payOSQRComponent.OnPaid -= OnPayOSQR_Paid;
        payOSQRComponent.OnFailed -= OnPayOSQR_Failed;

        // Đăng ký callback
        payOSQRComponent.OnPaid += OnPayOSQR_Paid;
        payOSQRComponent.OnFailed += OnPayOSQR_Failed;

        // Set ngữ cảnh & bắt đầu tạo QR
        payOSQRComponent.SetContext(amountVnd, description);
        payOSQRComponent.CreatePaymentPayOSWithQR();
    }

    private void OnPayOSQR_Paid(long orderCode, int amount)
    {
        var item = _pendingItem; _pendingItem = null;

        if (payOSQRPanel) payOSQRPanel.SetActive(false);

        GrantRewardFor(item);

        if (resultPanel)
        {
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                amount,
                true,
                "PayOS (QR)",
                orderCode.ToString()
            );
        }

        OnPaymentFinished?.Invoke(true, item);

        // ngắt đăng ký sau khi xong
        if (payOSQRComponent != null)
        {
            payOSQRComponent.OnPaid -= OnPayOSQR_Paid;
            payOSQRComponent.OnFailed -= OnPayOSQR_Failed;
        }
    }

    private void OnPayOSQR_Failed(long orderCode, string reason)
    {
        var item = _pendingItem; _pendingItem = null;

        if (payOSQRPanel) payOSQRPanel.SetActive(false);

        if (resultPanel)
        {
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                item != null ? item.priceVnd : 0,
                false,
                "PayOS (QR)",
                orderCode.ToString()
            );
        }

        OnPaymentFinished?.Invoke(false, item);

        if (payOSQRComponent != null)
        {
            payOSQRComponent.OnPaid -= OnPayOSQR_Paid;
            payOSQRComponent.OnFailed -= OnPayOSQR_Failed;
        }
    }

    // ========= ZALO QR (giữ nguyên) =========
    public void PayWithZaloQR(int amountVnd, string description, ShopItemView itemRef)
    {
        _pendingItem = itemRef;

        if (embeddedBrowserRoot) embeddedBrowserRoot.SetActive(false);
        if (payOSQRPanel) payOSQRPanel.SetActive(false);
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

    public void NotifyZaloPaymentSuccess()
    {
        var item = _pendingItem; _pendingItem = null;
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);

        GrantRewardFor(item);

        if (resultPanel)
        {
            resultPanel.Show(
                item != null ? item.displayName : "(Không rõ)",
                item != null ? item.priceVnd : 0,
                true,
                "ZaloPay"
            );
        }

        OnPaymentFinished?.Invoke(true, item);
    }

    public void NotifyZaloPaymentFailed()
    {
        var item = _pendingItem; _pendingItem = null;
        if (zaloPaymentQRPanel) zaloPaymentQRPanel.SetActive(false);
        OnPaymentFinished?.Invoke(false, item);
    }

    // ========= REWARD =========
    private void GrantRewardFor(ShopItemView item)
    {
        if (item == null)
        {
            Debug.LogWarning("[PaymentCoordinator] GrantRewardFor: item null");
        }
        else
        {
            int qty = Mathf.Max(1, item.rewardQuantity);
            InventoryManager.Instance.AddItem(item.rewardItemCode, qty);
        }
    }
}
