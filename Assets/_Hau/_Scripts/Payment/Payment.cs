using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser; // ZFBrowser + SimpleJSON

public class Payment : MonoBehaviour
{
    [Header("Common UI")]
    public Browser browser; // ZFBrowser
    public string cancelUrl = "https://pee2306.github.io/SunnyHome?cancel";
    public string returnUrl = "https://pee2306.github.io/SunnyHome?return";
    [Tooltip("Mô tả đơn hàng hiển thị ở cổng thanh toán")]
    public string description = "UNITY_ORDER";
    [Tooltip("Số tiền (đơn vị VNĐ)")]
    public int amount = 5000;

    [Header("PayOS Keys")]
    public string xClientId = "ad0d17ea-12fb-4613-b0a1-81b866c84e80";
    public string xApiKey = "3c9e4f20-bb0f-44e4-8529-fa0d1d651e1a";
    public string checksumKey = "9cfbe621228779e04e3b820e30fb4c111fde875c57053f771b35192f57ba64c7";
    public string partnerCodeOptional;

    [Header("ZaloPay (Sandbox)")]
    public string zaloAppId = "554";
    public string zaloKey1 = "8NdU5pG5R2spGHGhyO99HN1OhD8IQJBn";
    public string zaloKey2 = "uUfsWgfLkRLzq6W2uNXTCxrfxs51auny";
    public string zaloAppUser = "unity_user_001";
    public string zaloCreateOrderUrl = "https://sandbox.zalopay.com.vn/v001/tpe/createorder";
    public string zaloQueryStatusUrl = "https://sandbox.zalopay.com.vn/v001/tpe/getstatusbyapptransid";

    [Header("ZaloPay Browser Settings")]
    [Tooltip("User Agent cho ZaloPay để cải thiện tương thích")]
    public string zaloUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    [Tooltip("Timeout để fallback sang system browser (giây)")]
    public float zaloEmbeddedTimeout = 5f;

    [Tooltip("Có tự động fallback sang system browser không?")]
    public bool enableZaloFallback = true;

    public enum ZaloOpenMode { EmbeddedOnly, SystemBrowserOnly, AutoFallback }
    [Header("Zalo options")]
    public ZaloOpenMode zaloOpenMode = ZaloOpenMode.AutoFallback;

    private string _lastZaloOrderUrl;
    private Coroutine _zaloTimeoutCoroutine;

    // Event system
    public enum PaymentStatus { Success, Cancelled, Error }
    public struct PaymentResult
    {
        public PaymentStatus status;
        public long orderCode;
        public string provider;
        public string errorMessage;
    }
    public event Action<PaymentResult> OnPaymentCompleted;

    // Private state
    private long _lastOrderCode;
    private string _zaloAppTransId;
    private bool _usingZaloFlow = false;

