using UnityEngine;

public class Boss2HandController : MonoBehaviour
{
    [Header("Hand Settings")]
    public bool isLeftHand = true; // True nếu là cánh tay trái
    public float moveSpeed = 5f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float returnSpeed = 8f;
    
    [Header("Attack Settings")]
    public float attackDelay = 0.5f;
    public float attackDuration = 1.5f;
    
    private Boss2Controller boss2Controller;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    
    private Vector3 originalPosition;
    private Vector3 targetPosition; // Vị trí Player khi bắt đầu tấn công
    private Boss2AttackType assignedAttackType;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;
    private float attackStartTime;
    private bool hasExecutedAttack = false;
    private HandDamageReceiver damageReceiver;

    [Header("Hit Box")]
    public Boss2HandHitBox hitBox;

    [Header("Reset Settings")]
    private Vector3 initialPosition;
    private bool initialDataSaved = false;


    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip handMoveSound;
    public AudioClip handAttackSound;
    public AudioClip handHurtSound;
    public AudioClip handDeathSound;
    
    public enum HandState
    {
        Idle,
        MovingToTarget,
        Attacking,
        Returning
    }
    
    public HandState currentState = HandState.Idle;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        damageReceiver = GetComponent<HandDamageReceiver>();
        originalPosition = transform.position;
        
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

