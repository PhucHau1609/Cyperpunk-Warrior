using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageResponder
{
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;
    public Transform gunPoint;
    public Animator animator;
    public Slider healthSlider;
    public Transform player;
    
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;

    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public bool canShoot = true; // Flag để kiểm soát việc bắn

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;

    public HealthBarEnemy healthBarEnemy;
    private EnemyDamageReceiver damageReceiver;
    private ItemDropTable itemDropTable;

    private bool isDead = false;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        damageReceiver = GetComponent<EnemyDamageReceiver>();
        rb = GetComponent<Rigidbody2D>();
        itemDropTable = GetComponent<ItemDropTable>();
    }

    void Start()
    {
        initialPosition = transform.position;

        // Ẩn HealthBar lúc bắt đầu (nếu đầy máu)
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
        StickToGround();
    }

    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x < transform.position.x) ? -1 : 1;
        transform.localScale = scale;
    }

    void StickToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        if (hit.collider != null)
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.1f;
            transform.position = pos;
        }
    }

    public void OnShootAnimationEvent()
    {
        if (currentState == State.Dead || player == null || bulletPrefab == null || gunPoint == null)
            return;

        // Bắn chỉ theo trục X (trái hoặc phải)
        float xDir = (player.position.x < transform.position.x) ? -1f : 1f;
        Vector2 shootDir = new Vector2(xDir, 0f);

        // Tạo viên đạn
        GameObject bullet = GameObject.Instantiate(
            bulletPrefab,
            gunPoint.position,
            Quaternion.identity
        );

        // Gán hướng cho đạn
        Droid02Bullet bulletScript = bullet.GetComponent<Droid02Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDir);
        }

        Debug.Log($"[{gameObject.name}] Fired bullet at {Time.time}");
    }

    public void OnShootAnimationComplete()
    {
        canShoot = true;
        Debug.Log($"[{gameObject.name}] Shoot animation completed");
    }
    public void OnShootAnimationStart()
    {
        canShoot = false;
        Debug.Log($"[{gameObject.name}] Shoot animation started");
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        if (healthBarEnemy != null)
        {
            healthBarEnemy.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        if (isDead) return; 
        isDead = true;

        animator.SetTrigger("Death");
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        rb.angularVelocity = 0f;
        GetComponent<Collider2D>().enabled = false;
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
}