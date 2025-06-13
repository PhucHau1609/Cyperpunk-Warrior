using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // Nếu object có cha, tách ra khỏi parent để tránh bị phá huỷ cùng cha
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Tìm tất cả các instance cùng loại trong scene
        var existing = FindObjectsByType<PersistentObject>(FindObjectsSortMode.None);

        // Nếu đã có instance khác -> huỷ bản mới
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Đảm bảo object này không bị huỷ khi load scene mới
        DontDestroyOnLoad(gameObject);
    }
}
