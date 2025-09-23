/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.cyborgAssets.inspectorButtonPro;

public class InventoryManager : HauSingleton<InventoryManager>
{
    [SerializeField] protected List<InventoryCtrl> inventoryCtrlList;

    [SerializeField] protected List<ItemProfileSO> itemProfileSO;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadInventoryCtrl();
        this.LoadItemProfiles(); //E71 create
    }

    public virtual void AddItem(ItemInventory itemInventory)
    {
        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);
        inventoryCtrl.AddItem(itemInventory);

        AutosaveInventory.Instance?.EnqueueDelta(itemInventory.ItemProfileSO.itemCode, itemInventory.itemCount);

    }

    public virtual void AddItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        ItemInventory item = new(itemProfile, itemCount);
        this.AddItem(item);
    }

    public virtual void RemoveItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        ItemInventory item = new(itemProfile, itemCount);
        this.RemoveItem(item);
    }

    public virtual void RemoveItem(ItemInventory itemInventory)
    {
        //Debug.Log($"[InventoryManager] RemoveItem called - Item: {itemInventory.ItemProfileSO.itemCode}, Count: {itemInventory.itemCount}");

        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);

        if (inventoryCtrl == null)
        {
            //Debug.LogError($"[InventoryManager] Inventory not found: {invCodeName}");
            return;
        }

        //Debug.Log($"[InventoryManager] Found inventory: {invCodeName}, Items count before: {inventoryCtrl.ItemInventories.Count}");

        bool removed = inventoryCtrl.RemoveItem(itemInventory);

        //Debug.Log($"[InventoryManager] Remove result: {removed}, Items count after: {inventoryCtrl.ItemInventories.Count}");

        if (removed)
        {
            ObserverManager.Instance?.PostEvent(EventID.InventoryChanged, null);
            AutosaveInventory.Instance?.EnqueueDelta(itemInventory.ItemProfileSO.itemCode, -itemInventory.itemCount);

        }
    }

    protected virtual void LoadInventoryCtrl()
    {
        if (this.inventoryCtrlList.Count > 0) return;

        foreach(Transform child in transform)
        {
            InventoryCtrl inventoryCtrl = child.GetComponent<InventoryCtrl>();
            if(inventoryCtrl == null) continue;
            this.inventoryCtrlList.Add(inventoryCtrl);
        }

        Debug.LogWarning(transform.name + ": LoadInventoryCtrl", gameObject);
    }

    protected virtual void LoadItemProfiles() ////E71 create
    {
        if (this.itemProfileSO.Count > 0) return;
        ItemProfileSO[] itemProfileSOs = Resources.LoadAll<ItemProfileSO>("/");
        this.itemProfileSO = new List<ItemProfileSO>(itemProfileSOs);
        Debug.Log(transform.name + ": LoadItemProfiles", gameObject);
    }

    public virtual InventoryCtrl GetInventoryByName(InventoryCodeName inventoryCodeName)
    {
        foreach(InventoryCtrl inventoryCtrl in this.inventoryCtrlList)
        {
            if(inventoryCtrl.GetName()  == inventoryCodeName) return inventoryCtrl;
        }

        return null;
    }

    public virtual ItemProfileSO GetProfileByCode(ItemCode itemCode)
    {
        foreach (ItemProfileSO itemProfile in this.itemProfileSO)
        {
            if (itemProfile.itemCode == itemCode) return itemProfile;
        }

        return null;
    }


    public virtual InventoryCtrl MoneyInventory() //E67 create
    {
        return this.GetInventoryByName(InventoryCodeName.Currency);
    }

    public virtual InventoryCtrl ItemInventory() //E67 create
    {
        return this.GetInventoryByName(InventoryCodeName.Items);
    }

    // Helper kiểm tra trong inventory có item và count > 0 hay không
    public bool HasItemInInventory(ItemCode itemCode)
    {
        InventoryCtrl itemInv = ItemInventory();
        if (itemInv == null) return false;
        ItemInventory found = itemInv.FindItem(itemCode);
        if (found == null) return false;
        return found.itemCount > 0;
    }


}

*/


using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-200)] // Đảm bảo Awake/LoadComponents chạy sớm hơn các loader khác
public class InventoryManager : HauSingleton<InventoryManager>
{
    [Header("Inventories")]
    [SerializeField] protected List<InventoryCtrl> inventoryCtrlList = new();