    void Start()
    {
        // Configure browser for better ZaloPay compatibility
        if (browser != null)
        {
            browser.WhenReady(() => {
                // Set user agent for better compatibility
                browser.EvalJS($"navigator.__defineGetter__('userAgent', function(){{ return '{zaloUserAgent}'; }});");

                // Enable some browser features that ZaloPay might need
                browser.EvalJS(@"
                    // Enable some modern browser features
                    if (!window.Promise) {
                        console.log('Promise polyfill might be needed');
                    }
                    if (!window.fetch) {
                        console.log('Fetch polyfill might be needed');
                    }
                ");
            });
        }
    }

    // Public API
    public void PayWithPayOS()
    {
        _usingZaloFlow = false;
        StartCoroutine(Co_PayOS_CreatePaymentLink());
    }

    public void PayWithZalo()
    {
        _usingZaloFlow = true;

        switch (zaloOpenMode)
        {
            case ZaloOpenMode.SystemBrowserOnly:
                StartCoroutine(Co_Zalo_CreateOrder_SystemBrowser());
                break;
            case ZaloOpenMode.EmbeddedOnly:
                StartCoroutine(Co_Zalo_CreateOrder_EmbeddedOnly());
                break;
            case ZaloOpenMode.AutoFallback:
                StartCoroutine(Co_Zalo_CreateOrder_AutoFallback());
                break;
        }
    }

    public void SetPaymentContext(int amount, string description)
    {
        this.amount = amount;
        this.description = string.IsNullOrWhiteSpace(description) ? "UNITY_ORDER" : description;
    }

    // UI helpers
    private void ShowBrowserUI()
    {
        var rawImage = browser ? browser.GetComponent<RawImage>() : null;
        if (rawImage) rawImage.enabled = true;
    }

    private void HideBrowserUI()
    {
        var rawImage = browser ? browser.GetComponent<RawImage>() : null;
        if (rawImage) rawImage.enabled = false;
    }

    private void BindOnLoadHandler()
    {
        browser.onLoad -= OnBrowserLoad_PayOS;
        browser.onLoad -= OnBrowserLoad_Zalo;

        if (_usingZaloFlow) browser.onLoad += OnBrowserLoad_Zalo;
        else browser.onLoad += OnBrowserLoad_PayOS;
    }

    private void OpenInEmbeddedBrowser(string url)
    {
        if (browser == null)
        {
            Debug.LogError("[Payment] Browser is null!");
            return;
        }
        ShowBrowserUI();
        BindOnLoadHandler();
        browser.Url = url;
    }

    // PayOS Flow (unchanged)
    private void OnBrowserLoad_PayOS(JSONNode data)
    {
        string currentUrl = browser.Url;
        Debug.Log("[PayOS] Loading: " + currentUrl);

        if (currentUrl.Contains("return"))
        {
            Debug.Log("✅ [PayOS] Thanh toán thành công");
            InventoryManager.Instance.AddItem(ItemCode.MachineGun_3, 1);
            HideBrowserUI();
            OnPaymentCompleted?.Invoke(new PaymentResult
            {
                status = PaymentStatus.Success,
                provider = "PayOS",
                orderCode = _lastOrderCode,
                errorMessage = null
            });
        }
        else if (currentUrl.Contains("cancel"))
        {
            Debug.Log("❌ [PayOS] Người dùng hủy");
            HideBrowserUI();
            OnPaymentCompleted?.Invoke(new PaymentResult
            {
                status = PaymentStatus.Cancelled,
                provider = "PayOS",
                orderCode = _lastOrderCode,
                errorMessage = null
            });
        }
    }

    private IEnumerator Co_PayOS_CreatePaymentLink()
    {
        ShowBrowserUI();

        _lastOrderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int expiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();

        string toSign =
            $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={_lastOrderCode}&returnUrl={returnUrl}";
        string signature = HmacSha256Hex(toSign, checksumKey);

        var reqBody = new CreatePaymentRequest
        {
            orderCode = _lastOrderCode,
            amount = amount,
            description = description,
            cancelUrl = cancelUrl,
            returnUrl = returnUrl,
            expiredAt = expiredAt,
            signature = signature,
            items = null
        };

        string json = JsonUtility.ToJson(reqBody);

        using (var www = new UnityWebRequest("https://api-merchant.payos.vn/v2/payment-requests", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-client-id", xClientId);
            www.SetRequestHeader("x-api-key", xApiKey);
            if (!string.IsNullOrEmpty(partnerCodeOptional))
                www.SetRequestHeader("x-partner-code", partnerCodeOptional);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[PayOS] {www.responseCode} - {www.error}\n{www.downloadHandler.text}");
                HideBrowserUI();
                OnPaymentCompleted?.Invoke(new PaymentResult
                {
                    status = PaymentStatus.Error,
                    provider = "PayOS",
                    orderCode = _lastOrderCode,
                    errorMessage = www.error
                });
                yield break;
            }

            string jsonText = www.downloadHandler.text;
            string checkoutUrl = ExtractCheckoutUrl(jsonText);
            if (!string.IsNullOrEmpty(checkoutUrl))
            {
                OpenInEmbeddedBrowser(checkoutUrl);
            }
            else
            {
                Debug.LogWarning("[PayOS] Không tìm thấy checkoutUrl: " + jsonText);
                HideBrowserUI();
                OnPaymentCompleted?.Invoke(new PaymentResult
                {
                    status = PaymentStatus.Error,
                    provider = "PayOS",
                    orderCode = _lastOrderCode,
                    errorMessage = "Missing checkoutUrl"
                });
            }
        }
    }

    // ZaloPay Flows
    private IEnumerator Co_Zalo_CreateOrder_SystemBrowser()
    {
        yield return StartCoroutine(Co_Zalo_CreateOrderBase());

        if (!string.IsNullOrEmpty(_lastZaloOrderUrl))
        {
            Debug.Log("[ZaloPay] Opening in system browser");
            Application.OpenURL(_lastZaloOrderUrl);
            // Show manual check UI for user
        }
    }

    private IEnumerator Co_Zalo_CreateOrder_EmbeddedOnly()
    {
        ShowBrowserUI();
        yield return StartCoroutine(Co_Zalo_CreateOrderBase());

        if (!string.IsNullOrEmpty(_lastZaloOrderUrl))
        {
            Debug.Log("[ZaloPay] Opening in embedded browser only");
            OpenInEmbeddedBrowser(_lastZaloOrderUrl);

        }
    }

    private IEnumerator Co_Zalo_CreateOrder_AutoFallback()
    {
        ShowBrowserUI();
        yield return StartCoroutine(Co_Zalo_CreateOrderBase());

        if (!string.IsNullOrEmpty(_lastZaloOrderUrl))
        {
            Debug.Log("[ZaloPay] Trying embedded browser with auto fallback");
            OpenInEmbeddedBrowser(_lastZaloOrderUrl);

            // Start timeout coroutine
            if (_zaloTimeoutCoroutine != null)
                StopCoroutine(_zaloTimeoutCoroutine);
            _zaloTimeoutCoroutine = StartCoroutine(Co_Zalo_EmbeddedTimeout());
        }
    }

    private IEnumerator Co_Zalo_CreateOrderBase()
    {
        string yymmdd = DateTime.Now.ToString("yyMMdd");
        _zaloAppTransId = $"{yymmdd}_{UnityEngine.Random.Range(100000, 999999)}";
        long apptime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string embeddata = $"{{\"redirecturl\":\"{returnUrl}\"}}";
        string item = "[]";

        string macData = $"{zaloAppId}|{_zaloAppTransId}|{zaloAppUser}|{amount}|{apptime}|{embeddata}|{item}";
        string mac = HmacSha256Hex(macData, zaloKey1);

        WWWForm form = new WWWForm();
        form.AddField("appid", zaloAppId);
        form.AddField("apptransid", _zaloAppTransId);
        form.AddField("appuser", zaloAppUser);
        form.AddField("apptime", apptime.ToString());
        form.AddField("amount", amount.ToString());
        form.AddField("embeddata", embeddata);
        form.AddField("item", item);
        form.AddField("description", description);
        form.AddField("mac", mac);

        using UnityWebRequest req = UnityWebRequest.Post(zaloCreateOrderUrl, form);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[ZaloPay] Request error: " + req.error);
            HideBrowserUI();
            OnPaymentCompleted?.Invoke(new PaymentResult
            {
                status = PaymentStatus.Error,
                provider = "ZaloPay",
                orderCode = 0,
                errorMessage = req.error
            });
            yield break;
        }

        string raw = req.downloadHandler.text;
        var response = JsonUtility.FromJson<ZaloCreateOrderResponse>(FixJsonHasOrderUrl(raw));
        if (response.returncode != 1 || string.IsNullOrEmpty(response.orderurl))
        {
            Debug.LogError("[ZaloPay] Create order failed: " + response.returnmessage);
            HideBrowserUI();
            OnPaymentCompleted?.Invoke(new PaymentResult
            {
                status = PaymentStatus.Error,
                provider = "ZaloPay",
                orderCode = 0,
                errorMessage = response.returnmessage
            });
            yield break;
        }

        _lastZaloOrderUrl = response.orderurl;
        Debug.Log($"[ZaloPay] Order URL created: {_lastZaloOrderUrl}");
    }

