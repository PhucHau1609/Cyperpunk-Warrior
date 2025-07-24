using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int splitLevel = 0; // 0 = Large, 1 = Medium, 2 = Small
    
    [Header("Melee Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackDamage = 5f;
    
    [Header("Split Settings")]
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private float splitForce = 5f;
    
    [Header("Explosion Settings (Small Slime Only)")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 5f;
    [SerializeField] private float explosionDelay = 0.1f; // Thời gian delay trước khi nổ
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f; // Phạm vi phát hiện player
    
    // Private variables
    private float currentHealth;
    private float lastAttackTime;
    private bool facingRight = true;
    private bool isExploding = false; // Để tránh explode nhiều lần
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private bool canSplit = true;
    
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Exploding, // State mới cho Small slime
        Hurt,
        Dead
    }
    
    private EnemyState currentState = EnemyState.Idle;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Adjust stats based on split level
        AdjustStatsForSplitLevel();
        
        // Disable splitting for small slimes
        if (splitLevel >= 2)
            canSplit = false;
    }
    
    void Update()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Exploding) return;
        
        HandleState();
        HandleAnimation();
    }
    
    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Exploding) return;
        HandleMovement();
    }
    
    private void HandleState()
    {
        if (currentState == EnemyState.Hurt) return;
        
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
        
        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                    currentState = EnemyState.Chase;
                break;
                
            case EnemyState.Chase:
                // Small slime explodes when close to player
                if (splitLevel >= 2)
                {
                    if (distanceToPlayer <= explosionRadius * 0.5f)
                        StartExplosion();
                }
                else
                {
                    // Large and Medium slimes attack normally
                    if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                        currentState = EnemyState.Attack;
                    else if (distanceToPlayer > detectionRange * 1.5f)
                        currentState = EnemyState.Idle;
                }
                break;
                
            case EnemyState.Attack:
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformMeleeAttack();
                    lastAttackTime = Time.time;
                }
                break;
        }
    }
    
    private void HandleMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        
        switch (currentState)
        {
            case EnemyState.Chase:
                if (player != null)
                {
                    float direction = player.position.x > transform.position.x ? 1 : -1;
                    if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
                        Flip();
                    
                    velocity.x = direction * moveSpeed;
                }
                break;
                
            case EnemyState.Attack:
                velocity.x = 0;
                break;
                
            default:
                velocity.x = 0;
                break;
        }
        
        rb.linearVelocity = velocity;
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    private void HandleAnimation()
    {
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && currentState != EnemyState.Attack;
        animator.SetBool("Run", isMoving);
    }
    
    // THÊM HÀM MỚI: Start Explosion cho Small Slime
    private void StartExplosion()
    {
        if (isExploding) return;
        
        isExploding = true;
        currentState = EnemyState.Exploding;
        animator.SetTrigger("Explode");
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        rb.angularVelocity = 0f;
        
        // Delay trước khi nổ
        Invoke(nameof(Explode), explosionDelay);
    }
    
    // THÊM HÀM MỚI: Explode
    private void Explode()
    {
        // Damage player if in range
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= explosionRadius)
            {
                // Get player script và gọi ApplyDamage
                var playerController = player.GetComponent<CharacterController2D>(); // Thay PlayerController bằng tên script player của bạn
                if (playerController != null)
                {
                    playerController.ApplyDamage(explosionDamage, transform.position);
                }
            }
        }
        
        // Camera shake
        CameraFollow.Instance?.ShakeCamera();
        
        // Destroy immediately
        Destroy(gameObject);
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Exploding) return;
        
        currentHealth -= damageAmount;
        
        if (currentHealth <= 0)
        {
            OnDead();
        }
        else
        {
            OnHurt();
            currentState = EnemyState.Hurt;
            Invoke(nameof(RecoverFromHurt), 0.5f);
        }
    }
    
    public void OnHurt()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Exploding) return;
        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();
    }
    
    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = EnemyState.Dead;
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        rb.angularVelocity = 0f;
        
        // Split logic
        if (canSplit && splitLevel < 2)
        {
            SplitSlime();
        }
        
        this.enabled = false;
        Destroy(gameObject, 2f);
    }
    
    private void RecoverFromHurt()
    {
        if (currentState == EnemyState.Hurt)
            currentState = EnemyState.Chase;
    }
    
    private void SplitSlime()
    {
        if (slimePrefab == null) return;
        
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-1f, 1f), 
                Random.Range(0.5f, 1f), 
                0
            );
            
            GameObject newSlime = Instantiate(slimePrefab, spawnPosition, Quaternion.identity);
            SlimeEnemy newSlimeScript = newSlime.GetComponent<SlimeEnemy>();
            
            if (newSlimeScript != null)
            {
                newSlimeScript.splitLevel = splitLevel + 1;
                
                Rigidbody2D newRb = newSlime.GetComponent<Rigidbody2D>();
                if (newRb != null)
                {
                    Vector2 force = new Vector2(
                        i == 0 ? -splitForce : splitForce,
                        splitForce * 0.5f
                    );
                    newRb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }
    }
    
    // CẬP NHẬT HÀM: AdjustStatsForSplitLevel
    private void AdjustStatsForSplitLevel()
    {
        float damageMultiplier = 1f;
        float scaleMultiplier = 1f;
        
        switch (splitLevel)
        {
            case 0: // Large slime
                damageMultiplier = 1f;
                scaleMultiplier = 1f;
                break;
            case 1: // Medium slime
                damageMultiplier = 0.5f;
                scaleMultiplier = 0.7f;
                moveSpeed *= 1.2f;
                break;
            case 2: // Small slime
                damageMultiplier = 0.25f;
                scaleMultiplier = 0.5f;
                moveSpeed *= 1.5f;
                break;
        }
        
        attackDamage *= damageMultiplier;
        transform.localScale *= scaleMultiplier;
        
        canSplit = splitLevel < 2;
    }
    
    private void PerformMeleeAttack()
    {
        // Chỉ Large và Medium slime mới attack, Small slime sẽ explode
        if (splitLevel >= 2) return;
        
        animator.SetTrigger("Attack");
        currentState = EnemyState.Attack;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    // CẬP NHẬT HÀM: OnAttackHit
    public void OnAttackHit()
    {
        if (attackPoint == null || splitLevel >= 2) return;
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            // Gọi ApplyDamage của player
            var playerController = playerCollider.GetComponent<CharacterController2D>(); // Thay PlayerController bằng tên script player của bạn
            if (playerController != null)
            {
                playerController.ApplyDamage(attackDamage, transform.position);
            }
        }
    }
    
    public void OnAttackEnd()
    {
        if (currentState == EnemyState.Attack)
            currentState = EnemyState.Chase;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range for Large/Medium slimes
        if (splitLevel < 2)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (attackPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            }
        }
        
        // Draw explosion radius for Small slimes
        if (splitLevel >= 2)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}