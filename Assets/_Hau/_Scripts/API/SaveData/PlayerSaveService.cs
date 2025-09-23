using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using static PlayerSaveService;

public static class PlayerSaveService
{
    // Nhớ thay baseUrl theo API của bạn
    private static string baseUrl = "http://localhost:5235";

    [System.Serializable]
    public class SaveGameDTO
    {
        public int userId;
        public float posX, posY, posZ;
        public int lastCheckpointID;
        public string lastCheckpointScene;
        public float health;
        public float maxHealth;

        public List<EquippedItemDTO> equippedItems;

        // ĐỔI: dùng int thay vì SkillID cho chắc chắn
        public List<int> unlockedSkills;

        public bool petUnlocked;  // NEW: robot đồng hành đã mở



    }

    [System.Serializable]
    public class EquippedItemDTO
    {
        public int slotType;   // Enum EquipmentType (Helmet, Armor, Weapon...)
        public int itemCode;   // Enum ItemCode
    }

    [System.Serializable]
    public class SaveGameResponse
    {
        public int userId;
        public float posX, posY, posZ;
        public int lastCheckpointID;
        public string lastCheckpointScene;
        public float health;
        public float maxHealth;
        public string updatedAt;

        // NEW
        public List<int> unlockedSkills;

        public bool petUnlocked;   // NEW: tiện kiểm tra ngay sau save

    }

    public static async Task<bool> SaveGameAsync(SaveGameDTO dto)
    {
        string url = $"{baseUrl}/api/savegame";
        string json = JsonConvert.SerializeObject(dto);
        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            // TODO: nếu dùng JWT: req.SetRequestHeader("Authorization", "Bearer " + token);

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SaveGame] Error: {req.responseCode} - {req.error} - {req.downloadHandler.text}");
                return false;
            }
            return true;
        }
    }

    public static async Task<SaveGameResponse> LoadGameAsync(int userId)
    {
        string url = $"{baseUrl}/api/savegame/{userId}";
        using (var req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Content-Type", "application/json");
            // TODO: nếu dùng JWT: req.SetRequestHeader("Authorization", "Bearer " + token);

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[LoadGame] {req.responseCode} - {req.error} - {req.downloadHandler.text}");
                return null;
            }

            var res = JsonConvert.DeserializeObject<SaveGameResponse>(req.downloadHandler.text);
            return res;
        }
    }

    public static class EquipmentSaveService
    {
        public static async Task<bool> SaveEquipmentAsync(int userId)
        {
            var equipped = new List<EquippedItemDTO>();
            foreach (var slot in Object.FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None))
            {
                if (slot.HasItem())
                {
                    equipped.Add(new EquippedItemDTO
                    {
                        slotType = (int)slot.slotType,
                        itemCode = (int)slot.currentItem.ItemProfileSO.itemCode
                    });
                }
            }



            var dto = new { userId, equippedItems = equipped };
            string url = $"{baseUrl}/api/savegame/equipment";
            string json = JsonConvert.SerializeObject(dto);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            return req.result == UnityWebRequest.Result.Success;
        }
    }
}

