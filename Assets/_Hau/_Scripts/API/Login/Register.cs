using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems; // để dùng EventTrigger

public class Register : MonoBehaviour
{
    // Đổi sang InputField thường
    private readonly Dictionary<InputField, string> inputHistory = new();
    private InputField currentInputField;

    [Header("InputField")]
    public InputField emailInput;
    public InputField passwordInput;
    public InputField nameInput;

    [Header("Message")]
    public UIMessageManager messageManager;

    void Start()
    {
        InputField[] inputs = { emailInput, passwordInput, nameInput };
        foreach (var input in inputs)
        {
            if (input == null) continue;

            // gõ
            input.onValueChanged.AddListener(_ => OnInputTyping());

            // chọn (focus) – InputField không có onSelect ⇒ dùng EventTrigger
            AddSelectListener(input, () => OnInputSelected(input));

            // cache text ban đầu
            inputHistory[input] = input.text;
        }
    }

    private void AddSelectListener(InputField input, Action onSelected)
    {
        var go = input.gameObject;
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        entry.callback.AddListener(_ => onSelected?.Invoke());
        trigger.triggers.Add(entry);
    }

    void OnInputTyping()
    {
        if (currentInputField == null) return;

        string currentText = currentInputField.text;
        string previousText = inputHistory.TryGetValue(currentInputField, out var prev) ? prev : string.Empty;

        if (currentText != previousText)
            AudioManager.Instance?.PlayTypingSFX();

        inputHistory[currentInputField] = currentText;
    }

    void OnInputSelected(InputField input)
    {
        currentInputField = input;
        // (tuỳ chọn) âm click khi focus
        AudioManager.Instance?.PlayClickSFX();

        if (!inputHistory.ContainsKey(input))
            inputHistory[input] = input.text;
    }

    void OnDisable()
    {
        currentInputField = null;
    }

    public void GoToLogin()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }

    private static bool IsValidGmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        email = email.Trim().ToLowerInvariant();
        // kiểm tra đúng @gmail.com
        return System.Text.RegularExpressions.Regex.IsMatch(
            email, @"^[A-Za-z0-9._%+\-]+@gmail\.com$");
    }

    public void OnRegisterClick()
    {
        AudioManager.Instance?.PlayClickSFX();

        var email = emailInput ? emailInput.text.Trim() : string.Empty;
        var password = passwordInput ? passwordInput.text : string.Empty;
        var name = nameInput ? nameInput.text.Trim() : string.Empty;

        // Pre-validate rõ ràng
        if (string.IsNullOrEmpty(name))
        {
            messageManager?.ShowError("Tên không được để trống");
            return;
        }
        if (!IsValidGmail(email))
        {
            messageManager?.ShowError("Chỉ chấp nhận email đuôi @gmail.com");
            return;
        }
        if (password == null || password.Length < 6 || password.Length > 15)
        {
            messageManager?.ShowError("Mật khẩu phải từ 6 đến 15 ký tự");
            return;
        }

        var dto = new RegisterDTO { username = name, email = email, password = password };
        var json = JsonUtility.ToJson(dto);
        StartCoroutine(Post(json));
    }


    IEnumerator Post(string json)
    {
        //var url = "https://apiv3-sunny.up.railway.app/api/Register/register";
        var url = "http://localhost:5235/api/Register/register";

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<RegisterModelResponse>(responseText);
                messageManager?.ShowError(errorResponse?.notification ?? "Lỗi kết nối đến máy chủ!");
            }
            catch
            {
                messageManager?.ShowError("Lỗi mạng hoặc máy chủ không phản hồi!");
            }
        }
        else
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ApiResponse<object>>(responseText);
                if (response != null)
                {
                    if (response.Success)
                        messageManager?.ShowSuccess(response.Message ?? "Đăng ký thành công!");
                    else
                        messageManager?.ShowError(response.Message ?? "Đăng ký thất bại, vui lòng thử lại!");
                }
                else
                {
                    messageManager?.ShowError("Phản hồi không hợp lệ từ server.");
                }
            }
            catch (JsonReaderException)
            {
                messageManager?.ShowError("Lỗi khi phân tích phản hồi từ server.");
            }

        }
    }
}
