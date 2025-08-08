using UnityEngine;

public class MiniBoss : MonoBehaviour, IBossResettable
{
    [Header("Boss Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public float stopDistance = 2f;

    [Header("Attack Settings")]
    [Range(0f, 1f)]
    public float attack1Chance = 0.33f;     // Tỷ lệ Attack 1
    [Range(0f, 1f)]
    public float attack2Chance = 0.33f;     // Tỷ lệ Attack 2
    [Range(0f, 1f)]
    public float attack3Chance = 0.34f;     // Tỷ lệ Attack 3

    [Header("Bullet Attack Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;             // Điểm bắn đạn
    public float bulletSpeed = 8f;
    public int bulletsPerShot = 1;          // Số đạn bắn mỗi lần
    public float bulletSpread = 0f;         // Độ tản của đạn

    [Header("Bomb Attack Settings")]
    public GameObject bombPrefab;
    public float bombSpawnHeight = 3f;      // Độ cao spawn bomb so với Player
    public float bombSpawnOffset = 0.5f;    // Offset ngẫu nhiên vị trí spawn

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isHurt = false;
    private bool facingRight = true;

    private Vector2 movement;
    private float distanceToPlayer;
    private float initialFirePointX; // Lưu vị trí X ban đầu của FirePoint

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;
    public HealthBarEnemy healthBarEnemy;
    private MiniBossDamageReceiver damageReceiver;

    [Header("Teleport Skill Settings")]
    public float teleportRange = 8f;            // Khoảng cách dịch chuyển tối đa
    public float teleportFadeTime = 1f;         // Thời gian fade out/in
    public float teleportCooldown = 5f;         // Thời gian hồi skill
    public GameObject teleportBombPrefab;       // Prefab bomb để lại sau khi dịch chuyển

    private bool isTeleporting = false;
    private bool canTeleport = true;
    private float lastTeleportTime;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] allColliders;

    [Header("Map Boundary Settings")]
    public Transform mapBoundaryLeft;      // GameObject hoặc Transform đánh dấu biên trái map
    public Transform mapBoundaryRight;

    [Header("Map Boundary Values (Alternative)")]
    public bool useFixedBoundaries = false;
    public float mapLeftBoundary = -20f;
    public float mapRightBoundary = 20f;

    [Header("Audio Settings")]
    public AudioClip attackSound;
    public AudioClip teleportSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip moveSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    private AudioSource audioSource;
    private BossManager bossManager;

    // Lưu trạng thái ban đầu để reset
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool initialDataSaved = false;

    void Awake()
    {
        damageReceiver = GetComponent<MiniBossDamageReceiver>();
    }

    void Start()
    {
        // Tự động tham chiếu các component
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        // Tìm Player trong scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        float normalizedHealth = GetNormalizedHealth();
        if (healthBarEnemy != null)
        {
            if (normalizedHealth < 1f)
                healthBarEnemy.ShowHealthBar(normalizedHealth);
            else
                healthBarEnemy.HideHealthBar();
        }

        // Kiểm tra firePoint, nếu không có thì tạo một cái mới
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }

        // Lưu vị trí FirePoint ban đầu
        initialFirePointX = firePoint.localPosition.x;

        // Validate tỷ lệ attack
        ValidateAttackChances();

        spriteRenderer = GetComponent<SpriteRenderer>();
        allColliders = GetComponents<Collider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;

        bossManager = FindFirstObjectByType<BossManager>();

        if (!useFixedBoundaries &&
        (mapBoundaryLeft == null || mapBoundaryRight == null))
        {
            AutoDetectMapBoundaries();
        }

        // Lưu trạng thái ban đầu lần đầu tiên
        if (!initialDataSaved)
        {
            SaveInitialState();
        }
    }

    private void SaveInitialState()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        
        // Lưu hướng facing ban đầu dựa trên scale
        if (initialScale.x > 0)
            facingRight = true;
        else
            facingRight = false;
            
        initialDataSaved = true;
        
        Debug.Log($"[MiniBoss] Saved initial state: Position {initialPosition}, Scale {initialScale}, FacingRight {facingRight}");
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Tính khoảng cách đến Player
        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Luôn luôn di chuyển theo Player (không cần khoảng cách)
        if (!isAttacking && !isHurt && !isTeleporting)
        {
            MoveTowardsPlayer();
        }

        // Kiểm tra tấn công
        if (CanAttack() && !isTeleporting)
        {
            PerformRandomAttack();
        }

        // Cập nhật animation
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking || isHurt) return;

        // Di chuyển
        rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);
    }

    void MoveTowardsPlayer()
    {
        // Chỉ di chuyển khi khoảng cách lớn hơn stopDistance
        if (distanceToPlayer > stopDistance)
        {
            // Tính hướng đến Player
            Vector2 direction = (player.position - transform.position).normalized;
            movement.x = direction.x;

            // Flip sprite theo hướng di chuyển
            if (direction.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (direction.x < 0 && facingRight)
            {
                Flip();
            }
        }
        else
        {
            // Dừng lại khi quá gần Player
            movement.x = 0f;

            // Vẫn quay mặt về phía Player
            Vector2 direction = (player.position - transform.position).normalized;
            if (direction.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (direction.x < 0 && facingRight)
            {
                Flip();
            }
        }

        if (distanceToPlayer > stopDistance && moveSound != null && audioSource != null)
        {
            if (!audioSource.isPlaying || audioSource.clip != moveSound)
            {
                audioSource.clip = moveSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            // Stop move sound when not moving
            if (audioSource != null && audioSource.clip == moveSound && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // Không cần di chuyển FirePoint vì nó đã tự động flip theo Boss
        // FirePoint sẽ luôn ở đúng vị trí phía trước Boss
    }

    bool CanAttack()
    {
        return !isAttacking && !isHurt &&
               Time.time - lastAttackTime >= attackCooldown &&
               distanceToPlayer <= attackRange;
    }

    void PerformRandomAttack()
    {
        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= attack1Chance)
        {
            StartAttack1();
        }
        else if (randomValue <= attack1Chance + attack2Chance)
        {
            StartAttack2();
        }
        else
        {
            StartAttack3();
        }

        lastAttackTime = Time.time;
    }

    void StartAttack1()
    {
        isAttacking = true;
        movement = Vector2.zero;
        animator.SetTrigger("Attack1");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    void StartAttack2()
    {
        isAttacking = true;
        movement = Vector2.zero;
        animator.SetTrigger("Attack2");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    void StartAttack3()
    {
        isAttacking = true;
        movement = Vector2.zero;
        animator.SetTrigger("Attack3");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    // Được gọi từ Animation Event trong Attack1
    public void FireBulletAttack1()
    {
        FireBullets();
    }

    // Được gọi từ Animation Event trong Attack2
    public void FireBulletAttack2()
    {
        FireBullets();
    }

    // Được gọi từ Animation Event trong Attack3
    public void SpawnBomb()
    {
        if (bombPrefab != null && player != null)
        {
            // Tính vị trí spawn bomb phía trên Player
            Vector3 spawnPosition = player.position + Vector3.up * bombSpawnHeight;

            // Thêm offset ngẫu nhiên
            spawnPosition.x += Random.Range(-bombSpawnOffset, bombSpawnOffset);

            // Spawn bomb
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void FireBullets()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        // Tính hướng bắn đến Player
        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            // Tính góc tản
            float spreadAngle = 0f;
            if (bulletsPerShot > 1)
            {
                spreadAngle = bulletSpread * (i - (bulletsPerShot - 1) / 2f);
            }

            // Xoay hướng theo góc tản
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) + spreadAngle * Mathf.Deg2Rad;
            Vector2 bulletDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Tạo đạn
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            MiniBossBullet bulletScript = bullet.GetComponent<MiniBossBullet>();

            if (bulletScript != null)
            {
                bulletScript.Initialize(bulletDirection, bulletSpeed);
            }
        }
    }

    // Được gọi từ Animation Event khi kết thúc attack
    public void OnAttackComplete()
    {
        isAttacking = false;
    }

    private System.Collections.IEnumerator TeleportSkill()
    {
        // Kiểm tra lại trạng thái trước khi thực hiện
        if (isDead || currentState == State.Dead || GetNormalizedHealth() <= 0.05f)
        {
            yield break;
        }

        isTeleporting = true;
        canTeleport = false;
        lastTeleportTime = Time.time;

        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // Lưu vị trí hiện tại để spawn bomb
        Vector3 originalPosition = transform.position;

        // Dừng mọi chuyển động
        rb.linearVelocity = Vector2.zero;
        movement = Vector2.zero;
        rb.gravityScale = 0;

        // Tắt colliders khi invisible
        SetCollidersEnabled(false);

        // Fade out (giảm opacity từ 100% về 0%)
        yield return StartCoroutine(FadeOut());

        // Kiểm tra lại trạng thái sau khi fade out
        if (isDead || currentState == State.Dead)
        {
            yield break;
        }

        // Spawn bomb tại vị trí cũ
        if (teleportBombPrefab != null)
        {
            GameObject bomb = Instantiate(teleportBombPrefab, originalPosition, Quaternion.identity);

            // Kích hoạt nổ sau 2 giây
            StartCoroutine(ExplodeBombAfterDelay(bomb, 1f));
        }

        // Tính toán vị trí mới
        Vector3 newPosition = CalculateNewTeleportPosition();

        // Dịch chuyển đến vị trí mới
        transform.position = newPosition;

        // Fade in (tăng opacity từ 0% về 100%)
        yield return StartCoroutine(FadeIn());

        // Bật lại colliders
        SetCollidersEnabled(true);

        isTeleporting = false;

        // Bắt đầu cooldown
        StartCoroutine(TeleportCooldown());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < teleportFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / teleportFadeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Đảm bảo alpha = 0
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < teleportFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / teleportFadeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Đảm bảo alpha = 1
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    private Vector3 CalculateNewTeleportPosition()
    {
        Vector3 currentPos = transform.position;
        Vector3 newPos = currentPos;

        // Lấy thông tin boundary của map (chỉ trục X)
        float leftBound, rightBound;

        if (useFixedBoundaries)
        {
            leftBound = mapLeftBoundary;
            rightBound = mapRightBoundary;
        }
        else
        {
            // Sử dụng Transform references
            leftBound = mapBoundaryLeft != null ? mapBoundaryLeft.position.x : currentPos.x - teleportRange;
            rightBound = mapBoundaryRight != null ? mapBoundaryRight.position.x : currentPos.x + teleportRange;
        }

        // Thử tìm vị trí mới trong vòng 30 lần
        for (int i = 0; i < 30; i++)
        {
            // Random vị trí theo trục X trong phạm vi map, giữ nguyên Y
            float randomX = Random.Range(leftBound + 1f, rightBound - 1f); // +1f và -1f để tránh spawn sát biên

            newPos = new Vector3(randomX, currentPos.y, currentPos.z);

            // Kiểm tra khoảng cách đến Player (tối thiểu 8f)
            float distanceToPlayer = Vector2.Distance(newPos, player.position);
            if (distanceToPlayer < 8f)
            {
                continue; // Thử lại nếu quá gần Player
            }

            // Kiểm tra vị trí có hợp lệ không (không va chạm với wall/ground)
            Collider2D hit = Physics2D.OverlapCircle(newPos, 0.5f, LayerMask.GetMask("Ground", "Wall"));
            if (hit == null)
            {
                return newPos;
            }
        }

        // Nếu không tìm được vị trí hợp lệ, tìm vị trí an toàn theo trục X
        Vector2 directionFromPlayer = (currentPos - player.position).normalized;

        // Thử 2 hướng: trái và phải
        float[] xOffsets = { 8f, -8f }; // Khoảng cách tối thiểu từ Player

        foreach (float offset in xOffsets)
        {
            float candidateX = player.position.x + offset;

            // Kiểm tra xem vị trí có trong map không
            if (candidateX >= leftBound + 1f && candidateX <= rightBound - 1f)
            {
                Vector3 candidatePos = new Vector3(candidateX, currentPos.y, currentPos.z);

                // Kiểm tra va chạm
                Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.5f, LayerMask.GetMask("Ground", "Wall"));
                if (hit == null)
                {
                    return candidatePos;
                }
            }
        }
        return currentPos;
    }

    private void AutoDetectMapBoundaries()
    {
        // Tìm các GameObject có tag "MapBoundary" hoặc tương tự
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("MapBoundary");

        if (boundaries.Length >= 2)
        {
            // Sắp xếp theo vị trí X để xác định left và right
            System.Array.Sort(boundaries, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            mapBoundaryLeft = boundaries[0].transform;
            mapBoundaryRight = boundaries[boundaries.Length - 1].transform;
        }
        else
        {
            useFixedBoundaries = true;
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider2D col in allColliders)
        {
            if (col != null)
            {
                col.enabled = enabled;
            }
        }
    }

    private System.Collections.IEnumerator ExplodeBombAfterDelay(GameObject bomb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bomb != null)
        {
            // Kích hoạt animation nổ
            Animator bombAnimator = bomb.GetComponent<Animator>();
            if (bombAnimator != null)
            {
                bombAnimator.SetTrigger("Explode");
            }

            // Gây damage cho Player nếu trong vùng nổ
            float explosionRadius = 3f; // Bán kính nổ
            Collider2D playerCollider = Physics2D.OverlapCircle(bomb.transform.position, explosionRadius, LayerMask.GetMask("TransparentFX"));

            if (playerCollider != null && playerCollider.CompareTag("Player"))
            {
                CharacterController2D playerController = playerCollider.GetComponent<CharacterController2D>();
                if (playerController != null)
                {
                    playerController.ApplyDamage(5f, bomb.transform.position); // 20 damage
                }
            }

            // Hủy bomb sau khi nổ
            Destroy(bomb, 1f);
        }
    }

    private System.Collections.IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

     public void ResetBoss()
    {
        // Dừng tất cả coroutines
        StopAllCoroutines();

        // Reset trạng thái
        isDead = false;
        isAttacking = false;
        isHurt = false;
        isTeleporting = false;
        canTeleport = true;
        currentState = State.Patrolling;
        lastAttackTime = 0f;
        lastTeleportTime = 0f;

        // Reset vị trí và scale
        if (initialDataSaved)
        {
            transform.position = initialPosition;
            // QUAN TRỌNG: Reset scale về giá trị ban đầu để sửa lỗi đi ngược
            transform.localScale = initialScale;
            
            // Reset hướng facing về đúng hướng ban đầu
            if (initialScale.x > 0)
                facingRight = true;
            else
                facingRight = false;
        }

        // Reset physics
        // rb.linearVelocity = Vector2.zero;
        // rb.gravityScale = 1f; // Reset gravity
        // rb.bodyType = RigidbodyType2D.Dynamic; // Đảm bảo Dynamic khi reset

        // Bật lại colliders và script
        SetCollidersEnabled(true);
        this.enabled = true;

        // Reset sprite renderer (trong trường hợp đang teleport)
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        // Reset animator
        if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Death");
            animator.SetBool("IsRunning", false);
        }

        // Reset máu
        if (damageReceiver != null)
        {
            damageReceiver.ResetBossHealth();
        }

        // Reset health bar
        if (healthBarEnemy != null)
        {
            healthBarEnemy.ShowHealthBar(1f);
        }

        // Reset audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Tìm lại player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Reset movement để tránh lỗi di chuyển
        movement = Vector2.zero;

        Debug.Log($"[MiniBoss] {gameObject.name} has been reset to initial state at position {transform.position} with scale {transform.localScale}");
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        if (healthBarEnemy != null)
        {
            healthBarEnemy.ShowHealthBar(GetNormalizedHealth());
        }

        if (canTeleport && !isTeleporting && GetNormalizedHealth() > 0.05f)
        {
            StartCoroutine(TeleportSkill());
        }
    }

    public void OnDead()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        animator.SetTrigger("Death");
        currentState = State.Dead;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        if (bossManager != null)
        {
            bossManager.ReportBossDeath(this.gameObject);
        }

        healthBarEnemy?.HideHealthBar();

        Destroy(gameObject, 2f);
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }

    // Được gọi từ Animation Event khi kết thúc Death animation
    public void OnDeathComplete()
    {
        Destroy(gameObject);
    }

    void UpdateAnimation()
    {
        if (isDead || isAttacking || isHurt) return;

        // Set animation parameters
        animator.SetBool("IsRunning", Mathf.Abs(movement.x) > 0.1f);
    }

    void ValidateAttackChances()
    {
        float total = attack1Chance + attack2Chance + attack3Chance;
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            // Tự động normalize
            attack1Chance /= total;
            attack2Chance /= total;
            attack3Chance /= total;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ phạm vi dừng lại
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Vẽ phạm vi phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ firePoint
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(firePoint.position, Vector3.one * 0.2f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(teleportRange * 2f, 1f, 1f));

        if (useFixedBoundaries)
        {
            Gizmos.color = Color.cyan;

            // Vẽ 2 đường thẳng đứng cho left và right boundary
            Vector3 leftLine = new Vector3(mapLeftBoundary, transform.position.y - 5f, 0f);
            Vector3 leftLineEnd = new Vector3(mapLeftBoundary, transform.position.y + 5f, 0f);
            Gizmos.DrawLine(leftLine, leftLineEnd);

            Vector3 rightLine = new Vector3(mapRightBoundary, transform.position.y - 5f, 0f);
            Vector3 rightLineEnd = new Vector3(mapRightBoundary, transform.position.y + 5f, 0f);
            Gizmos.DrawLine(rightLine, rightLineEnd);

            // Vẽ vùng teleport có thể
            Gizmos.color = Color.cyan;
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Vector3 center = new Vector3((mapLeftBoundary + mapRightBoundary) / 2f, transform.position.y, 0f);
            Vector3 size = new Vector3(mapRightBoundary - mapLeftBoundary, 2f, 1f);
            Gizmos.DrawCube(center, size);
        }
        else if (mapBoundaryLeft != null && mapBoundaryRight != null)
        {
            Gizmos.color = Color.cyan;

            // Vẽ 2 đường thẳng đứng cho left và right boundary
            Vector3 leftLine = new Vector3(mapBoundaryLeft.position.x, transform.position.y - 5f, 0f);
            Vector3 leftLineEnd = new Vector3(mapBoundaryLeft.position.x, transform.position.y + 5f, 0f);
            Gizmos.DrawLine(leftLine, leftLineEnd);

            Vector3 rightLine = new Vector3(mapBoundaryRight.position.x, transform.position.y - 5f, 0f);
            Vector3 rightLineEnd = new Vector3(mapBoundaryRight.position.x, transform.position.y + 5f, 0f);
            Gizmos.DrawLine(rightLine, rightLineEnd);

            // Vẽ vùng teleport có thể
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Vector3 center = new Vector3((mapBoundaryLeft.position.x + mapBoundaryRight.position.x) / 2f, transform.position.y, 0f);
            Vector3 size = new Vector3(mapBoundaryRight.position.x - mapBoundaryLeft.position.x, 2f, 1f);
            Gizmos.DrawCube(center, size);
        }
    }

    #region IBossResettable Implementation
    public bool IsActive()
    {
        return gameObject.activeInHierarchy && enabled && !isDead;
    }

    public string GetBossName()
    {
        return gameObject.name;
    }
    #endregion
}