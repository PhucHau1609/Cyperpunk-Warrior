using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public static class PlayerSaveService
{
    // KHÔNG để dấu '/' ở cuối để tránh "//" khi ghép URL
    private const string BaseUrl = "https://apiv3-sunny.up.railway.app";

    // --- DTO khớp API hiện tại ---
    [Serializable]
    public class SaveGameDTO
    {
        public int userId;
        public float health;
        public float maxHealth;
        public float posX;
        public float posY;
        public float posZ;
        public int? lastCheckpointID;
        public string lastCheckpointScene;
    }

    [Serializable]
    public class LoadGameResponse
    {
        public bool isSuccess;
        public string message;
        public float health;
        public float maxHealth;
        public float posX, posY, posZ;
        public int? lastCheckpointID;
        public string lastCheckpointScene;
        public string updatedAt;
    }

    // --- SAVE: POST /api/Save/SaveGame ---
    public static async Task<bool> SaveGameAsync(SaveGameDTO dto)
    {
        var url = $"{BaseUrl}/api/Save/SaveGame";
        var json = JsonConvert.SerializeObject(dto);
        var body = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[SaveGame] HTTP Error: {req.responseCode} {req.error} - {req.downloadHandler.text}");
            return false;
        }
        return true;
    }

    // --- LOAD: GET /api/Save/LoadGame/{userId} ---
    public static async Task<LoadGameResponse> LoadGameAsync(int userId)
    {
        var url = $"{BaseUrl}/api/Save/LoadGame/{userId}";
        using var req = UnityWebRequest.Get(url);

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LoadGame] HTTP Error: {req.responseCode} {req.error} - {req.downloadHandler.text}");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<LoadGameResponse>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogError("[LoadGame] Parse error: " + e);
            return null;
        }
    }
}
