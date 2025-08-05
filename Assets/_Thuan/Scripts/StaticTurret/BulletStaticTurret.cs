using UnityEngine;

public class BulletStaticBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 3f;
    public int damage = 2;
    private bool isDestroyed = false;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        // Tự hủy sau một thời gian
        Destroy(gameObject, lifetime);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        if (collision.CompareTag("Player"))
        {
            CharacterController2D playerController = collision.GetComponent<CharacterController2D>();

            if (playerController != null)
            {
                // Gây sát thương cho Player, truyền vị trí của đạn làm điểm gây damage
                playerController.ApplyDamage(damage, transform.position);
            }

            // Nổ đạn sau khi gây sát thương
            Explode();
        }
        else if (collision.CompareTag("Ground"))
        {
            Explode();
        }
    }
    
    void Explode()
    {
        isDestroyed = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Destroy");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}