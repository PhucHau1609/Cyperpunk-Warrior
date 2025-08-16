using UnityEngine;
using System.Collections.Generic;

public class DurationHUDManager : MonoBehaviour
{
    [Header("Layout")]
    public RectTransform container;        // Panel HUD (UI góc màn hình)
    public GameObject badgePrefab;         // Prefab DurationBadge
    public Vector2 firstPosition = new Vector2(40, -40); // toạ độ bắt đầu (anchored)
    public float spacingX = 90f;           // khoảng cách xếp sang phải

    // Optional: lấy icon từ SkillManagerUI
    public SkillManagerUI skillManagerUI;

    // runtime
    private readonly Dictionary<SkillID, DurationBadge> active = new();

    private void OnEnable()
    {
        PlayerShader.OnEffectStarted += HandleEffectStarted;
        PlayerShader.OnEffectEnded += HandleEffectEnded;
    }

    private void OnDisable()
    {
        PlayerShader.OnEffectStarted -= HandleEffectStarted;
        PlayerShader.OnEffectEnded -= HandleEffectEnded;
    }

    private void HandleEffectStarted(SkillID id, float durationSec)
    {
        // Nếu đã có badge cũ => dừng cũ trước (tránh trùng)
        if (active.TryGetValue(id, out var oldBadge))
        {
            oldBadge.ForceStop();
            Destroy(oldBadge.gameObject);
            active.Remove(id);
        }

        // Spawn badge mới
        var go = Instantiate(badgePrefab, container);
        var badge = go.GetComponent<DurationBadge>();

        // Lấy icon từ SkillManagerUI
        var icon = GetIconFor(id);
        badge.Setup(icon);

        active[id] = badge;

        // Bắt đầu timer, khi xong tự remove + reposition
        badge.StartTimer(durationSec, () =>
        {
            if (active.ContainsKey(id))
            {
                var b = active[id];
                active.Remove(id);
                Destroy(b.gameObject);
                Reposition();
            }
        });

        Reposition();
    }

    private void HandleEffectEnded(SkillID id)
    {
        if (active.TryGetValue(id, out var badge))
        {
            badge.ForceStop();
            active.Remove(id);
            Destroy(badge.gameObject);
            Reposition();
        }
    }

    private void Reposition()
    {
        // Xếp badge theo thứ tự thêm (active.Values)
        int i = 0;
        foreach (var kv in active)
        {
            var rt = kv.Value.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(firstPosition.x + i * spacingX, firstPosition.y);
            i++;
        }
    }

    private Sprite GetIconFor(SkillID id)
    {
        if (skillManagerUI == null) return null;
        // skillManagerUI.skills: List<SkillData>
        var data = skillManagerUI.skills.Find(s => s.skillID == id);
        return data != null ? data.icon : null;
    }
}
