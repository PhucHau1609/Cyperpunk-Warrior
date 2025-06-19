using UnityEngine;
using BehaviorDesigner.Runtime;

public class BossPhuController : MonoBehaviour, IDamageResponder
{
    [Header("Boss Stats")]
    public float moveSpeed = 3f;
    public float rangedAttackRange = 8f;
    public float meleeAttackRange = 3f;
    public float detectionRange = 15f;

    [Header("Attack Settings")]
    public Transform[] bulletSpawnPoints;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float attackCooldown = 2f;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    [HideInInspector] public Transform player;
    private BehaviorTree behaviorTree;

    [Header("Boss States")]
    [HideInInspector] public bool isPhase2 = false;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool hasChangedPhase = false;
    [HideInInspector] public float lastAttackTime;
    [HideInInspector] public bool playerDetected = false;

    public BossPhuHealthBar healthBar;
    private State currentState = State.Idle;
    public BossDamageReceiver damageReceiver;

    void Awake()
    {
        damageReceiver = GetComponent<BossDamageReceiver>();
    }
    void Start()
    {
        InitializeBoss();

        float normalizedHealth = GetNormalizedHealth();
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(normalizedHealth);
        }
    }

    void InitializeBoss()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();

        damageReceiver = GetComponent<BossDamageReceiver>();

        // Find Player immediately when scene starts
        FindPlayer();

        // Freeze rotation
        rb.freezeRotation = true;
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Eren");
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerDetected = true; // Ngay khi tìm thấy Player là target luôn
            Debug.Log("Boss đã phát hiện Player!");
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy Player, sẽ tìm lại...");
        }
    }

    void Update()
    {
        // Liên tục tìm Player nếu chưa có (vì Player là DontDestroyOnLoad)
        if (player == null || !playerDetected)
        {
            FindPlayer();
        }

        // Update Behavior Designer variables
        //UpdateBehaviorVariables();

        // Flip Boss để nhìn về phía Player
        if (!isDead && player != null)
        {
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        // Check phase change
        CheckPhaseChange();
        CheckDeathState();
    }

    void CheckPhaseChange()
    {
        if (damageReceiver.CurrentHP <= damageReceiver.MaxHP * 0.5f && !hasChangedPhase)
        {
            hasChangedPhase = true;
            StartPhaseChange();
        }
    }

    void CheckDeathState()
    {
        // Kiểm tra trạng thái chết từ DamageReceiver
        if (damageReceiver != null && damageReceiver.IsDead() && currentState != State.Dead)
        {
            OnDead();
        }
    }

    public void StartPhaseChange()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("PhaseChange");

        Invoke("EndPhaseChange", 2f);
    }

    void EndPhaseChange()
    {
        isPhase2 = true;
        isAttacking = false;
        Debug.Log("Boss đã chuyển sang Phase 2 - Có thêm tấn công tầm xa!");
    }

    // Methods cho Behavior Designer Tasks
    public void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        CameraFollow.Instance?.ShakeCamera();
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void FireBullet()
    {
        if (bulletPrefab != null && bulletSpawnPoints.Length > 0 && player != null)
        {
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
                BossPhuBullet bulletScript = bullet.GetComponent<BossPhuBullet>();
                if (bulletScript != null)
                {
                    Vector2 direction = (player.position - spawnPoint.position).normalized;
                    bulletScript.Initialize(direction, bulletSpeed);
                }
            }
            Debug.Log("Boss bắn đạn!");
        }
    }

    public float GetDistanceToPlayer()
    {
        if (player != null)
        {
            return Vector2.Distance(transform.position, player.position);
        }
        return float.MaxValue;
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown && !isAttacking && !isDead;
    }

    private enum State
    {
        Idle,
        Attacking,
        Hurt,
        Dead
    }

    // Cải tiến phương thức TakeDamage
    public void TakeDamage(int damage)
    {
        if (damageReceiver != null)
        {
            damageReceiver.TakeDamage(damage);
            OnDamageReceived();
        }
    }

    public void OnDamageReceived()
    {
        // Cập nhật thanh máu
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(damageReceiver.CurrentHP);
        }

        // Xử lý khi bị thương (nếu chưa chết)
        if (!damageReceiver.IsDead())
        {
            OnHurt();
        }
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        if (healthBar != null)
        {
            healthBar.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        rb.angularVelocity = 0f;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        healthBar?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();

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