using UnityEngine;

public class Droid01Bullet : MonoBehaviour
{
    public float destroyDelay = 0.5f; // thời gian delay trước khi hủy đạn
    private Rigidbody2D rb;
    private Animator animator;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Player"))
        {
            hasHit = true;
            rb.velocity = Vector2.zero; // dừng đạn
            animator.Play("Destroy");   // kích hoạt animation nổ
            Destroy(gameObject, destroyDelay); // hủy sau animation
        }
    }
}
