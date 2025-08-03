using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using static EnemyMini;

public class Boss2Controller : MonoBehaviour, IBossResettable
{
    [Header("Boss2 Attack Settings")]
    public Transform laserPoint;
    public Transform[] bombPoints;
    public GameObject laserPrefab;
    public GameObject bombPrefab;
    public float attackCooldown = 3f;
    public float laserDuration = 3f;
    public float laserSweepSpeed = 90f;
    public int bombCount = 3;

    [Header("State")]
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public float lastAttackTime = -999f; // Cho phép attack ngay từ đầu
    [HideInInspector] public Transform player;

    private Animator animator;
    private BehaviorTree behaviorTree;
    private Boss2AttackType currentAttackType;

    public Boss2HealthBar healthBar;
    private State currentState = State.Idle;
    public Boss2DamageReceiver damageReceiver;
    
    // Hand Management
    private List<Boss2HandController> hands = new List<Boss2HandController>();
    private Boss2HandController attackingHand = null;

    [Header("Shield System")]
    public GameObject shield;
    private bool shieldActive = false;

    [Header("Fire Area Settings")]
    public GameObject fireAreaPrefab;

    [Header("Phase 2 Settings")]
    public bool isPhase2 = false;
    public Transform[] spawnPoints; // Điểm spawn đệ tử
    public GameObject minionPrefab; // Prefab đệ tử
    public int maxMinions = 3; // Số lượng đệ tử tối đa
    public float minionSpawnInterval = 2f; // Khoảng cách spawn đệ tử
    private List<GameObject> activeMinions = new List<GameObject>();
    private float lastMinionSpawnTime = 0f;

    [Header("Phase 2 Laser Settings")]
    public Transform[] phase2LaserPoints; // 3 điểm laser: 8h, 6h, 4h
    public float laserSequenceDelay = 0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip laserChargeSound;
    public AudioClip laserFireSound;
    public AudioClip bombThrowSound;
    public AudioClip handAttackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip shieldActivateSound;
    public AudioClip phase2TransitionSound;
    
    // Lưu trạng thái ban đầu để reset
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool initialDataSaved = false;

    private void Awake()
    {
        damageReceiver = GetComponent<Boss2DamageReceiver>();
        animator = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();
        FindPlayer();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void Start()
    {
        float normalizedHealth = GetNormalizedHealth();
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(normalizedHealth);
        }

        // Cho phép attack ngay khi scene load
        lastAttackTime = -attackCooldown;
        
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
        initialDataSaved = true;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        CheckDeathState();
        
        // Phase 2: Spawn đệ tử
        if (isPhase2 && Time.time - lastMinionSpawnTime >= minionSpawnInterval)
        {
            SpawnMinion();
        }
    }

    private void SpawnMinion()
    {
        // Xóa đệ tử đã chết khỏi list
        activeMinions.RemoveAll(minion => minion == null);
        
        // Nếu chưa đủ số lượng đệ tử tối đa
        if (activeMinions.Count < maxMinions && spawnPoints.Length > 0)
        {
            // Chọn random một spawn point
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];
            
            // Spawn đệ tử
            GameObject minion = Instantiate(minionPrefab, spawnPoint.position, Quaternion.identity);
            activeMinions.Add(minion);
            
            lastMinionSpawnTime = Time.time;
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown &&
            !isAttacking &&
            attackingHand == null;
    }

    public void ActivateShield()
    {
        PlaySound(shieldActivateSound, 0.9f);

        shieldActive = true;
        if (shield != null)
        {
            shield.SetActive(true);
        }
    }

    public void DeactivateShield()
    {
        shieldActive = false;
        if (shield != null)
        {
            shield.SetActive(false);
        }
        
        // Chuyển sang Phase 2 khi shield bị phá
        if (!isPhase2)
        {
            isPhase2 = true;
            
            PlaySound(phase2TransitionSound, 1f);

            StartMinionSpawning();
        }
    }

    public void StartMinionSpawning()
    {
        lastMinionSpawnTime = Time.time;
    }

    // Thêm method này vào Boss2Controller
    public bool IsShieldActive()
    {
        return shieldActive;
    }
    
    // Method để reset Boss2 về trạng thái ban đầu
    public void ResetBoss()
    {
        // Dừng tất cả coroutines
        StopAllCoroutines();
        
        // Reset trạng thái cơ bản
        isAttacking = false;
        isPhase2 = false;
        shieldActive = false;
        currentState = State.Idle;
        lastAttackTime = -attackCooldown; // Cho phép attack ngay
        lastMinionSpawnTime = 0f;
        attackingHand = null;
        
        // Reset vị trí và scale
        if (initialDataSaved)
        {
            transform.position = initialPosition;
            transform.localScale = initialScale;
        }
        
        // Bật lại collider và script
        GetComponent<Collider2D>().enabled = true;
        this.enabled = true;
        
        // Reset animator
        animator.ResetTrigger("LaserAttack");
        animator.ResetTrigger("BombAttack");
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Death");
        
        // Reset máu
        if (damageReceiver != null)
        {
            damageReceiver.ResetBossHealth();
        }
        
        // Reset health bar
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(1f);
        }
        
