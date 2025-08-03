using UnityEngine;

public class FlyingDroidController : MonoBehaviour, IDamageResponder
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public Transform gunPoint;
    public Transform player;
    public GameObject bulletPrefab; // Thêm bulletPrefab

    [Header("Animation")]
    public Animator animator;

    [HideInInspector] public bool canShoot = true; // Flag để kiểm soát việc bắn

    private Vector3 target;
    private Rigidbody2D rb;
    private bool isMoving = true;

    private enum State { Patrolling, Shooting, Returning, Dead }
    private State currentState = State.Patrolling;

    [Header("Health")]
    public HealthBarEnemy healthBarEnemy;
    private EnemyDamageReceiver damageReceiver;
    private ItemDropTable itemDropTable;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        rb = GetComponent<Rigidbody2D>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        itemDropTable = GetComponent<ItemDropTable>();
    }

    void Start()
    {
        target = pointB.position;

        float normalizedHealth = GetNormalizedHealth();
        if (healthBarEnemy != null)
        {
            if (normalizedHealth < 1f)
                healthBarEnemy.ShowHealthBar(normalizedHealth);
            else
                healthBarEnemy.HideHealthBar();
        }
    }

    void FixedUpdate()
    {
        if (isMoving && currentState != State.Dead && currentState != State.Shooting)
        {
            MoveToTarget();
        }
    }

    void MoveToTarget()
    {
        if (Vector2.Distance(transform.position, target) < 0.2f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }

        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // Flip mặt khi di chuyển
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Set animation cho patrol
        if (animator != null)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Fly", true); // Uncomment nếu có Fly animation
        }
    }

    public void Stop()
    {
        isMoving = false;
        currentState = State.Shooting;
        rb.linearVelocity = Vector2.zero;
        
        // Set animation khi dừng
        if (animator != null)
        {
            animator.SetBool("Run", false);
            animator.SetBool("Fly", false);
        }
    }

    public void Resume()
    {
        isMoving = true;
        currentState = State.Patrolling;
        
        // Set animation khi tiếp tục di chuyển
        if (animator != null)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Fly", true);
        }
    }

    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        // Quay mặt về phía Player
        scale.x = (player.position.x < transform.position.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    /// </summary>
    public void OnShootAnimationEvent()
    {
        if (currentState == State.Dead || player == null || bulletPrefab == null || gunPoint == null)
            return;

        // Bắn theo hướng Player (có thể bắn theo đường chéo)
        Vector2 shootDir = (player.position - gunPoint.position).normalized;

        // Tạo viên đạn
        GameObject bullet = GameObject.Instantiate(
            bulletPrefab,
            gunPoint.position,
            Quaternion.identity
        );

        // Gán hướng cho đạn (có thể dùng Droid01Bullet hoặc script tương tự)
        var bulletScript = bullet.GetComponent<Droid01Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDir);
        }
        else
        {
            // Fallback nếu dùng loại bullet khác
            var droid02BulletScript = bullet.GetComponent<Droid02Bullet>();
            if (droid02BulletScript != null)
            {
                droid02BulletScript.SetDirection(shootDir);
            }
        }
        Debug.DrawLine(gunPoint.position, player.position, Color.red, 1f);
    }

    /// </summary>
    public void OnShootAnimationComplete()
    {
        canShoot = true;
    }

    public void OnShootAnimationStart()
    {
        canShoot = false;
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator?.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        float normalizedHealth = GetNormalizedHealth();
        healthBarEnemy?.ShowHealthBar(normalizedHealth);
    }

    public void OnDead()
    {
        animator?.SetTrigger("Death");
        currentState = State.Dead;

        rb.gravityScale = 1f;
        this.enabled = false;

        healthBarEnemy?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();
        //itemDropTable?.TryDropItems();

        Destroy(gameObject, 2f);
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

    // Getter để behavior tree có thể kiểm tra trạng thái
    public bool IsShooting => currentState == State.Shooting;
    public bool IsPatrolling => currentState == State.Patrolling;
}