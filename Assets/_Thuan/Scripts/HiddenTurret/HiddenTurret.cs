using UnityEngine;
using System.Collections;

public class HiddenTurret : MonoBehaviour, IDamageResponder
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

    [Header("Hidden Behavior")]
    public float idleTimeBeforeHiding = 3f;

    private Transform player;
    private Animator animator;
    private Collider2D col;
    private float lastFireTime;
    private bool playerInRange;
    private bool isDead = false;
    private Coroutine hideCoroutine;
    private HiddenTurretDamageReceiver damageReceiver;

    public enum TurretState
    {
        Hidden,
        Emerging,
        Active,
        Hiding,
        Dead
    }

    private TurretState currentState = TurretState.Hidden;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        damageReceiver = GetComponent<HiddenTurretDamageReceiver>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Bắt đầu ở trạng thái ẩn
        animator.SetBool("IsHidden", true);
        col.enabled = false;
    }

    void Update()
    {
        if (player == null || isDead) return; // Thêm điều kiện isDead

        CheckPlayerInRange();
        HandleStateMachine();
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
    }

    void HandleStateMachine()
    {
        if (isDead) return;

        switch (currentState)
        {
            case TurretState.Hidden:
                if (playerInRange)
                {
                    StartEmerging();
                }
                break;

            case TurretState.Emerging:
                // Chờ animation hoàn thành
                break;

            case TurretState.Active:
                if (playerInRange)
                {
                    if (CanShoot())
                    {
                        TriggerAttack();
                    }

                    // Hủy coroutine ẩn nếu player vẫn trong tầm
                    if (hideCoroutine != null)
                    {
                        StopCoroutine(hideCoroutine);
                        hideCoroutine = null;
                    }
                }
                else
                {
                    // Player rời khỏi tầm, bắt đầu đếm thời gian ẩn
                    if (hideCoroutine == null)
                    {
                        hideCoroutine = StartCoroutine(WaitAndHide());
                    }
                }
                break;

            case TurretState.Hiding:
                // Chờ animation hoàn thành
                break;
        }
    }

    void StartEmerging()
    {
        if (currentState != TurretState.Hidden) return;

        currentState = TurretState.Emerging;

        animator.SetBool("IsHidden", false);
        animator.SetTrigger("Emerge");
    }

    void StartHiding()
    {
        if (currentState != TurretState.Active) return;

        currentState = TurretState.Hiding;

        animator.SetTrigger("Hide");
    }

    IEnumerator WaitAndHide()
    {
        yield return new WaitForSeconds(idleTimeBeforeHiding);

        if (!playerInRange) // Kiểm tra lại player có còn trong tầm không
        {
            StartHiding();
        }

        hideCoroutine = null;
    }

    bool CanShoot()
    {
        return currentState == TurretState.Active &&
               Time.time - lastFireTime >= 1f / fireRate;
    }

    void TriggerAttack()
    {
        lastFireTime = Time.time;
        animator.SetTrigger("Attack");
        // Bỏ code tạo đạn khỏi đây
    }

    // Method mới để gọi từ Animation Event
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

    // Gọi từ Animation Event khi animation Up hoàn thành
    public void OnEmergeComplete()
    {
        currentState = TurretState.Active;
        animator.SetBool("IsActive", true);
        col.enabled = true;
    }

    // Gọi từ Animation Event khi animation Down hoàn thành  
    public void OnHideComplete()
    {
        currentState = TurretState.Hidden;
        animator.SetBool("IsHidden", true);
        animator.SetBool("IsActive", false);
        col.enabled = false;
    }

    public void OnHurt()
    {
        if (currentState == TurretState.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = TurretState.Dead;
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