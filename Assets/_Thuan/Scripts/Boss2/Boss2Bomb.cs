using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class Boss2Bomb : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasExploded = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // Dừng vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Tắt collider
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 1f);
    }
}