    private IEnumerator Co_Zalo_EmbeddedTimeout()
    {
        yield return new WaitForSeconds(zaloEmbeddedTimeout);

        // Check if still on spinner page or gateway page
        if (_usingZaloFlow && browser != null)
        {
            string currentUrl = browser.Url;
            Debug.Log($"[ZaloPay] Timeout check - Current URL: {currentUrl}");

            // Check if stuck on gateway or spinner
            if (currentUrl.Contains("cpgateway.zalopay.vn") ||
                currentUrl.Contains("loading") ||
                currentUrl == _lastZaloOrderUrl)
            {
                Debug.LogWarning("[ZaloPay] Embedded browser appears stuck, falling back to system browser");
                HideBrowserUI();
                Application.OpenURL(_lastZaloOrderUrl);

                // Show manual status check UI
                ShowManualStatusCheckUI();
            }
        }
    }

    private void ShowManualStatusCheckUI()
    {
        // You can implement a simple UI here for manual status checking
        Debug.Log("[ZaloPay] Please check payment status manually or implement UI for status checking");
    }

    // ZaloPay event handlers
    private void OnBrowserLoad_Zalo(JSONNode data)
    {
        string currentUrl = browser.Url;
        Debug.Log("[ZaloPay] Loading: " + currentUrl);

        // Stop timeout coroutine if we've moved past the gateway
        if (_zaloTimeoutCoroutine != null && !currentUrl.Contains("cpgateway.zalopay.vn"))
        {
            StopCoroutine(_zaloTimeoutCoroutine);
            _zaloTimeoutCoroutine = null;
        }

        // Check for return URL (payment completed)
        if (!string.IsNullOrEmpty(returnUrl) && currentUrl.Contains(new Uri(returnUrl).Host))
        {
            HideBrowserUI();
            StartCoroutine(Co_Zalo_QueryStatus());
        }

        // Log page information for debugging
        if (data != null) Debug.Log("[ZaloPay] Load data: " + data.ToString());

        // Additional debugging for ZaloPay pages
        if (browser != null)
        {
            browser.EvalJS("document.title").Then(title => {
                Debug.Log($"[ZaloPay] Page title: {title}");
            });

            browser.EvalJS("document.readyState").Then(state => {
                Debug.Log($"[ZaloPay] Document state: {state}");
            });
        }
    }

