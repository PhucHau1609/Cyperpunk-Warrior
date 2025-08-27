using UnityEngine;
using BehaviorDesigner.Runtime;

public class BossPhuController : MonoBehaviour, IDamageResponder, IBossResettable
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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip meleeAttackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip phaseChangeSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float soundVolume = 0.7f;
    [Range(0.5f, 2f)] public float soundPitch = 1f;
    private bool isRunning = false;
    private bool runningAudioPlaying = false;

    public BossPhuHealthBar healthBar;
    private State currentState = State.Idle;
    public BossPhuDamageReceiver damageReceiver;
    private BossManager bossManager;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool initialDataSaved = false;

    void OnEnable() { PlayerLocator.OnChanged += HandlePlayerChanged; TryResolvePlayer(); }
    void OnDisable() { PlayerLocator.OnChanged -= HandlePlayerChanged; }
    void HandlePlayerChanged(Transform t) { player = t; }
    void TryResolvePlayer() { if (PlayerLocator.Current) player = PlayerLocator.Current; else { var go = GameObject.FindWithTag("Player"); if (go) player = go.transform; } }


    void Awake()
    {
        // Khởi tạo components ngay trong Awake để đảm bảo luôn có sẵn
        InitializeComponents();
        
        damageReceiver = GetComponent<BossPhuDamageReceiver>();

        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        SetupAudioSource();
    }
    
    // Method để khởi tạo components
    private void InitializeComponents()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();
    }

    void SetupAudioSource()
    {
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
            audioSource.pitch = soundPitch;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.7f; // 3D sound
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 20f;
        }
    }
    void Start()
    {
        InitializeBoss();

        float normalizedHealth = GetNormalizedHealth();
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(normalizedHealth);
        }
        bossManager = FindFirstObjectByType<BossManager>();

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

    void InitializeBoss()
    {
        // Đảm bảo components được khởi tạo
        InitializeComponents();

        damageReceiver = GetComponent<BossPhuDamageReceiver>();

        // Find Player immediately when scene starts
        FindPlayer();

        // Freeze rotation
        if (rb != null)
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
            playerDetected = true;
        }
    }

    void Update()
    {
        // Liên tục tìm Player nếu chưa có (vì Player là DontDestroyOnLoad)
        if (player == null || !playerDetected)
        {
            FindPlayer();
        }

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

        CheckRunningState();

        // Check phase change
        CheckPhaseChange();
        CheckDeathState();
    }

    void CheckRunningState()
    {
        // Kiểm tra nếu Boss đang chạy dựa trên velocity
        if (rb != null)
        {
            bool wasRunning = isRunning;
            isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isAttacking && !isDead;

            // Xử lý âm thanh chạy
            if (isRunning && !runningAudioPlaying)
            {
                PlayRunningSound();
            }
            else if (!isRunning && runningAudioPlaying)
            {
                StopRunningSound();
            }
        }
    }

    void CheckPhaseChange()
    {
        if (damageReceiver != null && damageReceiver.CurrentHP <= damageReceiver.MaxHP * 0.5f && !hasChangedPhase)
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
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("PhaseChange");
        }

        PlaySound(phaseChangeSound);

        Invoke("EndPhaseChange", 2f);
    }

    void EndPhaseChange()
    {
        isPhase2 = true;
        isAttacking = false;
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
            PlaySound(shootSound);

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
        }
    }

    public void PlayMeleeAttackSound()
    {
        PlaySound(meleeAttackSound);
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
        if (healthBar != null && damageReceiver != null)
        {
            healthBar.ShowHealthBar(damageReceiver.CurrentHP);
        }

        // Xử lý khi bị thương (nếu chưa chết)
        if (damageReceiver != null && !damageReceiver.IsDead())
        {
            OnHurt();
        }
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        if (animator != null)
            animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        PlaySound(hurtSound);

        if (healthBar != null)
        {
            healthBar.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        StopAllSounds();

        // Phát âm thanh chết
        PlaySound(deathSound);

        if (animator != null)
            animator.SetTrigger("Death");
        currentState = State.Dead;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            rb.angularVelocity = 0f;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        this.enabled = false;

        if (bossManager != null)
        {
            bossManager.ReportBossDeath(this.gameObject);
        }

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

    public void ResetBoss()
    {
        Debug.Log($"[BossPhuController] Starting reset for {gameObject.name}");
        
        try
        {
            // Đảm bảo components được khởi tạo trước khi sử dụng
            InitializeComponents();
            
            // Reset trạng thái
            isDead = false;
            isPhase2 = false;
            hasChangedPhase = false;
            isAttacking = false;
            playerDetected = false;
            currentState = State.Idle;
            lastAttackTime = 0f;
            
            // Reset vị trí và scale
            if (initialDataSaved)
            {
                transform.position = initialPosition;
                transform.localScale = initialScale;
            }
            
            // Reset physics với null checks
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.freezeRotation = true;
            }
            else
            {
                Debug.LogWarning($"[BossPhuController] Rigidbody2D is null for {gameObject.name}");
            }
            
            // Bật lại collider và script
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
            this.enabled = true;
            
            // Reset animator với null checks
            if (animator != null)
            {
                animator.ResetTrigger("Death");
                animator.ResetTrigger("Hurt");
                animator.ResetTrigger("PhaseChange");
                animator.SetBool("IsRunning", false);
            }
            else
            {
                Debug.LogWarning($"[BossPhuController] Animator is null for {gameObject.name}");
            }
            
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
            
            // Reset behavior tree với null checks
            if (behaviorTree != null)
            {
                behaviorTree.EnableBehavior();
                behaviorTree.RestartWhenComplete = true;
            }
            else
            {
                // Thử tìm lại nếu null
                behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
                if (behaviorTree != null)
                {
                    behaviorTree.EnableBehavior();
                    behaviorTree.RestartWhenComplete = true;
                }
            }
            
            // Reset audio
            StopAllSounds();
            
            // Tìm lại player
            FindPlayer();
            
            Debug.Log($"[BossPhuController] Reset completed for {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BossPhuController] Error during reset for {gameObject.name}: {e.Message}");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.volume = soundVolume;
            audioSource.pitch = soundPitch;
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayRunningSound()
    {
        if (audioSource != null && runningSound != null && !runningAudioPlaying)
        {
            audioSource.volume = soundVolume * 0.6f; // Giảm âm lượng cho âm thanh chạy
            audioSource.pitch = soundPitch;
            audioSource.clip = runningSound;
            audioSource.loop = true;
            audioSource.Play();
            runningAudioPlaying = true;
        }
    }

    private void StopRunningSound()
    {
        if (audioSource != null && runningAudioPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            runningAudioPlaying = false;
        }
    }

    private void StopAllSounds()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            runningAudioPlaying = false;
        }
    }
    
     public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
        }
    }

    public void SetSoundPitch(float pitch)
    {
        soundPitch = Mathf.Clamp(pitch, 0.5f, 2f);
        if (audioSource != null)
        {
            audioSource.pitch = soundPitch;
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