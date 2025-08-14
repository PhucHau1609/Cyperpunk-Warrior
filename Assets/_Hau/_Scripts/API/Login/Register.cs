using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Register : MonoBehaviour
{
    private Dictionary<TMP_InputField, string> inputHistory = new Dictionary<TMP_InputField, string>();
    private TMP_InputField currentInputField;

    [Header("InputField")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;

    [Header("Message")]
    public UIMessageManager messageManager;


    void Start()
    {
        TMP_InputField[] inputs = { emailInput, passwordInput, nameInput };
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
    }

    void OnDisable()
    {
        currentInputField = null;
    }


    public void GoToLogin()
    {
        // chuyển scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
    
    public void OnRegisterClick()
    {
        AudioManager.Instance?.PlayClickSFX();

        var email = emailInput.text;
        var password = passwordInput.text;
        var name = nameInput.text;

        var dto = new RegisterDTO
        {
            username = name,
            email = email,
            password = password
        };
        var json = JsonUtility.ToJson(dto);


        /*var account = new Account
        { Email = email, Password = password, Name = name };

        var json = JsonUtility.ToJson(account);*/

        StartCoroutine(Post(json));
    }

    IEnumerator Post(string json)
    {
        //var url = "https://apiv3-sunny.up.railway.app/api/Register/register"; http://localhost:5235/api/Register/register
        var url = "http://localhost:5235/api/Register/register";

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<RegisterModelResponse>(responseText);
                messageManager.ShowError(errorResponse.notification ?? "Lỗi kết nối đến máy chủ!");
            }
            catch
            {
                messageManager.ShowError("Lỗi mạng hoặc máy chủ không phản hồi!");
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
                    {
                        messageManager.ShowSuccess("Đăng ký thành công!");
                    }
                    else
                    {
                        messageManager.ShowError(response.notification ?? "Đăng ký thất bại, vui lòng thử lại!");
                    }
                }
                else
                {
                    messageManager.ShowError("Phản hồi không hợp lệ từ server.");
                }
            }
            catch (JsonReaderException)
            {
                messageManager.ShowError("Lỗi khi phân tích phản hồi từ server.");
            }
        }
    }

}


/*
  IEnumerator Post(string json)
    {
        var url = "https://apiv3-sunny.up.railway.app/api/Register/register";

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;
        Debug.Log("Response Text: " + responseText);

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Protocol Error: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response: {responseText}");
        }
        else
        {
            try
            {
                var response = JsonConvert.DeserializeObject<RegisterModelResponse>(responseText);

                if (response != null)
                {
                    if (response.isSuccess)
                    {
                        messageManager.ShowSuccess("Đăng ký thành công!");
                        Debug.Log("Đăng ký thành công:");
                        Debug.Log("Email: " + response.data.Email);
                        Debug.Log("Tên: " + response.data.Name);
                    }
                    else
                    {
                        messageManager.ShowError(response.notification);
                        Debug.LogWarning("Đăng ký thất bại: " + response.notification);
                    }
                }
                else
                {
                    Debug.LogWarning("Không thể phân tích phản hồi từ server.");
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError("Lỗi khi phân tích JSON: " + ex.Message);
                Debug.LogError("Server có thể không trả về JSON hợp lệ.");
            }
        }
    }
 */


