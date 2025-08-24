using UnityEngine;
using System.Collections;

public class QuestEventOnLifecycle  : MonoBehaviour
{
    [Header("Quest Event")]
    [Tooltip("Tên sự kiện sẽ bắn khi object bị hủy/disable")]
    public string eventName = "lyra_puzzle_solved";

    [Tooltip("Số lượng tăng thêm (ví dụ: 1)")]
    public int amount = 1;

    [Header("When to fire")]
    [Tooltip("Bắn event khi OnDestroy được gọi")]
    public bool fireOnDestroy = true;

    [Tooltip("Bắn event khi OnDisable (hữu ích với object dùng object pooling)")]
    public bool fireOnDisable = false;

    [Tooltip("Độ trễ trước khi bắn event (giây)")]
    public float delaySeconds = 0f;

    [Tooltip("Chỉ bắn 1 lần, bỏ qua các lần gọi sau")]
    public bool onlyOnce = true;

    [Header("Safety")]
    [Tooltip("Bỏ qua bắn event khi scene đang unload (tránh spam lúc rời Play Mode)")]
    public bool ignoreWhenSceneUnloading = true;

    [Tooltip("In log khi bắn event để debug")]
    public bool debugLog = false;

    private bool _fired;

    /// <summary>
    /// Cho phép gọi thủ công từ code khác (ví dụ: trong Die()).
    /// </summary>
    public void FireNow()
    {
        TryFire("Manual");
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (!fireOnDisable) return;
        TryFire("OnDisable");
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (!fireOnDestroy) return;

        // Tránh bắn khi scene đang dỡ/unload (thường xảy ra khi thoát Play)
        if (ignoreWhenSceneUnloading && !gameObject.scene.isLoaded) return;

        TryFire("OnDestroy");
    }

    private void TryFire(string reason)
    {
        if (onlyOnce && _fired) return;
        if (string.IsNullOrEmpty(eventName)) return;

        _fired = true;
        if (delaySeconds > 0f)
        {
            StartCoroutine(DelayThenFire(reason));
        }
        else
        {
            Raise(reason);
        }
    }

    private IEnumerator DelayThenFire(string reason)
    {
        yield return new WaitForSeconds(delaySeconds);
        Raise(reason);
    }

    private void Raise(string reason)
    {
        QuestEventBus.Raise(eventName, Mathf.Max(1, amount));
        if (debugLog)
            Debug.Log($"[QuestEventOnLifecycle] Fired '{eventName}' (+{amount}) via {reason} on {name}");
    }
}
