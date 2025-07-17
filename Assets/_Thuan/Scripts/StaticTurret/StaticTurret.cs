using UnityEngine;

public class StaticTurret : MonoBehaviour, IDamageResponder
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public LayerMask playerLayer = 1;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform gunPoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;

    [Header("Direction")]
    public bool facingRight = true; // Hướng mặt của trụ súng

    private Transform player;
    private Animator animator;
    private float lastFireTime;
    private bool playerInRange;
    private bool isDead = false;
    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;
    private StaticTurretDamageReceiver damageReceiver;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        damageReceiver = GetComponent<StaticTurretDamageReceiver>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null || isDead) return; // Thêm điều kiện isDead
    
        CheckPlayerInRange();
        
        if (playerInRange && CanShoot())
        {
            // Bỏ Shoot() khỏi đây, chỉ trigger animation
            TriggerAttack();
        }
    }

    void CheckPlayerInRange()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position;

        // Raycast chỉ theo hướng mặt của trụ súng
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectionRange, playerLayer);

        bool wasInRange = playerInRange;
        playerInRange = hit.collider != null && hit.collider.transform == player;

        // Kiểm tra thêm vị trí player có đúng hướng không
        if (playerInRange)
        {
            float playerDirection = player.position.x - transform.position.x;
            if (facingRight && playerDirection < 0) playerInRange = false;
            if (!facingRight && playerDirection > 0) playerInRange = false;
        }

        // Chuyển animation
        if (playerInRange && !wasInRange)
        {
            animator.SetBool("PlayerInRange", true);
        }
        else if (!playerInRange && wasInRange)
        {
            animator.SetBool("PlayerInRange", false);
        }
    }

    bool CanShoot()
    {
        return Time.time - lastFireTime >= 1f / fireRate;
    }

    void TriggerAttack()
    {
        lastFireTime = Time.time;
        animator.SetTrigger("Attack");
        // Bỏ code tạo đạn khỏi đây
    }

    public void FireBullet()
    {
        if (isDead) return;
        
        // Di chuyển code tạo đạn vào đây
        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);
        Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
        
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletSpeed;
        }
        
        if (!facingRight)
        {
            bullet.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = State.Dead;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 2f);
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ tầm bắn trong Scene view
        Gizmos.color = Color.red;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * detectionRange);
    }
}