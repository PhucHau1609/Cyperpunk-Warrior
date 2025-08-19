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

    public void OnRegisterClick()
    {
        AudioManager.Instance?.PlayClickSFX();

        var email = emailInput != null ? emailInput.text : string.Empty;
        var password = passwordInput != null ? passwordInput.text : string.Empty;
        var name = nameInput != null ? nameInput.text : string.Empty;

        var dto = new RegisterDTO
        {
            username = name,
            email = email,
            password = password
        };
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
                var response = JsonConvert.DeserializeObject<RegisterModelResponse>(responseText);
                if (response != null)
                {
                    if (response.isSuccess)
                        messageManager?.ShowSuccess("Đăng ký thành công!");
                    else
                        messageManager?.ShowError(response.notification ?? "Đăng ký thất bại, vui lòng thử lại!");
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
