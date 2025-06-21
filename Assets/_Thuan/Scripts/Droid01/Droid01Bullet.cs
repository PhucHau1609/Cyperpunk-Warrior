using UnityEngine;

public class Droid01Bullet : MonoBehaviour
{
    public float speed = 8f;                      
    public float destroyDelay = 0.5f;
    public float damage = 15f;                    // Sát thương gây ra cho Player
    
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

        // Kiểm tra nếu đạn trúng Player
        if (collision.CompareTag("Player"))
        {
            // Lấy component CharacterController2D từ Player
            CharacterController2D playerController = collision.GetComponent<CharacterController2D>();
            
            if (playerController != null)
            {
                // Gây sát thương cho Player, truyền vị trí của đạn làm điểm gây damage
                playerController.ApplyDamage(damage, transform.position);
            }
            
            // Nổ đạn sau khi gây sát thương
            Explode();
        }
        // Đạn gặp bất kỳ collider nào khác cũng nổ (trừ Enemy hoặc chính nó)
        else if (!collision.CompareTag("Enemy") && !collision.isTrigger)
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