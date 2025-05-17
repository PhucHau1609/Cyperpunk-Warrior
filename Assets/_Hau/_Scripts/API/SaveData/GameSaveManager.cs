using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using com.cyborgAssets.inspectorButtonPro;

public class GameSaveManager : MonoBehaviour
{
    private string baseUrl = "https://apiv3-sunny.up.railway.app/api/Save"; // cập nhật đúng endpoint

    public int userId = 1;

    void Start()
    {
        if (UserSession.Instance != null)
        {
            userId = UserSession.Instance.UserId;
            LoadGameData();
        }
    }

    [ProButton]
    public void SaveGameData()
    {
        Vector3 pos = transform.position;
        SaveData data = new SaveData
        {
            userId = userId,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            health = 100f
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
                SaveDataResponse data = JsonUtility.FromJson<SaveDataResponse>(request.downloadHandler.text);
                ApplyGameState(data);
            }
        }
    }

    void ApplyGameState(SaveDataResponse data)
    {
        if (data == null) return;

        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        Debug.Log($"Loaded position: {data.posX},{data.posY},{data.posZ} - Health: {data.health}");
    }
}