        // Reset shield
        if (shield != null)
        {
            shield.SetActive(false);
        }
        
        // Reset behavior tree
        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) 
        {
            behavior.EnableBehavior();
            behavior.RestartWhenComplete = true;
        }

        foreach (var hand in hands)
        {
            if (hand != null)
            {
                hand.ResetHand(); // Gọi method reset của hand
            }
        }
        
        // Xóa tất cả minions hiện tại
        foreach (var minion in activeMinions)
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }
        activeMinions.Clear();
        
        // Reset audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Tìm lại player
        FindPlayer();
    }
    
    // Hand Management Methods
    public void RegisterHand(Boss2HandController hand)
    {
        if (!hands.Contains(hand))
        {
            hands.Add(hand);
        }
    }
    
     public void OnHandStartAttack(Boss2HandController hand)
    {
        // Method này giờ chỉ để log, attackingHand đã được set trong ExecuteHandAttack
    }
    
    private void ExecuteHandAttack(Boss2AttackType attackType)
    {
        if (player == null || hands.Count == 0)
        {
            // Nếu không có player hoặc cánh tay, kết thúc attack
            EndAttack();
            return;
        }

        // Tìm cánh tay gần player nhất và còn sống
        Boss2HandController closestHand = null;
        float closestDistance = float.MaxValue;

        foreach (var hand in hands)
        {
            if (hand != null && !hand.IsDead()) // Thêm kiểm tra cánh tay còn sống
            {
                float distance = Vector2.Distance(hand.transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHand = hand;
                }
            }
        }

        if (closestHand != null)
        {
            PlaySound(handAttackSound, 0.7f);

            closestHand.StartHandAttack(attackType, player.position);
            attackingHand = closestHand;
        }
        else
        {
            EndAttack();
        }
    }
    
    public void OnHandEndAttack(Boss2HandController hand)
    {
        if (attackingHand == hand)
        {
            attackingHand = null;
            
            // Kiểm tra nếu shield đang active và cả 2 cánh tay đều chết
            if (shieldActive && AreAllHandsDead())
            {
                DeactivateShield();
            }
        }
    }

    private bool AreAllHandsDead()
    {
        int deadCount = 0;
        int totalHands = 0;
        
        foreach (var hand in hands)
        {
            if (hand != null)
            {
                totalHands++;
                if (hand.IsDead())
                {
                    deadCount++;
                }
            }
        }
        
        return deadCount >= totalHands && totalHands > 0;
    }

    public void CheckHandsStatus()
    {
        if (shieldActive && AreAllHandsDead())
        {
            DeactivateShield();
        }
    }
    
    public bool IsAnyHandOrBossAttacking()
    {
        return isAttacking || attackingHand != null;
    }
    
    public Boss2HandController GetOtherHand(Boss2HandController currentHand)
    {
        foreach (var hand in hands)
        {
            if (hand != currentHand)
                return hand;
        }
        return null;
    }

    public void StartAttack()
    {
        // Chỉ cho phép Boss2 tấn công khi không có cánh tay nào đang tấn công
        if (attackingHand != null) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;

        Boss2AttackType selectedAttack;
        
        // Khi Shield active, ưu tiên tấn công Hand (Attack3/Attack4)
        if (shieldActive)
        {
            // Ưu tiên tấn công Hand 70%, Boss attack 30%
            int[] weights = {40, 20, 20, 20}; // Laser(10%), Bomb(10%), Attack3(40%), Attack4(40%)
            int totalWeight = 100;
            int randomValue = Random.Range(0, totalWeight);
            
            if (randomValue < weights[0]) // 0-9: Laser
            {
                selectedAttack = Boss2AttackType.Laser;
            }
            else if (randomValue < weights[0] + weights[1]) // 10-19: Bomb
            {
                selectedAttack = Boss2AttackType.Bomb;
            }
            else if (randomValue < weights[0] + weights[1] + weights[2]) // 20-59: Attack3
            {
                selectedAttack = Boss2AttackType.Attack3;
            }
            else // 60-99: Attack4
            {
                selectedAttack = Boss2AttackType.Attack4;
            }
        }
        else
        {
            // Logic cũ khi Shield không active
            int[] weights = {40, 20, 20, 20}; // Laser(40%), Bomb(20%), Attack3(20%), Attack4(20%)
            int totalWeight = 100;
            int randomValue = Random.Range(0, totalWeight);
            
            if (randomValue < weights[0]) // 0-39: Laser
            {
                selectedAttack = Boss2AttackType.Laser;
            }
            else if (randomValue < weights[0] + weights[1]) // 40-59: Bomb
            {
                selectedAttack = Boss2AttackType.Bomb;
            }
            else if (randomValue < weights[0] + weights[1] + weights[2]) // 60-79: Attack3
            {
                selectedAttack = Boss2AttackType.Attack3;
            }
            else // 80-99: Attack4
            {
                selectedAttack = Boss2AttackType.Attack4;
            }
        }
        
        currentAttackType = selectedAttack;

        if (animator != null && (selectedAttack == Boss2AttackType.Laser || selectedAttack == Boss2AttackType.Bomb))
        {
            if (selectedAttack == Boss2AttackType.Laser)
            {
                animator.SetTrigger("LaserAttack");
                CameraFollow.Instance?.ShakeCamera();
            }
            else if (selectedAttack == Boss2AttackType.Bomb)
            {
                animator.SetTrigger("BombAttack");
                CameraFollow.Instance?.ShakeCamera();
            }
        }
        else if (selectedAttack == Boss2AttackType.Attack3 || selectedAttack == Boss2AttackType.Attack4)
        {
            // Tìm cánh tay gần player nhất để tấn công
            ExecuteHandAttack(selectedAttack);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    // Animation Event
    public void OnLaserAttackTrigger()
    {
        ExecuteLaserAttack();
    }

    public void OnBombAttackTrigger()
    {
        ExecuteBombAttack();
    }

    public void OnAttackAnimationEnd()
    {
        EndAttack();
    }

    private void ExecuteLaserAttack()
    {
        if (laserPrefab == null || laserPoint == null) return;

        PlaySound(laserChargeSound, 0.8f);

        if (isPhase2 && phase2LaserPoints != null && phase2LaserPoints.Length >= 3)
        {
            // Phase 2: Bắn 3 tia laser lần lượt
            StartCoroutine(ExecutePhase2LaserSequence());
        }
        else
        {
            // Phase 1: Laser quét bình thường
            Instantiate(laserPrefab, laserPoint.position, laserPoint.rotation);
        }
    }
    
    private System.Collections.IEnumerator ExecutePhase2LaserSequence()
    {
        // Bắn laser tại 3 vị trí: 8h, 6h, 4h
        for (int i = 0; i < phase2LaserPoints.Length; i++)
        {
            if (phase2LaserPoints[i] != null)
            {
                Instantiate(laserPrefab, phase2LaserPoints[i].position, phase2LaserPoints[i].rotation);
                yield return new WaitForSeconds(laserSequenceDelay);
            }
        }
    }

    private void ExecuteBombAttack()
    {
        if (bombPrefab == null || bombPoints == null) return;

        PlaySound(bombThrowSound, 0.9f);

        int spawnCount = Mathf.Min(bombCount, bombPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            if (bombPoints[i] != null)
            {
                GameObject bomb = Instantiate(bombPrefab, bombPoints[i].position, Quaternion.identity);

                // Truyền fire area prefab cho bomb
                Boss2Bomb bombScript = bomb.GetComponent<Boss2Bomb>();
                if (bombScript != null && fireAreaPrefab != null)
                {
                    bombScript.fireAreaPrefab = fireAreaPrefab;
                }
            }
        }
    }

    public void ApplyDamageToPlayer(float damage, Vector3 attackPosition)
    {
        if (player != null)
        {
            var playerScript = player.GetComponent<MonoBehaviour>();
            if (playerScript != null)
            {
                var applyDamageMethod = playerScript.GetType().GetMethod("ApplyDamage", 
                    new System.Type[] { typeof(float), typeof(Vector3) });
                
                if (applyDamageMethod != null)
                {
                    applyDamageMethod.Invoke(playerScript, new object[] { damage, attackPosition });
                }
            }
        }
    }

    void CheckDeathState()
    {
        if (damageReceiver != null && damageReceiver.IsDead() && currentState != State.Dead)
        {
            OnDead();
        }
    }

    public void StartPhaseChange()
    {
      
    }

    void EndPhaseChange()
    {
        
    }

    private enum State
    {
        Idle,
        Attacking,
        Hurt,
        Dead
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }
    
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

        PlaySound(hurtSound, 0.8f);

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        if (healthBar != null)
        {
            healthBar.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        PlaySound(deathSound, 1f);

        animator.SetTrigger("Death");
        currentState = State.Dead;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        healthBar?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();

        // Buộc tất cả cánh tay dừng tấn công
        foreach (var hand in hands)
        {
            if (hand != null)
            {
                hand.ForceEndAttack();
            }
        }
        KillAllEnemies();
        Destroy(gameObject, 2f);
    }

    private void KillAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            // Lấy animator
            Animator enemyAnimator = enemy.GetComponent<Animator>();
            
            // Kiểm tra và disable các script enemy
            SlimeEnemy slime = enemy.GetComponent<SlimeEnemy>();
            if (slime != null)
            {
                slime.enabled = false;
            }

            FlyingDroidController flyingDroid = enemy.GetComponent<FlyingDroidController>();
            if (flyingDroid != null)
            {
                flyingDroid.enabled = false;
                flyingDroid.rb.gravityScale = 1;
                flyingDroid.healthBarEnemy.HideHealthBar();
            }

            // Trigger death animation nếu có
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Death");
            }

            // Destroy enemy sau 2 giây
            Destroy(enemy, 2f);
        }
    }

    #region IBossResettable Implementation
    public bool IsActive()
    {
        return gameObject.activeInHierarchy && enabled;
    }
    
    public string GetBossName()
    {
        return gameObject.name;
    }
    #endregion
}

public enum Boss2AttackType
{
    Laser,
    Bomb,
    Attack3,
    Attack4
}