    [Header("Item Profiles (ScriptableObjects in Resources)")]
    [SerializeField] protected List<ItemProfileSO> itemProfileSO = new();

    // Map tra cứu nhanh: ItemCode -> ItemProfileSO
    private Dictionary<ItemCode, ItemProfileSO> profileMap = new();

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadInventoryCtrl();
        this.LoadItemProfiles();
    }

    #region Public APIs

    /// <summary>
    /// Thêm item theo đối tượng ItemInventory có sẵn
    /// </summary>
    public virtual void AddItem(ItemInventory itemInventory)
    {
        if (itemInventory == null || itemInventory.ItemProfileSO == null)
        {
            Debug.LogError("[InventoryManager] AddItem bị bỏ qua vì itemInventory hoặc profile null.");
            return;
        }

        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);

        if (inventoryCtrl == null)
        {
            Debug.LogError($"[InventoryManager] InventoryCtrl không tồn tại: {invCodeName}");
            return;
        }

        inventoryCtrl.AddItem(itemInventory);

        // Đẩy delta lên autosave (nếu có)
        AutosaveInventory.Instance?.EnqueueDelta(itemInventory.ItemProfileSO.itemCode, itemInventory.itemCount);
    }

    /// <summary>
    /// Thêm item theo mã ItemCode và số lượng
    /// </summary>
    public virtual void AddItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        if (itemProfile == null)
        {
            Debug.LogError($"[InventoryManager] AddItem bị bỏ qua vì không tìm thấy profile cho {itemCode}");
            return;
        }

        ItemInventory item = new(itemProfile, itemCount);
        if (item.ItemProfileSO == null)
        {
            Debug.LogError($"[InventoryManager] AddItem khởi tạo ItemInventory thất bại (profile null?) cho {itemCode}");
            return;
        }

        this.AddItem(item);
    }

    /// <summary>
    /// Xóa item theo mã ItemCode và số lượng
    /// </summary>
    public virtual void RemoveItem(ItemCode itemCode, int itemCount)
    {
        ItemProfileSO itemProfile = InventoryManager.Instance.GetProfileByCode(itemCode);
        if (itemProfile == null)
        {
            Debug.LogError($"[InventoryManager] RemoveItem bị bỏ qua vì không tìm thấy profile cho {itemCode}");
            return;
        }

        ItemInventory item = new(itemProfile, itemCount);
        if (item.ItemProfileSO == null)
        {
            Debug.LogError($"[InventoryManager] RemoveItem khởi tạo ItemInventory thất bại (profile null?) cho {itemCode}");
            return;
        }

        this.RemoveItem(item);
    }

    /// <summary>
    /// Xóa item theo đối tượng ItemInventory
    /// </summary>
    public virtual void RemoveItem(ItemInventory itemInventory)
    {
        if (itemInventory == null || itemInventory.ItemProfileSO == null)
        {
            Debug.LogError("[InventoryManager] RemoveItem bị bỏ qua vì itemInventory hoặc profile null.");
            return;
        }

        InventoryCodeName invCodeName = itemInventory.ItemProfileSO.invCodeName;
        InventoryCtrl inventoryCtrl = InventoryManager.Instance.GetInventoryByName(invCodeName);

        if (inventoryCtrl == null)
        {
            Debug.LogError($"[InventoryManager] InventoryCtrl không tồn tại: {invCodeName}");
            return;
        }

        bool removed = inventoryCtrl.RemoveItem(itemInventory);
        if (removed)
        {
            ObserverManager.Instance?.PostEvent(EventID.InventoryChanged, null);
            AutosaveInventory.Instance?.EnqueueDelta(itemInventory.ItemProfileSO.itemCode, -itemInventory.itemCount);
        }
    }

    /// <summary>
    /// Lấy InventoryCtrl theo tên (enum InventoryCodeName)
    /// </summary>
    public virtual InventoryCtrl GetInventoryByName(InventoryCodeName inventoryCodeName)
    {
        foreach (InventoryCtrl inventoryCtrl in this.inventoryCtrlList)
        {
            if (inventoryCtrl != null && inventoryCtrl.GetName() == inventoryCodeName)
                return inventoryCtrl;
        }
        return null;
    }

    /// <summary>
    /// Lấy profile theo ItemCode (ưu tiên tra map)
    /// </summary>
    public virtual ItemProfileSO GetProfileByCode(ItemCode itemCode)
    {
        if (profileMap != null && profileMap.TryGetValue(itemCode, out var p) && p != null)
            return p;

        // Fallback quét list (trong trường hợp hiếm)
        foreach (ItemProfileSO itemProfile in this.itemProfileSO)
        {
            if (itemProfile != null && itemProfile.itemCode == itemCode) return itemProfile;
        }

        Debug.LogError($"[InventoryManager] Không tìm thấy ItemProfileSO cho ItemCode={itemCode}. Hãy đảm bảo ScriptableObject nằm dưới thư mục 'Resources/'.");
        return null;
    }

    /// <summary>
    /// Lấy Inventory tiền tệ
    /// </summary>
    public virtual InventoryCtrl MoneyInventory() => this.GetInventoryByName(InventoryCodeName.Currency);

    /// <summary>
    /// Lấy Inventory vật phẩm
    /// </summary>
    public virtual InventoryCtrl ItemInventory() => this.GetInventoryByName(InventoryCodeName.Items);

    /// <summary>
    /// Kiểm tra tồn tại item trong inventory vật phẩm
    /// </summary>
    public bool HasItemInInventory(ItemCode itemCode)
    {
        InventoryCtrl itemInv = ItemInventory();
        if (itemInv == null) return false;
        ItemInventory found = itemInv.FindItem(itemCode);
        if (found == null) return false;
        return found.itemCount > 0;
    }

    /// <summary>
    /// Cho biết đã load xong profiles chưa (để các loader khác chờ)
    /// </summary>
    public bool HasProfiles => profileMap != null && profileMap.Count > 0;

    #endregion

    #region Load Helpers

    protected virtual void LoadInventoryCtrl()
    {
        if (this.inventoryCtrlList != null && this.inventoryCtrlList.Count > 0) return;

        this.inventoryCtrlList.Clear();

        foreach (Transform child in transform)
        {
            InventoryCtrl inventoryCtrl = child.GetComponent<InventoryCtrl>();
            if (inventoryCtrl == null) continue;
            this.inventoryCtrlList.Add(inventoryCtrl);
        }

        if (this.inventoryCtrlList.Count == 0)
            Debug.LogWarning($"{transform.name}: Không tìm thấy InventoryCtrl nào là con trực tiếp.");
        else
            Debug.Log($"{transform.name}: LoadInventoryCtrl -> {this.inventoryCtrlList.Count} inventories");
    }

    /// <summary>
    /// Load tất cả ItemProfileSO từ thư mục 'Resources/' (tại bất kỳ cấp con nào)
    /// </summary>
    protected virtual void LoadItemProfiles()
    {
        if (this.itemProfileSO != null && this.itemProfileSO.Count > 0)
        {
            BuildProfileMap();
            return;
        }

        // ❗ Dùng chuỗi rỗng "" để quét toàn bộ Resources (không dùng "/")
        ItemProfileSO[] itemProfileSOs = Resources.LoadAll<ItemProfileSO>("");

        this.itemProfileSO = itemProfileSOs != null
            ? new List<ItemProfileSO>(itemProfileSOs)
            : new List<ItemProfileSO>();

        if (this.itemProfileSO.Count == 0)
        {
            Debug.LogError($"{transform.name}: Không tìm thấy ItemProfileSO nào trong Resources. Đảm bảo các ScriptableObject nằm dưới bất kỳ thư mục có tên 'Resources/'.");
        }
        else
        {
            Debug.Log($"{transform.name}: Loaded {this.itemProfileSO.Count} ItemProfileSO");
        }

        BuildProfileMap();
    }

    private void BuildProfileMap()
    {
        profileMap.Clear();

        if (this.itemProfileSO == null) return;

        foreach (var p in this.itemProfileSO)
        {
            if (p == null) continue;
            var code = p.itemCode;
            if (!profileMap.ContainsKey(code))
            {
                profileMap[code] = p;
            }
            // Nếu có trùng code, ưu tiên profile đầu tiên load được
        }
    }

    #endregion
}
