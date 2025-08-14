using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems; // <- để dùng EventTrigger

public class LoginManager : MonoBehaviour
{
    // Đổi sang InputField (UI thường)
    private Dictionary<InputField, string> inputHistory = new Dictionary<InputField, string>();
    private InputField currentInputField;

    public GameObject loadingSpinner;

    [Header("InputField")]
    public InputField emailInput;
    public InputField passwordInput;

    [Header("Message")]
    public UIMessageManager messageManager;

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class UserData
    {
        public string email;
        public string userName;
        public string name;
        public string avatar;
        public int regionID;
    }

    [Serializable]
    public class LoginResponse
    {
        public bool isSuccess;
        public string notification;
        public UserData data;
    }

    void Start()
    {
        // Đăng ký lắng nghe cho InputField thường
        InputField[] inputs = { emailInput, passwordInput };

        foreach (var input in inputs)
        {
            if (input == null) continue;

            // 1) Gắn listener khi đang gõ
            input.onValueChanged.AddListener(_ => OnInputTyping());

            // 2) Gắn listener khi được chọn (InputField thường không có onSelect => dùng EventTrigger)
            AddSelectListener(input, () => OnInputSelected(input));

            // Lưu text ban đầu
            inputHistory[input] = input.text;
        }
    }

    // Gắn EventTrigger Entry Select để gọi callback khi input được chọn (focus)
    private void AddSelectListener(InputField input, Action onSelected)
    {
        var go = input.gameObject;
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        // Entry cho sự kiện Select
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
        {
            AudioManager.Instance?.PlayTypingSFX();
        }

        inputHistory[currentInputField] = currentText;
    }

    void OnInputSelected(InputField input)
    {
        currentInputField = input;
        AudioManager.Instance?.PlayClickSFX();

        // Đảm bảo có entry trong từ điển
        if (!inputHistory.ContainsKey(input))
            inputHistory[input] = input.text;
    }

    void OnDisable()
    {
        currentInputField = null;
    }

    public void OnLoginClick()
    {
        AudioManager.Instance?.PlayClickSFX();

        var email = emailInput != null ? emailInput.text : string.Empty;
        var password = passwordInput != null ? passwordInput.text : string.Empty;

        var loginRequest = new LoginRequest
        {
            email = email,
            password = password
        };

        var json = JsonConvert.SerializeObject(loginRequest);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        StartCoroutine(PostLogin(json));
    }

    IEnumerator PostLogin(string json)
    {
        string url = "https://apiv3-sunny.up.railway.app/api/Login/login";
        //string url = "http://localhost:5235/api/Login/login";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                messageManager?.ShowError(errorResponse?.notification ?? "Đã xảy ra lỗi kết nối đến máy chủ!");
            }
            catch
            {
                messageManager?.ShowError("Đã xảy ra lỗi kết nối đến máy chủ!");
            }
        }
        else
        {
            var responseText = request.downloadHandler.text;
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseText);

            if (loginResponse != null && loginResponse.isSuccess)
            {
                messageManager?.ShowSuccess("Đăng nhập thành công!");

                AudioManager.Instance?.StopBGM();

                if (UserSession.Instance != null && loginResponse.data != null)
                {
                    UserSession.Instance.UserId = loginResponse.data.regionID;
                }

                StartCoroutine(LoadSceneAfterDelay(.5f));
            }
            else
            {
                messageManager?.ShowError(loginResponse?.notification ?? "Đăng nhập thất bại!");
            }
        }
    }

    IEnumerator LoadSceneAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(1);
    }
}
