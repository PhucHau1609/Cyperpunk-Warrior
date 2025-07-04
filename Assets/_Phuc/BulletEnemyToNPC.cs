using UnityEngine;

public class BulletEnemyToNPC : MonoBehaviour
{
    public float speed = 8f;
    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, 5f); // Tự hủy sau 5s
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Xoay theo hướng bay (nếu sprite nghiêng, bạn có thể cộng/trừ góc tùy ý)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
