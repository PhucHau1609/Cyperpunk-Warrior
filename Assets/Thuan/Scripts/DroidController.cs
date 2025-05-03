using UnityEngine;

public class DroidController : MonoBehaviour
{
    [Header("Di chuyển")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;

    [Header("Bắn Player")]
    public Transform player;                   // Tham chiếu tới Player
    public float fireRange = 5f;               // Tầm bắn
    public float fireCooldown = 2f;            // Thời gian giữa các lần bắn
    public GameObject bulletPrefab;            // Prefab viên đạn
    public Transform gunPoint;                 // Nòng súng – nơi spawn đạn

    private Rigidbody2D rb;
    private Animator animator;
    private Transform targetPoint;
    private bool facingRight = true;
    private float fireTimer = 0f;
    private bool isShooting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetPoint = pointB;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        fireTimer -= Time.deltaTime;

        if (distanceToPlayer <= fireRange)
        {
            // Dừng lại và bắn
            rb.velocity = Vector2.zero;
            animator.SetBool("IsRunning", false);

            if (fireTimer <= 0f)
            {
                animator.SetTrigger("Shoot");
                fireTimer = fireCooldown;
            }

            // Lật mặt về phía Player nếu cần
            if (player.position.x > transform.position.x && !facingRight)
                Flip();
            else if (player.position.x < transform.position.x && facingRight)
                Flip();
        }
        else
        {
            // Bay giữa A và B
            MoveBetweenPoints();
        }
    }

    void MoveBetweenPoints()
    {
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        animator.SetBool("IsRunning", true);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Hàm này được gọi từ Animation Event trong animation Shoot
    public void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        // Tính hướng đến Player
        Vector2 direction = (player.position - gunPoint.position).normalized;

        // Gán vận tốc bay
        bulletRb.velocity = direction * 5f; // Tốc độ tùy chỉnh

        // Xoay viên đạn về hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
    }

}
