using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuestTrigger2D : MonoBehaviour
{
    [Header("Event to raise when Player enters")]
    public string eventName = "near_door";
    public bool oneShot = true;

    [Tooltip("Chỉ kích hoạt nếu QuestManager đang ở đúng step này. -1 = bỏ qua kiểm tra.")]
    public int requireCurrentIndex = -1;

    [Header("Optional")]
    [SerializeField] private QuestManager_01 manager; // Kéo vào Inspector, hoặc để trống sẽ tự tìm

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (manager == null) manager = QuestManager_01.Instance ?? FindObjectOfType<QuestManager_01>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Ép đúng thứ tự nếu được yêu cầu
        if (manager != null && requireCurrentIndex >= 0 && manager.CurrentIndex != requireCurrentIndex)
        {
            // Nếu đã vượt qua bước này thì im lặng; nếu chưa tới bước thì log nhẹ để debug
            if (manager.CurrentIndex < requireCurrentIndex)
                Debug.Log($"[QuestTrigger2D] Bỏ qua '{eventName}' vì chưa tới step {requireCurrentIndex} (hiện tại = {manager.CurrentIndex}).");
            return;
        }

        // ✅ Trigger CHỈ bắn event, không tự ý ShowStep/đụng questData
        QuestEventBus.Raise(eventName, 1);

        if (oneShot) gameObject.SetActive(false);
    }
}
