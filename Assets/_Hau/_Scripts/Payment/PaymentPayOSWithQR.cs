/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PaymentPayOSWithQR : MonoBehaviour
{
    public UniWebView webView;
    public string xClientId = "ad0d17ea-12fb-4613-b0a1-81b866c84e80"; //lấy trên PayOS
    public string xApiKey = "3c9e4f20-bb0f-44e4-8529-fa0d1d651e1a"; //lấy trên PayOS
    public string checksumKey = "9cfbe621228779e04e3b820e30fb4c111fde875c57053f771b35192f57ba64c7"; //lấy trên PayOS
    public string partnerCodeOptional;

    public int amount = 6000; //thay đổi số tiền cần thanh toán
    public string description = "UNITY01"; //tự tạo mã đơn nếu muốn
    public string cancelUrl = "https://dinhnt.com/cancel"; //có thể thay bằng link khác
    public string returnUrl = "https://dinhnt.com/return"; //có thể thay bằng link khác

    long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // đảm bảo duy nhất
    private bool isChecking = false;

    public QRDemo qRDemo;
    public Text txtResult;

    public void CreatePaymentPayOSWithQR()
    {
        txtResult.text = "Đang thực hiện thanh toán bằng phương thức QR PayOS";
        StartCoroutine(CreatePayOSQR());
    }

    IEnumerator CreatePayOSQR()
    {
        int expiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();

        //Tạo data để ký theo đúng thứ tự alphabet
        string toSign =
            $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";

        //HMAC_SHA256 với checksumKey
        string signature = HmacSha256Hex(toSign, checksumKey);

        //Tạo payload
        var req = new CreatePaymentQRRequest
        {
            orderCode = orderCode,
            amount = amount,
            description = description,
            cancelUrl = cancelUrl,
            returnUrl = returnUrl,
            expiredAt = expiredAt,
            signature = signature,
            items = null
        };

        string json = JsonUtility.ToJson(req);

        //Gọi API
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
                Debug.LogError($"PayOS error: {www.responseCode} - {www.error}\n{www.downloadHandler.text}");
                yield break;
            }
            else
            {
                var jsonText = www.downloadHandler.text;
                Debug.Log(jsonText);
                PayOSResponse response = JsonUtility.FromJson<PayOSResponse>(jsonText);
                if (response.data.qrCode != null)
                {
                    qRDemo.GenQR(response.data.qrCode);

                    // Sau khi tạo đơn, thử gọi query trạng thái
                    if (!isChecking)
                    {
                        StartCoroutine(CheckPaymentLoop());
                    }
                }
            }
        }
    }

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

    IEnumerator CheckPaymentLoop()
    {
        isChecking = true;
        float elapsed = 0f;

        while (isChecking && elapsed < 120f)
        {
            yield return StartCoroutine(CheckPaymentStatus());

            if (!isChecking) // đã thanh toán thành công
                yield break;

            elapsed += 3f;
            yield return new WaitForSeconds(3f);
        }

        if (isChecking)
        {
            Debug.Log("⏱ Hết thời gian chờ, chưa thấy thanh toán.");
            isChecking = false;
        }
    }

    IEnumerator CheckPaymentStatus()
    {
        string url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-client-id", xClientId.Trim());
        www.SetRequestHeader("x-api-key", xApiKey.Trim());
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("PayOS QR Error: " + www.error);
        }
        else
        {
            PayOSStatusResponse resp = JsonUtility.FromJson<PayOSStatusResponse>(www.downloadHandler.text);
            if (resp != null && resp.data != null)
            {
                Debug.Log("📡 Order status: " + resp.data.status);
                if (resp.data.status == "PAID")
                {
                    txtResult.text = "Thanh toán thành công! (PayOS QR)";
                    isChecking = false;
                    yield break; // thoát loop
                }
            }
        }
    }
}

[Serializable]
public class PayOSQRItem
{
    public string name;
    public int quantity;
    public int price;
}

[Serializable]
public class CreatePaymentQRRequest
{
    public long orderCode;
    public int amount;
    public string description;
    public string cancelUrl;
    public string returnUrl;
    public int expiredAt;
    public string signature;
    public List<PayOSQRItem> items;
}

[Serializable]
public class PayOSResponse
{
    public string code;
    public string desc;
    public PayOSData data;
}

[Serializable]
public class PayOSData
{
    public string id;
    public string orderCode;
    public string checkoutUrl;
    public string qrCode;
}

[Serializable]
public class PayOSStatusResponse
{
    public string code;
    public string desc;
    public PayOSStatusData data;
}

[Serializable]
public class PayOSStatusData
{
    public string id;
    public string orderCode;
    public int amount;
    public string status;
}*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Payment;

public class PaymentPayOSWithQR : MonoBehaviour
{
    // ===== NEW: Events để coordinator subscribe =====
    public event Action<long, int> OnPaid;            // (orderCode, amount)
    public event Action<long, string> OnFailed;       // (orderCode, reason) - optional
    public event Action<string> OnQrReady;            // trả ra data string để render QR nếu muốn

