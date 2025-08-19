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
        public int userId;         // NEW: nhận từ API
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

    [Serializable]
    public class LoadGameResponse
    {
        public bool isSuccess;
        public string message;          // nếu có
        public float health;
        public float maxHealth;
        public float posX, posY, posZ;
        public int? lastCheckpointID;
        public string lastCheckpointScene;
        public string updatedAt;
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
        //string url = "https://apiv3-sunny.up.railway.app/api/Login/login";
        string url = "http://localhost:5235/api/Login/login";
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
                    UserSession.Instance.UserId = loginResponse.data.userId;

                }
                // NEW: load save rồi mới vào game
                StartCoroutine(LoadSaveAndEnterGame());
                StartCoroutine(LoadSceneAfterDelay(.5f));
            }
            else
            {
                messageManager?.ShowError(loginResponse?.notification ?? "Đăng nhập thất bại!");
            }
        }
    }

    // NEW: tải save, quyết định scene, rồi apply sau khi load scene
    IEnumerator LoadSaveAndEnterGame()
    {
        // gọi API load save
        bool done = false;
        LoadGameResponse cache = null;

        yield return SaveApi.LoadGame(UserSession.Instance.UserId, (resp) =>
        {
            cache = resp;
            done = true;
        });
        if (!done) yield break;

        // nếu có save → cache vào UserSession để apply sau khi vào scene
        string sceneToLoad = null; // null => dùng scene mặc định (build index 1)

        if (cache != null && cache.isSuccess)
        {
            UserSession.Instance.HasLoadedSave = true;
            UserSession.Instance.SavedPosition = new Vector3(cache.posX, cache.posY, cache.posZ);
            UserSession.Instance.SavedMaxHealth = cache.maxHealth;
            UserSession.Instance.SavedHealth = cache.health;
            UserSession.Instance.SavedSceneName = string.IsNullOrWhiteSpace(cache.lastCheckpointScene)
                ? null
                : cache.lastCheckpointScene;

            sceneToLoad = UserSession.Instance.SavedSceneName;
        }
        else
        {
            UserSession.Instance.HasLoadedSave = false; // lần đầu chơi
        }

        // chuyển scene: ưu tiên scene đã lưu; nếu không có → dùng scene mặc định index 1
        AsyncOperation op = null;
        if (!string.IsNullOrEmpty(sceneToLoad))
            op = SceneManager.LoadSceneAsync(sceneToLoad);
        else
            op = SceneManager.LoadSceneAsync(1);

        while (!op.isDone) yield return null;

        // Sau khi scene load xong, apply state nếu có
        ApplySavedStateIfAny();
    }

    // NEW: áp vị trí & máu vào Player sau khi scene đã load
    private void ApplySavedStateIfAny()
    {
        if (UserSession.Instance == null || !UserSession.Instance.HasLoadedSave) return;

        var player = FindFirstObjectByType<CharacterController2D>();
        if (player != null)
        {
            // đặt vị trí
            player.transform.position = UserSession.Instance.SavedPosition;

            // đặt máu
            player.maxLife = UserSession.Instance.SavedMaxHealth;
            player.life = Mathf.Clamp(UserSession.Instance.SavedHealth, 0, player.maxLife);

            // đồng bộ vài thứ UI/Camera nếu cần
            player.SyncFacingDirection();
            CameraFollow.Instance?.TryFindPlayer();
        }
    }

    IEnumerator LoadSceneAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(1);
    }

    public static class SaveApi
    {
        public static string BaseUrl = "https://apiv3-sunny.up.railway.app"; // đổi nếu khác

        public static IEnumerator LoadGame(int userId, Action<LoadGameResponse> onDone)
        {
            var url = $"{BaseUrl}/api/Save/LoadGame/{userId}";
            using var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError ||
                req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("[LoadGame] HTTP Error: " + req.error);
                onDone?.Invoke(null);
                yield break;
            }

            try
            {
                var resp = JsonConvert.DeserializeObject<LoadGameResponse>(req.downloadHandler.text);
                onDone?.Invoke(resp);
            }
            catch (Exception e)
            {
                Debug.LogError("[LoadGame] Parse error: " + e);
                onDone?.Invoke(null);
            }
        }
    }

}



