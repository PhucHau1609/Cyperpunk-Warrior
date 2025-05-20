using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

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

    public void OnLoginClick()
    {
        var email = emailInput.text;
        var password = passwordInput.text;

        var loginRequest = new LoginRequest
        {
            email = email,
            password = password
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        StartCoroutine(PostLogin(json));
    }

    IEnumerator PostLogin(string json)
    {
        string url = "https://apiv3-sunny.up.railway.app/api/Login/login";

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Login failed: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            var responseText = request.downloadHandler.text;
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseText);

            if (loginResponse.isSuccess)
            {
                Debug.Log("Đăng nhập thành công!");
                Debug.Log($"Tên người dùng: {loginResponse.data.name}");

                if (UserSession.Instance != null)
                {
                    UserSession.Instance.UserId = loginResponse.data.regionID;
                }

                SceneManager.LoadScene(1); // đổi scene
            }
            else
            {
                Debug.LogWarning("Đăng nhập thất bại: " + loginResponse.notification);
            }
        }
    }
}
