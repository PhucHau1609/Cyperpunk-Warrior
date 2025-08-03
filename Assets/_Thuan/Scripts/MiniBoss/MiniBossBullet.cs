using UnityEngine;

public class MiniBossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;
    public float lifetime = 5f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;
    private bool isInitialized = false;
    private Animator animator;
    private bool isDestroyed = false;
    private bool hasStopped = false; // Thêm flag để kiểm soát việc dừng

    [Header("Audio Settings")]
    public AudioClip fireSound;
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Tự động hủy sau thời gian lifetime
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 moveDirection, float bulletSpeed)
    {
        direction = moveDirection.normalized;
        speed = bulletSpeed;
        isInitialized = true;

        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Thiết lập velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Xoay bullet theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void FixedUpdate()
    {
        // Chỉ di chuyển nếu chưa bị dừng
        if (isInitialized && rb != null && !hasStopped && !isDestroyed)
        {
            // Đảm bảo bullet bay theo hướng đã thiết lập
            rb.linearVelocity = direction * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        // Kiểm tra va chạm với Player
        if (other.CompareTag("Player"))
        {
            StopBulletMovement();
            
            // Gây damage cho Player sử dụng hàm ApplyDamage từ CharacterController2D
            CharacterController2D playerController = other.GetComponent<CharacterController2D>();
            if (playerController != null)
            {
                // Truyền damage và vị trí của bullet để tính toán knockback
                playerController.ApplyDamage(damage, transform.position);
            }

            CreateImpactEffect();
        }
        // Kiểm tra va chạm với Ground/Wall
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            StopBulletMovement();
            CreateImpactEffect();
        }
    }

    void StopBulletMovement()
    {
        hasStopped = true;
        
        if (rb != null)
        {
            // Dừng hoàn toàn mọi chuyển động
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            // Tùy chọn: Có thể đóng băng bullet tại vị trí hiện tại
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void CreateImpactEffect()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }
        
        // Kích hoạt animation hủy (nếu có)
        if (animator != null)
        {
            animator.SetTrigger("Destroy");
        }
        else
        {
            // Nếu không có animation, hủy ngay lập tức
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        // Hủy bullet khi ra khỏi camera để tối ưu hiệu suất
        if (isInitialized && !isDestroyed)
        {
            Destroy(gameObject);
        }
    }
    
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    // Method bổ sung để force stop bullet từ bên ngoài
    public void ForceStop()
    {
        StopBulletMovement();
    }

    // Method để kiểm tra trạng thái bullet
    public bool IsStopped()
    {
        return hasStopped;
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}