    void Start()
    {
        // Tìm Boss2Controller
        boss2Controller = FindFirstObjectByType<Boss2Controller>();

        // Tìm Player
        FindPlayer();

        // Đăng ký cánh tay với Boss2
        if (boss2Controller != null)
        {
            boss2Controller.RegisterHand(this);
        }
        
        if (!initialDataSaved)
        {
            SaveInitialState();
        }
    }
    
    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }
        
        // Xử lý state - Hand không tự động tấn công nữa
        HandleCurrentState();
    }

    private void SaveInitialState()
    {
        initialPosition = transform.position;
        initialDataSaved = true;
    }

    public void ResetHand()
    {
        // Dừng tất cả coroutines nếu có
        StopAllCoroutines();
        
        // Reset trạng thái
        isAttacking = false;
        currentState = HandState.Idle;
        lastAttackTime = -999f;
        hasExecutedAttack = false;
        assignedAttackType = Boss2AttackType.Laser; // Default value
        
        // Reset vị trí
        if (initialDataSaved)
        {
            transform.position = initialPosition;
        }
        else
        {
            transform.position = originalPosition;
        }
        
        // Reset physics
        rb.linearVelocity = Vector2.zero;
        
        // Reset animator
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("Attack4");
        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Death");
        
        // Reset collider và enable script
        GetComponent<Collider2D>().enabled = true;
        this.enabled = true;
        
        // Reset damage receiver (máu)
        if (damageReceiver != null)
        {
            damageReceiver.ResetHandHealth();
        }
        
        // Disable hitbox nếu đang active
        DisableHitBox();
        
        // Reset audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
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
    

    
    // Method được gọi từ Boss2Controller
    public void StartHandAttack(Boss2AttackType attackType, Vector3 playerPosition)
    {
        if (isAttacking || IsDead() || currentState != HandState.Idle) 
        {
            return;
        }

        PlaySound(handMoveSound, 0.8f);
        
        isAttacking = true;
        assignedAttackType = attackType;
        targetPosition = playerPosition; // Lưu vị trí Player hiện tại
        lastAttackTime = Time.time;
        attackStartTime = Time.time;
        hasExecutedAttack = false;
        currentState = HandState.MovingToTarget;
        
        // Thông báo cho Boss2
        if (boss2Controller != null)
        {
            boss2Controller.OnHandStartAttack(this);
        }
    }
    
    void HandleCurrentState()
    {
        switch (currentState)
        {
            case HandState.MovingToTarget:
                MoveToTarget();
                break;
            case HandState.Attacking:
                HandleAttacking();
                break;
            case HandState.Returning:
                ReturnToOriginalPosition();
                break;
        }
    }
    
    void MoveToTarget()
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        
        // Khi đến gần target position, chuyển sang trạng thái tấn công
        if (Vector2.Distance(transform.position, targetPosition) <= 1.5f)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = HandState.Attacking;
            
            // Chạy animation dựa trên assignedAttackType
            string attackAnimation = assignedAttackType == Boss2AttackType.Attack3 ? "Attack3" : "Attack4";
            animator.SetTrigger(attackAnimation);
        }
    }
    
    void HandleAttacking()
    {
        float elapsedTime = Time.time - attackStartTime;
        
        // Thực hiện enable hitbox sau attackDelay
        if (!hasExecutedAttack && elapsedTime >= attackDelay)
        {
            ExecuteAttackDamage();
            hasExecutedAttack = true;
        }
        
        // Kết thúc tấn công sau attackDuration
        if (elapsedTime >= attackDuration)
        {
            DisableHitBox(); // Tắt HitBox khi kết thúc attack
            currentState = HandState.Returning;
        }
    }
    
    void ExecuteAttackDamage()
    {
        if (hitBox != null)
        {
            PlaySound(handAttackSound, 0.9f);

            hitBox.EnableHitBox();
        }
    }

    void DisableHitBox()
    {
        if (hitBox != null)
        {
            hitBox.DisableHitBox();
        }
    }
    
    void ReturnToOriginalPosition()
    {
        Vector2 direction = (originalPosition - transform.position).normalized;
        rb.linearVelocity = direction * returnSpeed;
        
        // Khi về đến vị trí ban đầu
        if (Vector2.Distance(transform.position, originalPosition) <= 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = originalPosition;
            
            // Reset trạng thái
            currentState = HandState.Idle;
            isAttacking = false;
            
            // Thông báo cho Boss2
            if (boss2Controller != null)
            {
                boss2Controller.OnHandEndAttack(this);
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu player tấn công vào cánh tay
        // if (other.CompareTag("PlayerAttack") || other.CompareTag("PlayerBullet"))
        // {
        //     // Gây damage cho Boss2
        //     if (boss2Controller != null)
        //     {
        //         boss2Controller.TakeDamage(5); // Damage có thể điều chỉnh
        //     }
        // }
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public void ForceEndAttack()
    {
        if (isAttacking)
        {
            DisableHitBox();
            isAttacking = false;
            currentState = HandState.Idle; // Reset về Idle thay vì Returning
            rb.linearVelocity = Vector2.zero;
            
            // Nếu cánh tay chưa chết, đưa về vị trí ban đầu
            if (!IsDead())
            {
                transform.position = originalPosition;
            }
            
            // Thông báo cho Boss2
            if (boss2Controller != null)
            {
                boss2Controller.OnHandEndAttack(this);
            }
        }
    }

    public bool CanAttack()
    {
        return !isAttacking && 
            !IsDead() && 
            currentState == HandState.Idle &&
            Time.time - lastAttackTime >= attackCooldown;
    }

    public bool IsDead()
    {
        return damageReceiver != null && damageReceiver.IsDead();
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
        if (damageReceiver != null && !damageReceiver.IsDead())
        {
            PlaySound(handHurtSound, 0.7f);

            // Chỉ gọi OnHurt của boss2Controller khi cánh tay bị hurt
            if (boss2Controller != null)
            {
                boss2Controller.OnHurt();
            }
        }
        else if (damageReceiver != null && damageReceiver.IsDead())
        {
            PlaySound(handDeathSound, 0.8f);
            
            // Nếu cánh tay đang tấn công thì dừng lại
            if (isAttacking)
            {
                ForceEndAttack();
            }
            
            // Khi cánh tay chết, kiểm tra trạng thái shield
            if (boss2Controller != null)
            {
                boss2Controller.CheckHandsStatus();
            }

            // Vô hiệu hóa cánh tay
            OnDead();
        }
    }

     public void OnDead()
    {
        animator.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Destroy(gameObject, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Hiển thị attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Hiển thị vị trí ban đầu
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originalPosition, 0.5f);
    }
}