using UnityEngine;

public class Bullet : MonoBehaviour
{
    void Start()
    {
        // Tự hủy sau 2 giây nếu không va chạm
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Player"))
        {
            Destroy(gameObject); // Hủy viên đạn ngay khi chạm
        }
    }
}
