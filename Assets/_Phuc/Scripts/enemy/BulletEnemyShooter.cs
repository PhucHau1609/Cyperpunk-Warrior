using UnityEngine;

public class BulletEnemyShooter : MonoBehaviour
{
    public float speed = 8f;
    private Vector2 direction;

    private void Start()
    {
        // Tự hủy sau 5 giây nếu không va chạm
        Destroy(gameObject, 5f);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Xoay theo hướng bay và bù lại góc nghiêng của sprite là 45 độ
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f); // cộng hoặc trừ 45 tùy thiết kế
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
