using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AutosaveInventory : MonoBehaviour
{
    public static AutosaveInventory Instance { get; private set; }

    [Header("Timings")]
    [SerializeField] float flushIntervalSec = 2f;     // đẩy delta mỗi 2s nếu có thay đổi
    [SerializeField] float snapshotIntervalSec = 60f; // chốt snapshot mỗi 60s
    [SerializeField] bool logDebug = true;

    private readonly Dictionary<ItemCode, int> pending = new();
    private float tFlush, tSnapshot;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary> Gom delta (+ nhặt, − dùng). </summary>
    public void EnqueueDelta(ItemCode code, int delta)
    {
        if (delta == 0) return;
        pending.TryGetValue(code, out var cur);
        cur += delta;
        if (cur == 0) pending.Remove(code); else pending[code] = cur;
        if (logDebug) Debug.Log($"[Autosave] queued {code} {delta} (agg={cur})");
    }

    void Update()
    {
        tFlush += Time.unscaledDeltaTime;
        tSnapshot += Time.unscaledDeltaTime;

        if (tFlush >= flushIntervalSec && pending.Count > 0)
        {
            tFlush = 0f;
            _ = FlushDeltasAsync(); // fire-and-forget
        }

        if (tSnapshot >= snapshotIntervalSec)
        {
            tSnapshot = 0f;
            _ = SyncFullSnapshotAsync(); // fire-and-forget
        }
    }

    int CurrentUserId()
    {
        return UserSession.Instance != null ? UserSession.Instance.UserId : 0;
    }

    public async Task FlushDeltasAsync()
    {
        if (pending.Count == 0) return;
        var uid = CurrentUserId();
        if (uid <= 0) { if (logDebug) Debug.LogWarning("[Autosave] No user id; skip flush"); return; }

        // copy để không bị sửa trong lúc await
        var copy = new List<KeyValuePair<ItemCode, int>>(pending);
        pending.Clear();

        foreach (var kv in copy)
        {
            if (kv.Value == 0) continue;
            var ok = await InventoryApiClient.PickItemAsync(uid, (int)kv.Key, kv.Value, "auto_delta");
            if (!ok)
            {
                // nếu gửi fail, đưa lại vào hàng chờ
                pending.TryGetValue(kv.Key, out var cur);
                pending[kv.Key] = cur + kv.Value;
            }
        }
    }

    public async Task SyncFullSnapshotAsync()
    {
        var uid = CurrentUserId();
        if (uid <= 0) return;

        var invCtrl = InventoryManager.Instance?.ItemInventory();
        if (invCtrl == null) return;

        var list = new List<InventorySnapshotItem>();
        foreach (var it in invCtrl.ItemInventories)
            list.Add(new InventorySnapshotItem { itemCode = (int)it.ItemProfileSO.itemCode, count = it.itemCount });

        await InventoryApiClient.SyncInventoryAsync(uid, list);
    }

    // Khi mất focus/pause → cố gắng flush + snapshot
    async void OnApplicationPause(bool pause)
    {
        if (pause) { await FlushDeltasAsync(); await SyncFullSnapshotAsync(); }
    }
    async void OnApplicationFocus(bool focus)
    {
        if (!focus) { await FlushDeltasAsync(); await SyncFullSnapshotAsync(); }
    }
    async void OnDisable()
    {
        await FlushDeltasAsync();
    }
}
