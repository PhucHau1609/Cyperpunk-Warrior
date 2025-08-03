using UnityEngine;

public class SplitDoorController : MonoBehaviour
{
    public Collider2D doorCollider;

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();
    }

    // Gọi từ Animation Event
    public void DisableCollider()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }

    // Tuỳ chọn: bật lại nếu cần
    public void EnableCollider()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }
    }
}
