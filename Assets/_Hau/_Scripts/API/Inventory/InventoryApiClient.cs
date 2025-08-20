using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class InventoryItemDto
{
    public int itemCode;
    public int count;
}

[Serializable]
public class PickItemRequest
{
    public int userId;
    public int itemCode;
    public int delta;
    public string reason;
}

[Serializable]
public class InventorySnapshotItem
{
    public int itemCode;
    public int count;
}

[Serializable]
public class SyncInventoryRequest
{
    public int userId;
    public List<InventorySnapshotItem> items = new();
}

// JsonUtility cần wrapper để serialize mảng
[Serializable]
public class InventoryItemDtoArrayWrapper
{
    public InventoryItemDto[] items;
}

public static class InventoryApiClient
{
    // Đổi đúng IP/port máy chạy API
    private static readonly string BaseUrl = "http://localhost:5235";

    public static async Task<bool> PickItemAsync(int userId, int itemCode, int delta, string reason = "pickup")
    {
        var req = new PickItemRequest { userId = userId, itemCode = itemCode, delta = delta, reason = reason };
        var json = JsonUtility.ToJson(req);
        Debug.Log($"[PickItemAsync] JSON => {json}");

        using var www = new UnityWebRequest($"{BaseUrl}/api/Inventory/pick", "POST"); // giữ đúng /Inventory
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        var op = www.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"PickItemAsync error: {www.responseCode} | {www.error} | {www.downloadHandler.text}");
            return false;
        }

        Debug.Log($"PickItemAsync ok: {www.downloadHandler.text}");
        return true;
    }

    public static async Task<bool> SyncInventoryAsync(int userId, List<InventorySnapshotItem> items)
    {
        var req = new SyncInventoryRequest { userId = userId, items = items };
        var json = JsonUtility.ToJson(req);
        Debug.Log($"[SyncInventoryAsync] JSON => {json}");

        using var www = new UnityWebRequest($"{BaseUrl}/api/Inventory/sync", "POST"); // giữ đúng /Inventory
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        var op = www.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"SyncInventoryAsync error: {www.responseCode} | {www.error} | {www.downloadHandler.text}");
            return false;
        }

        Debug.Log($"SyncInventoryAsync ok: {www.downloadHandler.text}");
        return true;
    }
}
