using UnityEngine;

public class Droid01Bullet : MonoBehaviour
{
    public float speed = 8f;                      
    public float destroyDelay = 0.5f;             
    private Rigidbody2D rb;
    private Animator animator;
    private bool isDestroyed = false;

    private Vector2 direction = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, 3f);
    }

    // Gọi từ Enemy để set hướng viên đạn
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        // Xoay viên đạn đúng hướng
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        if (!collision.CompareTag("Enemy") && !collision.isTrigger)
        {
            Explode();
        }
    }

    void Explode()
    {
        isDestroyed = true;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animator?.SetTrigger("Destroy");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
