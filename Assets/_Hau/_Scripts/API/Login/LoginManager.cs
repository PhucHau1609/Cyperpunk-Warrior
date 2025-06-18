using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static LoginManager;

public class LoginManager : MonoBehaviour
{
    private Dictionary<TMP_InputField, string> inputHistory = new Dictionary<TMP_InputField, string>();
    private TMP_InputField currentInputField;
    public GameObject loadingSpinner;
    [Header("InputField")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

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
        TMP_InputField[] inputs = { emailInput, passwordInput };
        foreach (var input in inputs)
        {
            input.onSelect.AddListener(delegate { OnInputSelected(input); });
            input.onValueChanged.AddListener(delegate { OnInputTyping(); });
            inputHistory[input] = input.text;
        }
    }

    void OnInputTyping()
    {
        if (currentInputField == null) return;

        string currentText = currentInputField.text;
        string previousText = inputHistory[currentInputField];

        if (currentText != previousText)
        {
            AudioManager.Instance?.PlayTypingSFX();
        }

        inputHistory[currentInputField] = currentText;
    }

    void OnInputSelected(TMP_InputField input)
    {
        currentInputField = input;
        AudioManager.Instance?.PlayClickSFX();
    }

    void OnDisable()
    {
        currentInputField = null;
    }

    public void OnLoginClick()
    {
        AudioManager.Instance?.PlayClickSFX();

        var email = emailInput.text;
        var password = passwordInput.text;

        var loginRequest = new LoginRequest
        {
            email = email,
            password = password
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        loadingSpinner.SetActive(true);
        StartCoroutine(PostLogin(json));
    }

    IEnumerator PostLogin(string json)
    {
        //string url = "https://apiv3-sunny.up.railway.app/api/Login/login";
        string url = "http://localhost:5235/api/Login/login";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        loadingSpinner.SetActive(false);

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                messageManager.ShowError(errorResponse.notification);
            }
            catch
            {
                messageManager.ShowError("Đã xảy ra lỗi kết nối đến máy chủ!");
            }
        }
        else
        {
            var responseText = request.downloadHandler.text;
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseText);

            if (loginResponse.isSuccess)
            {
                messageManager.ShowSuccess("Đăng nhập thành công!");

                AudioManager.Instance?.StopBGM();

                if (UserSession.Instance != null)
                {
                    UserSession.Instance.UserId = loginResponse.data.regionID;
                }

                StartCoroutine(LoadSceneAfterDelay(.5f));
            }
            else
            {
                messageManager.ShowError(loginResponse.notification);
            }
        }
    }

    IEnumerator LoadSceneAfterDelay(float delaySeconds)
    {
        //loadingSpinner.SetActive(true); // Bật spinner nếu đã tắt
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(1);
    }

}

/*
  IEnumerator PostLogin(string json)
    {
        string url = "https://apiv3-sunny.up.railway.app/api/Login/login";

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        loadingSpinner.SetActive(false);

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Login failed: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                messageManager.ShowError(errorResponse.notification);
            }
            catch
            {
                messageManager.ShowError("Đã xảy ra lỗi kết nối!");
            }
        }
        else
        {
            var responseText = request.downloadHandler.text;
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseText);

            if (loginResponse.isSuccess)
            {
                Debug.Log("Đăng nhập thành công!");
                messageManager.ShowSuccess("Đăng nhập thành công");
                AudioManager.Instance?.StopBGM();
                Debug.Log($"Tên người dùng: {loginResponse.data.name}");

                if (UserSession.Instance != null)
                {
                    UserSession.Instance.UserId = loginResponse.data.regionID;
                }
                StartCoroutine(LoadSceneAfterDelay(3f));
                //SceneManager.LoadScene(1); // đổi scene
            }
            else
            {
                messageManager.ShowError(loginResponse.notification);
                Debug.LogWarning("Đăng nhập thất bại: " + loginResponse.notification);
            }
        }
    }
 */