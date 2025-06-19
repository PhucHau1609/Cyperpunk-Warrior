using UnityEngine;

public class BossPhuBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 25f;
    public float lifetime = 5f;
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;
    private bool isInitialized = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (isInitialized && rb != null)
        {
            // Đảm bảo bullet bay theo hướng đã thiết lập
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra va chạm với Player
        if (other.CompareTag("Player"))
        {
            // Gây damage cho Player
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damage);
            // }
            
            // Tạo hiệu ứng nổ hoặc va chạm (nếu có)
            CreateImpactEffect();
            
            // Hủy bullet
            Destroy(gameObject);
        }
        // Kiểm tra va chạm với Ground/Wall
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            CreateImpactEffect();
            Destroy(gameObject);
        }
    }
    
    void CreateImpactEffect()
    {
        // Tạo hiệu ứng particle hoặc animation khi bullet va chạm
        // Bạn có thể thêm particle system tại đây
        
        // Ví dụ: Instantiate impact particle
        // if (impactEffectPrefab != null)
        // {
        //     Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        // }
    }
    
    void OnBecameInvisible()
    {
        // Hủy bullet khi ra khỏi camera để tối ưu hiệu suất
        if (isInitialized)
        {
            Destroy(gameObject);
        }
    }
}