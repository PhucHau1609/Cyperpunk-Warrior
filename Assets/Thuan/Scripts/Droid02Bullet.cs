using UnityEngine;

public class Droid02Bullet : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private bool isDestroyed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        if (collision.CompareTag("Player"))
        {
            isDestroyed = true;
            rb.velocity = Vector2.zero;                 // Dừng đạn
            animator.SetTrigger("Destroy");             // Kích hoạt animation nổ

            Destroy(gameObject, 0.5f);                  // Tự hủy sau animation
        }
    }
}
