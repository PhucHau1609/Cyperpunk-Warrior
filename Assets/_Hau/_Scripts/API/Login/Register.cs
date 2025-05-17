using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

public class Register : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;
    
    public void GoToLogin()
    {
        // chuyển scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
    
    public void OnRegisterClick()
    {
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

    // Gửi đối tượng JSON lên server
    IEnumerator Post(string json)
    {
        //var url = "http://localhost:5245/api/Register";
        var url = "https://apiv3-sunny.up.railway.app/api/Register/register";

        var request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler =(UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler =(DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Protocol Error: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            // Deserialize JSON phản hồi thành object C# sử dụng JsonUtility
            // var response = JsonUtility.FromJson<RegisterModelResponse>(request.downloadHandler.text);

            // Deserialize JSON phản hồi thành object C# sử dụng Newtonsoft.Json (Json.NET)
            var response = JsonConvert.DeserializeObject<RegisterModelResponse>(request.downloadHandler.text);

            if (response.isSuccess)
            {
                Debug.Log(response.data.Email);
                Debug.Log(response.data.Name);
            }
        }
    }
}
