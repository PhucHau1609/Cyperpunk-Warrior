using UnityEngine;

public class Droid02Bullet : MonoBehaviour
{
    public float speed = 8f;                      // Tốc độ bay
    public float destroyDelay = 0.5f;             // Thời gian chờ animation nổ
    private Rigidbody2D rb;
    private Animator animator;
    private bool isDestroyed = false;

    private Vector2 direction = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.velocity = direction * speed;

        Destroy(gameObject, 3f);
    }

    // Gọi từ Enemy để set hướng viên đạn
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null) rb.velocity = direction * speed;

        // Lật sprite nếu cần
        if (dir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        // Đạn gặp bất kỳ collider nào cũng nổ (trừ Enemy hoặc chính nó)
        if (!collision.CompareTag("Enemy") && !collision.isTrigger)
        {
            Explode();
        }
    }

    void Explode()
    {
        isDestroyed = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Destroy");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
