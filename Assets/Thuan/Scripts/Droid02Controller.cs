using System.Collections;
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
    private bool isReloading = false;
    private bool isDead = false;

    [Header("Bắn")]
    public GameObject bulletPrefab;
    public Transform gunPoint;
    public float fireRange = 4f;
    public float fireCooldown = 2f;
    private float fireTimer = 0f;

    public int maxAmmo = 3;
    private int currentAmmo;

    [Header("Nạp đạn")]
    public float reloadTime = 3f;


    [Header("Máu")]
    public float Hitpoints;
    public float MaxHitpoints = 5f;
    public HealthBarEnemy HealthBar;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalPosition = transform.position;

        Hitpoints = MaxHitpoints;
        HealthBar.SetHealth(Hitpoints, MaxHitpoints);

        currentAmmo = maxAmmo;
    }

    void FixedUpdate()
    {
        if (isDead || isReloading) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        fireTimer -= Time.deltaTime;

        if (distanceToPlayer <= fireRange && fireTimer <= 0f)
        {
            StartShooting();
        }

        HandleMovement(distanceToPlayer);
    }


    void StartShooting()
    {
        if (currentAmmo > 0)
        {
            fireTimer = fireCooldown;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("Shoot");

            currentAmmo--;
        }
        else
        {
            StartReloading();
        }
    }

    void StartReloading()
    {
        isReloading = true;
        animator.SetTrigger("CountDown");
        rb.velocity = Vector2.zero; // đứng yên khi bắt đầu nạp đạn
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        // Dừng di chuyển trong thời gian nạp đạn
        float timer = 0f;
        while (timer < reloadTime)
        {
            rb.velocity = Vector2.zero; // giữ đứng yên
            timer += Time.deltaTime;
            yield return null;
        }

        FinishReloading(); // Gọi hàm nạp xong
    }


    // Gọi ở cuối animation CountDown
    public void FinishReloading()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
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

    public void FireBullet()
    {
        Vector3 spawnPosition = gunPoint.position;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        float directionX = facingRight ? 1f : -1f;
        bulletRb.velocity = new Vector2(directionX * 5f, 0f);
        bullet.transform.rotation = Quaternion.Euler(0, 0, facingRight ? 0 : 180);
    }

    public void EndShoot()
    {
        // Được gọi ở cuối animation Shoot
    }

    // --- THÊM: Gọi khi bị Player tấn công
   public void ApplyDamage(float amount)
    {
        if (isDead) return;

        Hitpoints -= Mathf.Abs(amount);
        HealthBar.SetHealth(Hitpoints, MaxHitpoints);
        animator.SetTrigger("Hurt");

        if (Hitpoints <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Death");

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
