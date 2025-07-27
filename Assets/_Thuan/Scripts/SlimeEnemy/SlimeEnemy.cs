using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int splitLevel = 0; // 0 = Large, 1 = Medium, 2 = Small

    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB; 
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 1f; // Thời gian đợi tại mỗi điểm

    // Private patrol variables
    private Transform currentTarget;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    
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

    [Header("Parent Reference (Auto-assigned)")]
    private SlimeEnemy parentSlime;
    [Header("Auto Patrol Setup")]
    [SerializeField] private string patrolGroupName = "SlimePatrol"; // Tên nhóm patrol points mặc định
    [SerializeField] private bool autoFindPatrolPoints = true; // Checkbox để bật/tắt auto find
    [SerializeField] private float patrolSearchRadius = 20f;
    
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
        Patrol,
        Attack,
        Exploding,
        Hurt,
        Dead
    }
    
    private EnemyState currentState = EnemyState.Patrol;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // ===== THÊM: Auto find patrol points =====
        if (autoFindPatrolPoints && (pointA == null || pointB == null))
        {
            AutoAssignPatrolPoints();
        }
        
        // Adjust stats based on split level
        AdjustStatsForSplitLevel();
        
        // Disable splitting for small slimes
        if (splitLevel >= 2)
            canSplit = false;
        
        // Setup patrol
        SetupPatrol();
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
    
    private void SetupPatrol()
    {
        if (splitLevel <= 1 && pointA != null && pointB != null)
        {
            // Chọn điểm gần nhất làm target đầu tiên
            float distanceToA = Vector2.Distance(transform.position, pointA.position);
            float distanceToB = Vector2.Distance(transform.position, pointB.position);
            
            currentTarget = distanceToA < distanceToB ? pointA : pointB;
        }
        else if (splitLevel <= 1)
        {
            Debug.LogWarning($"Patrol points not set for slime (level {splitLevel}): {gameObject.name}");
        }
    }
    
    private void AutoAssignPatrolPoints()
    {
        // Method 1: Tìm theo tên nhóm cụ thể
        if (!string.IsNullOrEmpty(patrolGroupName))
        {
            FindPatrolPointsByGroupName();
            if (pointA != null && pointB != null) return; // Thành công thì return
        }
        
        // Method 2: Tìm patrol points bằng tag
        FindNearestPatrolPointsByTag();
        if (pointA != null && pointB != null) return; // Thành công thì return
        
        // Method 3: Tìm theo tên (backup)
        FindPatrolPointsByName();
    }

    // THÊM HÀM: FindPatrolPointsByGroupName
    private void FindPatrolPointsByGroupName()
    {
        // Tìm parent object chứa patrol points
        GameObject patrolGroup = GameObject.Find(patrolGroupName);
        if (patrolGroup == null)
        {
            Debug.LogWarning($"Không tìm thấy patrol group: {patrolGroupName}");
            return;
        }
        
        // Lấy children làm patrol points
        if (patrolGroup.transform.childCount >= 2)
        {
            pointA = patrolGroup.transform.GetChild(0);
            pointB = patrolGroup.transform.GetChild(1);
            Debug.Log($"Auto assigned patrol points from group '{patrolGroupName}' for {gameObject.name}: {pointA.name} and {pointB.name}");
        }
        else
        {
            Debug.LogWarning($"Patrol group '{patrolGroupName}' cần ít nhất 2 children");
        }
    }

    // THÊM HÀM: FindNearestPatrolPointsByTag
    private void FindNearestPatrolPointsByTag()
    {
        // Tìm tất cả objects có tag "PatrolPoint"
        GameObject[] allPatrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
        
        if (allPatrolPoints.Length < 2)
        {
            return; // Không đủ points, thử method khác
        }
        
        // Lọc những points trong bán kính tìm kiếm
        System.Collections.Generic.List<GameObject> nearbyPoints = new System.Collections.Generic.List<GameObject>();
        foreach (GameObject point in allPatrolPoints)
        {
            float distance = Vector2.Distance(transform.position, point.transform.position);
            if (distance <= patrolSearchRadius)
            {
                nearbyPoints.Add(point);
            }
        }
        
        // Nếu không đủ points gần, lấy tất cả
        if (nearbyPoints.Count < 2)
        {
            nearbyPoints.Clear();
            nearbyPoints.AddRange(allPatrolPoints);
        }
        
        // Sắp xếp theo khoảng cách
        nearbyPoints.Sort((a, b) => 
        {
            float distA = Vector2.Distance(transform.position, a.transform.position);
            float distB = Vector2.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
        
        pointA = nearbyPoints[0].transform;
        pointB = nearbyPoints[1].transform;
        
        Debug.Log($"Auto assigned nearest patrol points by tag for {gameObject.name}: {pointA.name} and {pointB.name}");
    }

    // THÊM HÀM: FindPatrolPointsByName (backup method)
    private void FindPatrolPointsByName()
    {
        // Tìm các objects có tên chứa "Patrol", "Point", etc.
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        System.Collections.Generic.List<GameObject> patrolCandidates = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if (objName.Contains("patrol") || objName.Contains("point") || objName.Contains("waypoint"))
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance <= patrolSearchRadius)
                {
                    patrolCandidates.Add(obj);
                }
            }
        }
        
        if (patrolCandidates.Count >= 2)
        {
            // Sắp xếp theo khoảng cách
            patrolCandidates.Sort((a, b) => 
            {
                float distA = Vector2.Distance(transform.position, a.transform.position);
                float distB = Vector2.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });
            
            pointA = patrolCandidates[0].transform;
            pointB = patrolCandidates[1].transform;
            
            Debug.Log($"Auto assigned patrol points by name for {gameObject.name}: {pointA.name} and {pointB.name}");
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy đủ patrol points cho {gameObject.name}. Slime sẽ không patrol.");
        }
    }

    // THÊM CÁC PUBLIC METHODS (nếu cần control từ bên ngoài):
    public void SetPatrolPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
        autoFindPatrolPoints = false; // Tắt auto find khi đã set manual
        SetupPatrol(); // Re-setup với points mới
    }

    public void ForceAutoFindPatrolPoints()
    {
        autoFindPatrolPoints = true;
        AutoAssignPatrolPoints();
        SetupPatrol();
    }
    
    private void HandleState()
    {
        if (currentState == EnemyState.Hurt) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        switch (currentState)
        {
            case EnemyState.Patrol:
                // ===== THÊM: Logic riêng cho Small slime (splitLevel = 2) =====
                if (splitLevel >= 2)
                {
                    // Small slime: Chase player và explode khi gần
                    if (player != null && distanceToPlayer <= detectionRange)
                    {
                        // Quay mặt về phía Player
                        float directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
                        if ((directionToPlayer > 0 && !facingRight) || (directionToPlayer < 0 && facingRight))
                            Flip();

                        // Explode khi đủ gần
                        if (distanceToPlayer <= explosionRadius * 0.5f)
                            StartExplosion();
                    }
                    return; // Small slime không patrol
                }

                // ===== Logic cho Large (0) và Medium (1) slimes =====
                if (distanceToPlayer <= attackRange)
                {
                    // Quay mặt về phía Player khi vào vùng tấn công
                    if (player != null)
                    {
                        float directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
                        if ((directionToPlayer > 0 && !facingRight) || (directionToPlayer < 0 && facingRight))
                            Flip();
                    }

                    // Large/Medium slime attacks
                    if (Time.time >= lastAttackTime + attackCooldown)
                        currentState = EnemyState.Attack;
                }
                else
                {
                    // ===== SỬA: Cả Large và Medium đều patrol =====
                    if (splitLevel <= 1) // Large (0) và Medium (1) đều patrol
                    {
                        HandlePatrolLogic();
                    }
                }
                break;

            case EnemyState.Attack:
                // Đảm bảo vẫn quay mặt về Player trong lúc tấn công
                if (player != null)
                {
                    float directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
                    if ((directionToPlayer > 0 && !facingRight) || (directionToPlayer < 0 && facingRight))
                        Flip();
                }

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformMeleeAttack();
                    lastAttackTime = Time.time;
                }
                break;
        }
    }
    
    private void HandlePatrolLogic()
    {
       if (pointA == null || pointB == null || currentTarget == null) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
            }
            return;
        }
        
        // ===== SỬA: Chỉ kiểm tra khoảng cách theo trục X =====
        float distanceToTargetX = Mathf.Abs(transform.position.x - currentTarget.position.x);
        
        // ===== SỬA: Điều chỉnh threshold dựa trên splitLevel =====
        float threshold = splitLevel == 0 ? 0.5f : 1f; // Medium slime có threshold lớn hơn
        
        if (distanceToTargetX <= threshold)
        {
            // Đổi target và bắt đầu wait
            currentTarget = currentTarget == pointA ? pointB : pointA;
            isWaiting = true;
            
            // Flip để quay mặt về hướng target mới
            float direction = currentTarget.position.x > transform.position.x ? 1 : -1;
            if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
                Flip();
        }
    }
    
    private Vector2 GetAdjustedPatrolPosition(Transform patrolPoint)
    {
        if (patrolPoint == null) return Vector2.zero;
        
        // Giữ Y của slime hiện tại, chỉ lấy X của patrol point
        return new Vector2(patrolPoint.position.x, transform.position.y);
    }
    
    private void HandleMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (splitLevel >= 2)
                {
                    // Small slime chase player
                    if (player != null && distanceToPlayer <= detectionRange)
                    {
                        float direction = player.position.x > transform.position.x ? 1 : -1;
                        velocity.x = direction * moveSpeed;
                    }
                    else
                    {
                        velocity.x = 0;
                    }
                }
                else if (distanceToPlayer <= attackRange)
                {
                    // Dừng di chuyển khi player trong tầm tấn công
                    velocity.x = 0;
                }
                else if (splitLevel <= 1 && !isWaiting && currentTarget != null)
                {
                    // ===== SỬA: Sử dụng vị trí X của patrol point =====
                    Vector2 adjustedTargetPos = GetAdjustedPatrolPosition(currentTarget);
                    float direction = adjustedTargetPos.x > transform.position.x ? 1 : -1;
                    
                    if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
                        Flip();
                    
                    velocity.x = direction * patrolSpeed;
                }
                else
                {
                    velocity.x = 0;
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
            currentState = EnemyState.Patrol;
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
                
                // ===== THÊM: Truyền patrol points từ Large xuống Medium =====
                if (splitLevel == 0) // Large slime split thành Medium
                {
                    newSlimeScript.pointA = this.pointA; // Truyền patrol points
                    newSlimeScript.pointB = this.pointB;
                    newSlimeScript.parentSlime = this; // Lưu reference
                }
                else if (splitLevel == 1) // Medium slime split thành Small
                {
                    // Small slime không cần patrol points
                    newSlimeScript.pointA = null;
                    newSlimeScript.pointB = null;
                    newSlimeScript.parentSlime = this.parentSlime; // Truyền reference của Large gốc
                }
                
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
            currentState = EnemyState.Patrol;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            // Hiển thị current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }
        
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