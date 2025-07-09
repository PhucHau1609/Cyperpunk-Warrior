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
        if (boss2Controller == null)
        {
            Debug.LogError($"{gameObject.name}: Không tìm thấy Boss2Controller!");
        }
        
        // Tìm Player
        FindPlayer();
        
        // Đăng ký cánh tay với Boss2
        if (boss2Controller != null)
        {
            boss2Controller.RegisterHand(this);
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
            Debug.Log($"{gameObject.name}: Không thể tấn công - isAttacking:{isAttacking}, IsDead:{IsDead()}, State:{currentState}");
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
        
        Debug.Log($"{gameObject.name}: Bắt đầu tấn công {attackType} tại vị trí {targetPosition}!");
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
            
            Debug.Log($"{gameObject.name}: Thực hiện {attackAnimation} tại target position!");
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
            Debug.Log($"{gameObject.name}: HitBox enabled for attack!");
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
            
            Debug.Log($"{gameObject.name}: Đã trở về vị trí ban đầu");
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
        //         Debug.Log($"{gameObject.name}: Player tấn công vào cánh tay!");
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
            
            Debug.Log($"{gameObject.name}: Buộc kết thúc tấn công");
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

            Debug.Log($"[{gameObject.name}] Hand is dead!");
            
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