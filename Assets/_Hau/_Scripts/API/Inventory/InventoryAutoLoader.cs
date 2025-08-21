// InventoryAutoLoader.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InventoryAutoLoader : MonoBehaviour
{
    [Tooltip("Xóa toàn bộ inventory local trước khi apply server")]
    public bool clearLocalFirst = true;

    private async void Start()
    {
        // chờ UserSession sẵn sàng
        for (int i = 0; i < 120; i++) // ~2s
        {
            if (UserSession.Instance != null && UserSession.Instance.UserId > 0) break;
            await Task.Yield();
        }

        if (UserSession.Instance == null || UserSession.Instance.UserId <= 0)
        {
            Debug.LogWarning("[InventoryAutoLoader] UserSession chưa sẵn sàng, bỏ qua load.");
            return;
        }

        int userId = UserSession.Instance.UserId;
        var serverItems = await InventoryApiClient.GetInventoryAsync(userId);
        if (serverItems == null)
        {
            Debug.LogWarning("[InventoryAutoLoader] Không tải được inventory từ server.");
            return;
        }

        ApplyToLocal(serverItems, clearLocalFirst);
        Debug.Log($"[InventoryAutoLoader] Loaded {serverItems.Count} items for user {userId}");
    }

    private void ApplyToLocal(List<InventoryItemDto> items, bool clear)
    {
        var invCtrl = InventoryManager.Instance?.ItemInventory();
        if (invCtrl == null)
        {
            Debug.LogWarning("[InventoryAutoLoader] InventoryCtrl null");
            return;
        }

        if (clear)
        {
            // Clear sạch local để không dính item của account khác
            invCtrl.ItemInventories.Clear();
        }

        foreach (var it in items)
        {
            // Bảo vệ dữ liệu
            if (it.count <= 0) continue;

            // Enum bên Unity và server phải trùng số
            InventoryManager.Instance.AddItem((ItemCode)it.itemCode, it.count);
        }

        // (tuỳ chọn) bắn event cập nhật UI nếu bạn có
        ObserverManager.Instance?.PostEvent(EventID.InventoryChanged, null);
    }
}
