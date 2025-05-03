using UnityEngine;

public class Droid02Controller : MonoBehaviour
{
    [Header("Thông tin phát hiện Player")]
    public Transform player;
    public float chaseRange = 5f;
    public float stopChaseRange = 7f;

    [Header("Di chuyển")]
    public float moveSpeed = 2f;
    private Vector3 originalPosition;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isChasing = false;
    private bool facingRight = true;
    private bool isShooting = false;

    [Header("Bắn")]
    public GameObject bulletPrefab;
    public Transform gunPoint;
    public float fireRange = 4f;
    public float fireCooldown = 2f;
    private float fireTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalPosition = transform.position;
    }

    void FixedUpdate()
{
    float distanceToPlayer = Vector2.Distance(transform.position, player.position);
    fireTimer -= Time.deltaTime;

    // Nếu player trong tầm bắn và cooldown xong thì bắn
    if (distanceToPlayer <= fireRange && fireTimer <= 0f)
    {
        StartShooting();
    }

    // Cho phép di chuyển kể cả khi đang bắn
    HandleMovement(distanceToPlayer);
}


    void StartShooting()
    {
        isShooting = true;
        fireTimer = fireCooldown;
        rb.velocity = Vector2.zero;
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("Shoot"); // Animation sẽ gọi FireBullet và EndShoot
    }

    void HandleMovement(float distanceToPlayer)
    {
        if (distanceToPlayer < chaseRange)
            isChasing = true;
        else if (distanceToPlayer > stopChaseRange)
            isChasing = false;

        Vector2 targetPosition = isChasing ? player.position : originalPosition;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        if (!isChasing && Vector2.Distance(transform.position, targetPosition) < 0.05f)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            return;
        }

        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        animator.SetBool("IsRunning", Mathf.Abs(rb.velocity.x) > 0.1f);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            if (direction.x > 0 && !facingRight)
                Flip();
            else if (direction.x < 0 && facingRight)
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

    // Gọi từ Animation Event
    public void FireBullet()
    {
        Vector3 spawnPosition = gunPoint.position;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        float directionX = facingRight ? 1f : -1f;
        bulletRb.velocity = new Vector2(directionX * 5f, 0f);
        bullet.transform.rotation = Quaternion.Euler(0, 0, facingRight ? 0 : 180);
    }

    // Gọi ở cuối animation Shoot
    public void EndShoot()
    {
        isShooting = false;
    }
}