    public void ZaloCheckStatusNow()
    {
        if (string.IsNullOrEmpty(_zaloAppTransId))
        {
            Debug.LogWarning("[ZaloPay] Chưa có apptransid để query.");
            return;
        }
        StartCoroutine(Co_Zalo_QueryStatus());
    }

    private IEnumerator Co_Zalo_QueryStatus()
    {
        string mac = HmacSha256Hex($"{zaloAppId}|{_zaloAppTransId}|{zaloKey1}", zaloKey1);

        WWWForm form = new WWWForm();
        form.AddField("appid", zaloAppId);
        form.AddField("apptransid", _zaloAppTransId);
        form.AddField("mac", mac);

        using UnityWebRequest req = UnityWebRequest.Post(zaloQueryStatusUrl, form);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string raw = req.downloadHandler.text;
            Debug.Log("[ZaloPay] Status response: " + raw);

            if (raw.Contains("\"returncode\":1"))
            {
                OnPaymentCompleted?.Invoke(new PaymentResult
                {
                    status = PaymentStatus.Success,
                    provider = "ZaloPay",
                    orderCode = 0,
                    errorMessage = null
                });
            }
            else
            {
                OnPaymentCompleted?.Invoke(new PaymentResult
                {
                    status = PaymentStatus.Error,
                    provider = "ZaloPay",
                    orderCode = 0,
                    errorMessage = raw
                });
            }
        }
        else
        {
            Debug.LogError("[ZaloPay] Status error: " + req.error);
            OnPaymentCompleted?.Invoke(new PaymentResult
            {
                status = PaymentStatus.Error,
                provider = "ZaloPay",
                orderCode = 0,
                errorMessage = req.error
            });
        }
    }

    // Helper methods
    static string HmacSha256Hex(string message, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    static string ExtractCheckoutUrl(string json)
    {
        const string key = "\"checkoutUrl\":\"";
        int i = json.IndexOf(key, StringComparison.Ordinal);
        if (i < 0) return null;
        i += key.Length;
        int j = json.IndexOf("\"", i, StringComparison.Ordinal);
        if (j < 0) return null;
        return json.Substring(i, j - i).Replace("\\/", "/");
    }

    private static string FixJsonHasOrderUrl(string raw)
    {
        if (!raw.Contains("\"orderurl\""))
        {
            raw = raw.TrimEnd('}') + ",\"orderurl\":\"\"}";
        }
        return raw;
    }

    [Serializable]
    private class ZaloCreateOrderResponse
    {
        public int returncode;
        public string returnmessage;
        public string orderurl;
        public string zptranstoken;
    }

    [Serializable]
    public class PayOSItem { public string name; public int quantity; public int price; }

    [Serializable]
    public class CreatePaymentRequest
    {
        public long orderCode;
        public int amount;
        public string description;
        public string cancelUrl;
        public string returnUrl;
        public int expiredAt;
        public string signature;
        public List<PayOSItem> items;
    }

    void OnDestroy()
    {
        if (_zaloTimeoutCoroutine != null)
        {
            StopCoroutine(_zaloTimeoutCoroutine);
        }
    }
}