    [Header("API Keys (demo)")]
    public string xClientId = "ad0d17ea-12fb-4613-b0a1-81b866c84e80";
    public string xApiKey = "3c9e4f20-bb0f-44e4-8529-fa0d1d651e1a";
    public string checksumKey = "9cfbe621228779e04e3b820e30fb4c111fde875c57053f771b35192f57ba64c7";
    public string partnerCodeOptional;

    [Header("Payment Context")]
    public int amount = 6000;
    public string description = "UNITY01";
    public string cancelUrl = "https://dinhnt.com/cancel";
    public string returnUrl = "https://dinhnt.com/return";

    [Header("UI (optional)")]
    public QRDemo qRDemo;
    public Text txtResult;

    private long orderCode;
    private bool isChecking = false;

    // ===== NEW: Set context từ ngoài vào thay vì sửa biến public trực tiếp =====
    public void SetContext(int amountVnd, string desc)
    {
        amount = amountVnd;
        description = desc;
        orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // đảm bảo duy nhất cho mỗi lần gọi
    }

    public void CreatePaymentPayOSWithQR()
    {
        if (txtResult) txtResult.text = "Đang thực hiện thanh toán bằng phương thức QR PayOS";
        // orderCode nếu chưa set (trường hợp quên SetContext)
        if (orderCode == 0) orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        StartCoroutine(CreatePayOSQR());
    }

    IEnumerator CreatePayOSQR()
    {
        int expiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();

        // Tạo data để ký theo đúng thứ tự alphabet
        string toSign = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        string signature = HmacSha256Hex(toSign, checksumKey);

        var req = new CreatePaymentQRRequest
        {
            orderCode = orderCode,
            amount = amount,
            description = description,
            cancelUrl = cancelUrl,
            returnUrl = returnUrl,
            expiredAt = expiredAt,
            signature = signature,
            items = null
        };

        string json = JsonUtility.ToJson(req);

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
                Debug.LogError($"PayOS error: {www.responseCode} - {www.error}\n{www.downloadHandler.text}");
                OnFailed?.Invoke(orderCode, "create_failed");
                yield break;
            }
            else
            {
                var jsonText = www.downloadHandler.text;
                Debug.Log(jsonText);
                PayOSResponse response = JsonUtility.FromJson<PayOSResponse>(jsonText);
                if (response != null && response.data != null && !string.IsNullOrEmpty(response.data.qrCode))
                {
                    // render QR (nếu có qRDemo), đồng thời phát event
                    if (qRDemo) qRDemo.GenQR(response.data.qrCode);
                    OnQrReady?.Invoke(response.data.qrCode);

                    if (!isChecking) StartCoroutine(CheckPaymentLoop());
                }
                else
                {
                    OnFailed?.Invoke(orderCode, "no_qr");
                }
            }
        }
    }

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

    IEnumerator CheckPaymentLoop()
    {
        isChecking = true;
        float elapsed = 0f;

        while (isChecking && elapsed < 120f)
        {
            yield return StartCoroutine(CheckPaymentStatus());

            if (!isChecking) // đã thanh toán thành công
                yield break;

            elapsed += 3f;
            yield return new WaitForSeconds(3f);
        }

        if (isChecking)
        {
            Debug.Log("⏱ Hết thời gian chờ, chưa thấy thanh toán.");
            isChecking = false;
            OnFailed?.Invoke(orderCode, "timeout");
        }
    }

    IEnumerator CheckPaymentStatus()
    {
        string url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-client-id", xClientId.Trim());
        www.SetRequestHeader("x-api-key", xApiKey.Trim());
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("PayOS QR Error: " + www.error);
        }
        else
        {
            PayOSStatusResponse resp = JsonUtility.FromJson<PayOSStatusResponse>(www.downloadHandler.text);
            if (resp != null && resp.data != null)
            {
                Debug.Log("📡 Order status: " + resp.data.status);
                if (resp.data.status == "PAID")
                {
                    if (txtResult) txtResult.text = "Thanh toán thành công! (PayOS QR)";
                    isChecking = false;
                    OnPaid?.Invoke(orderCode, resp.data.amount);
                    yield break; // thoát loop
                }
            }
        }
    }
}

// ==== MODELS cho PayOS QR ====
[Serializable]
public class PayOSQRItem
{
    public string name;
    public int quantity;
    public int price;
}

[Serializable]
public class CreatePaymentQRRequest
{
    public long orderCode;
    public int amount;
    public string description;
    public string cancelUrl;
    public string returnUrl;
    public int expiredAt;
    public string signature;
    public List<PayOSQRItem> items;
}

[Serializable]
public class PayOSResponse
{
    public string code;
    public string desc;
    public PayOSData data;
}

[Serializable]
public class PayOSData
{
    public string id;
    public string orderCode;
    public string checkoutUrl;
    public string qrCode;
}

[Serializable]
public class PayOSStatusResponse
{
    public string code;
    public string desc;
    public PayOSStatusData data;
}

[Serializable]
public class PayOSStatusData
{
    public string id;
    public string orderCode;
    public int amount;
    public string status;
}

