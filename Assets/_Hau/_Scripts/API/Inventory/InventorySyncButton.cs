using System;
using System.Collections.Generic;
using UnityEngine;

// InventorySyncButton.cs — sửa để dùng UserSession
public class InventorySyncButton : HauSingleton<InventorySyncButton>
{
    private int CurrentUserId => (UserSession.Instance != null) ? UserSession.Instance.UserId : 0;

    public async void OnClickSyncSnapshot()
    {
        try
        {
            if (CurrentUserId <= 0) { Debug.LogWarning("UserId invalid"); return; }

            var invCtrl = InventoryManager.Instance?.ItemInventory();
            if (invCtrl == null) { Debug.LogWarning("InventoryCtrl null"); return; }

            var items = new List<InventorySnapshotItem>();
            foreach (var it in invCtrl.ItemInventories)
            {
                items.Add(new InventorySnapshotItem
                {
                    itemCode = (int)it.ItemProfileSO.itemCode,
                    count = it.itemCount
                });
            }

            bool ok = await InventoryApiClient.SyncInventoryAsync(CurrentUserId, items);
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
            if (CurrentUserId <= 0) { Debug.LogWarning("UserId invalid"); return; }

            bool ok = await InventoryApiClient.PickItemAsync(CurrentUserId, (int)ItemCode.Gold, 10, "pickup_button");
            if (ok)
            {
                InventoryManager.Instance.AddItem(ItemCode.Gold, 10);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}

