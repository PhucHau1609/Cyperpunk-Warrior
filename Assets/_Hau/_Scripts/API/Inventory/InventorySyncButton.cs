using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySyncButton : MonoBehaviour
{
    // Test với user id trong DB (ví dụ 1 nếu bạn đã insert tay)
    public int userId = 1;

    public async void OnClickSyncSnapshot()
    {
        try
        {
            var invCtrl = InventoryManager.Instance?.ItemInventory();
            if (invCtrl == null)
            {
                Debug.LogWarning("InventoryCtrl null");
                return;
            }

            var items = new List<InventorySnapshotItem>();
            foreach (var it in invCtrl.ItemInventories)
            {
                items.Add(new InventorySnapshotItem
                {
                    itemCode = (int)it.ItemProfileSO.itemCode,
                    count = it.itemCount
                });
            }

            bool ok = await InventoryApiClient.SyncInventoryAsync(userId, items);
            Debug.Log("Sync snapshot: " + ok);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public async void OnClickPickGold()
    {
        try
        {
            bool ok = await InventoryApiClient.PickItemAsync(userId, (int)ItemCode.Gold, 10, "pickup_button");
            if (ok)
            {
                // Cập nhật local cho khớp
                InventoryManager.Instance.AddItem(ItemCode.Gold, 10);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
