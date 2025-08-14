using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Tự hút item trong bán kính về phía Player, sau đó gọi lại pipeline nhặt đồ sẵn có trong ItemsPicker.
/// Gắn script này vào Player (hoặc object nào bạn muốn làm “nam châm”).
/// </summary>
[DisallowMultipleComponent]
public class AutoPickupMagnet : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Tham chiếu tới ItemsPicker trên Player để gọi lại pipeline nhặt đồ (UI icon bay, event, despawn...)")]
    [SerializeField] private ItemsPicker itemsPicker;

    [Tooltip("Controller để áp dụng rule HP đầy thì không hút (tùy chọn).")]
    [SerializeField] private CharacterController2D controller;

    [Header("Scan Settings")]
    [SerializeField] private LayerMask itemLayerMask = -1;
    [SerializeField, Min(0.1f)] private float radius = 3.0f;
    [SerializeField, Range(1, 32)] private int maxStartsPerFrame = 8;
    [SerializeField] private bool drawGizmo = true;

    [Header("Flight Settings")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField, Min(0.05f)] private float minDuration = 0.15f;
    [SerializeField, Min(0.05f)] private float maxDuration = 0.35f;
    [SerializeField] private Ease ease = Ease.OutQuad;
    [SerializeField] private Vector3 catchOffset = new Vector3(0f, 0.6f, 0f);

    [Header("Rules (Optional)")]
    [Tooltip("Nếu bật, item HP sẽ không bị hút khi máu đã đầy.")]
    [SerializeField] private bool skipHPPickWhenFull = true;

    // Internal
    private readonly Collider2D[] _hits = new Collider2D[32];
    private readonly HashSet<ItemsDropCtrl> _attracting = new();

    private void Reset()
    {
        if (itemsPicker == null) itemsPicker = GetComponentInParent<ItemsPicker>();
        if (controller == null) controller = GetComponentInParent<CharacterController2D>();
    }

    private void Awake()
    {
        if (itemsPicker == null)
            itemsPicker = GetComponentInParent<ItemsPicker>();
    }

    private void Update()
    {
        AutoScanAndMagnet();
    }

    private void AutoScanAndMagnet()
    {
        // Chuẩn bị filter
        var filter = new ContactFilter2D();
        filter.SetLayerMask(itemLayerMask);
        filter.useTriggers = true;

        // Quét vào mảng _hits
        int count = Physics2D.OverlapCircle(transform.position, radius, filter, _hits);

        int started = 0;
        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (col == null) continue;
            if (col.transform.parent == null) continue;

            var item = col.transform.parent.GetComponent<ItemsDropCtrl>();
            if (item == null) continue;
            if (!item.isActiveAndEnabled) continue;
            if (_attracting.Contains(item)) continue;

            if (skipHPPickWhenFull && controller != null &&
                item.ItemCode == ItemCode.HP &&
                controller.life >= controller.maxLife)
                continue;

            StartMagnet(item);
            started++;
            if (started >= maxStartsPerFrame) break;
        }
    }


    private void StartMagnet(ItemsDropCtrl item)
    {
        _attracting.Add(item);

        // Tắt vật lý để bay mượt và không va chạm lung tung
        var rb = item.Rigidbody;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Thời gian bay tỷ lệ theo khoảng cách
        float dist = Vector3.Distance(item.transform.position, GetCatchPoint());
        float t = Mathf.Lerp(minDuration, maxDuration, Mathf.InverseLerp(0f, radius, dist));

        // Tween về player
        item.transform
            .DOMove(GetCatchPoint(), t)
            .SetEase(ease)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                if (item == null) { _attracting.Remove(null); return; }

                // Gọi pipeline nhặt đồ của ItemsPicker (cần wrapper public đã thêm ở Bước 1)
                if (itemsPicker != null)
                {
                    itemsPicker.PickupFromMagnet(item);
                }
                else
                {
                    // Fallback an toàn: nếu thiếu ItemsPicker, tự xử nhanh (ít ưu tiên)
                    TryFallbackPickup(item);
                }

                _attracting.Remove(item);
            })
            .OnKill(() => _attracting.Remove(item));
    }

    private Vector3 GetCatchPoint()
    {
        return transform.position + catchOffset;
    }

    private void TryFallbackPickup(ItemsDropCtrl item)
    {
        // Giữ đúng rule HP đầy nếu có controller
        if (skipHPPickWhenFull && controller != null &&
            item.ItemCode == ItemCode.HP &&
            controller.life >= controller.maxLife)
        {
            // Không nhặt
            ReenablePhysics(item);
            return;
        }

        // Gọi hiệu ứng UI icon bay (chặng 2) như hệ thống cũ
        ItemPickupFlyUI.Instance?.PlayFromTransform(item.ItemCode, item.transform);

        // Các event/gameplay bạn đang làm trong ItemsPicker.PickupItem(...)
        ItemCollectionTracker.Instance?.OnItemCollected(item.ItemCode);
        HauSoundManager.Instance?.SpawnSound(Vector3.zero, SoundName.PickUpItem);

        // Thêm vào inventory nằm trong DoDespawn()
        item.Despawn.DoDespawn();
    }

    private void ReenablePhysics(ItemsDropCtrl item)
    {
        if (item != null && item.Rigidbody != null)
            item.Rigidbody.simulated = true;
    }

    private void OnDisable()
    {
        // Nếu object bị disable, dừng mọi tween & trả vật lý cho item đang hút (tránh kẹt)
        foreach (var it in _attracting)
        {
            if (it == null) continue;
            it.transform.DOKill();
            ReenablePhysics(it);
        }
        _attracting.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position + catchOffset, 0.15f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
