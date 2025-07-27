using UnityEngine;

public class Droid02Bullet : MonoBehaviour
{
    public float speed = 8f;                      // Tốc độ bay
    public float destroyDelay = 0.5f;             // Thời gian chờ animation nổ
    public float damage = 2f;                    // Sát thương gây ra cho Player
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool isDestroyed = false;

    private Vector2 direction = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, 3f);
    }

    // Gọi từ Enemy để set hướng viên đạn
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null) rb.linearVelocity = direction * speed;

        // Lật sprite nếu cần
        if (dir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
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
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Destroy");
        Destroy(gameObject, 0.5f);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}