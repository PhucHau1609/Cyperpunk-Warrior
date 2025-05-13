using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using com.cyborgAssets.inspectorButtonPro;

public class GameSaveManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:5245/api/SaveResult"; // đổi thành IP thật khi build

    public int userId = 1;

    void Start()
    {
        if (UserSession.Instance != null)
        {
            userId = UserSession.Instance.UserId;
            LoadGameData(); // Tự động tải dữ liệu khi scene bắt đầu
        }
    }


    [ProButton]
    public void SaveGameData()
    {
        Vector3 pos = transform.position; // ví dụ dùng vị trí của chính gameObject
        SaveData data = new SaveData
        {
            userId = userId,
            position = $"{pos.x},{pos.y},{pos.z}",
            health = 100
        };
        StartCoroutine(PostRequest($"{baseUrl}/SaveGame", data));
    }

    public void LoadGameData()
    {
        StartCoroutine(GetRequest($"{baseUrl}/LoadGame/{userId}"));
    }

    IEnumerator PostRequest(string uri, SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(uri, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError("Save failed: " + request.error);
            else
                Debug.Log("Save success: " + request.downloadHandler.text);
        }
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError("Load failed: " + request.error);
            else
            {
                Debug.Log("Load success: " + request.downloadHandler.text);
                var json = request.downloadHandler.text;

                // Parse thủ công (hoặc dùng JSON parser mạnh hơn nếu muốn)
                SaveDataResponse data = JsonUtility.FromJson<SaveDataResponse>(json);
                ApplyGameState(data);
            }
        }
    }

    void ApplyGameState(SaveDataResponse data)
    {
        if (data == null) return;
        string[] coords = data.position.Split(',');
        float x = float.Parse(coords[0]);
        float y = float.Parse(coords[1]);
        float z = float.Parse(coords[2]);

        transform.position = new Vector3(x, y, z);
        Debug.Log($"Loaded position: {x},{y},{z} - Health: {data.health}");
    }

    [System.Serializable]
    public class SaveDataResponse
    {
        public bool isSuccess;
        public string position;
        public int health;
        public string name;
    }
}
