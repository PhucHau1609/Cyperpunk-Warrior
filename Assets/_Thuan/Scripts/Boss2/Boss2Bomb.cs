using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Boss2Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float fuseTime = 2.5f;
    [SerializeField] private LayerMask groundLayer = -1;
    
    [Header("Physics")]
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private Vector2 randomForceRange = new Vector2(-1f, 1f);
    [SerializeField] private float bounceForce = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionEffectDuration = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip tickSound;
    [SerializeField] private AudioClip explosionSound;
    
    // Components
    private Rigidbody2D rb;
    private Collider2D bombCollider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    // State
    private bool hasExploded = false;
    private bool isArmed = false;
    private bool isBlinking = false;
    
    // Visual
    private Color originalColor;
    private Color warningColor = Color.red;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bombCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Cấu hình physics
        if (bombCollider != null)
        {
            bombCollider.isTrigger = false; // Không phải trigger để có thể va chạm
        }
    }
    
    private void Start()
    {
        ApplyInitialForce();
        StartCoroutine(ArmBombDelayed());
        StartCoroutine(FuseCountdown());
        StartCoroutine(WarningBlink());
    }
    
    private void ApplyInitialForce()
    {
        if (rb != null)
        {
            // Thêm lực rơi và lực ngẫu nhiên theo trục x
            Vector2 force = new Vector2(
                Random.Range(randomForceRange.x, randomForceRange.y),
                -dropForce
            );
            
            rb.AddForce(force, ForceMode2D.Impulse);
            
            Debug.Log($"Boss2Bomb: Áp dụng lực {force}");
        }
    }
    
    private IEnumerator ArmBombDelayed()
    {
        yield return new WaitForSeconds(0.3f);
        isArmed = true;
        Debug.Log("Boss2Bomb: Đã được kích hoạt");
    }
    
    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        
        if (!hasExploded)
        {
            Debug.Log("Boss2Bomb: Tự nổ do hết thời gian");
            Explode();
        }
    }
    
    private IEnumerator WarningBlink()
    {
        // Bắt đầu nhấp nháy khi còn 1.5 giây
        float warningTime = fuseTime - 1.5f;
        if (warningTime > 0)
        {
            yield return new WaitForSeconds(warningTime);
        }
        
        if (!hasExploded && spriteRenderer != null)
        {
            isBlinking = true;
            float blinkRate = 0.15f;
            
            while (!hasExploded && isBlinking)
            {
                // Đổi sang màu đỏ
                spriteRenderer.color = warningColor;
                PlayTickSound();
                yield return new WaitForSeconds(blinkRate);
                
                if (!hasExploded)
                {
                    // Đổi về màu gốc
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(blinkRate);
                }
                
                // Tăng tốc độ nhấp nháy khi gần nổ
                blinkRate = Mathf.Max(0.05f, blinkRate - 0.01f);
            }
        }
    }
    
    private void PlayTickSound()
    {
        if (audioSource != null && tickSound != null)
        {
            audioSource.PlayOneShot(tickSound);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded || !isArmed) return;
        
        // Kiểm tra va chạm với ground
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 || 
            collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Boss2Bomb: Va chạm với ground!");
            
            // Thêm hiệu ứng bounce nhẹ
            if (rb != null && rb.linearVelocity.y < -1f)
            {
                Vector2 bounceVelocity = rb.linearVelocity;
                bounceVelocity.y = -bounceVelocity.y * 0.3f; // Bounce với lực nhỏ
                rb.linearVelocity = bounceVelocity;
            }
            
            // Không nổ ngay mà chờ fuse time
        }
        
        // Kiểm tra va chạm với Player (nổ ngay lập tức)
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boss2Bomb: Va chạm với Player!");
            Explode();
        }
    }
    
    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        isBlinking = false;
        
        Debug.Log("Boss2Bomb: NỔ!");
        
        // Dừng physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Tạo explosion effect
        CreateExplosionEffect();
        
        // Phát âm thanh nổ
        PlayExplosionSound();
        
        // Kiểm tra explosion area (không gây damage, chỉ để debug)
        CheckExplosionArea();
        
        // Ẩn bomb và tự hủy
        StartCoroutine(DestroyAfterExplosion());
    }
    
    private void CreateExplosionEffect()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, explosionEffectDuration);
        }
        else
        {
            // Tạo explosion visual đơn giản
            Debug.Log("Boss2Bomb: Explosion effect triggered!");
        }
    }
    
    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }
    
    private void CheckExplosionArea()
    {
        // Tìm tất cả collider trong vùng nổ
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        Debug.Log($"Boss2Bomb: Explosion ảnh hưởng đến {hitColliders.Length} objects");
        
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Boss2Bomb: Player trong vùng nổ!");
                // Có thể thêm effects như camera shake, knockback, v.v.
            }
        }
    }
    
    private IEnumerator DestroyAfterExplosion()
    {
        // Ẩn sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Chờ effect hoàn thành
        yield return new WaitForSeconds(explosionEffectDuration);
        
        Destroy(gameObject);
    }
    
    // Public methods
    public void ForceExplode()
    {
        if (!hasExploded)
        {
            StopAllCoroutines();
            Explode();
        }
    }
    
    public void SetFuseTime(float newFuseTime)
    {
        if (!hasExploded && !isArmed)
        {
            fuseTime = newFuseTime;
        }
    }
    
    // Cleanup
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    // Gizmos để hiển thị explosion radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = hasExploded ? Color.gray : Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        
        // Hiển thị trajectory nếu đang rơi
        if (rb != null && !hasExploded && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 velocity = rb.linearVelocity;
            Vector3 pos = transform.position;
            
            // for (int i = 1; i <= 10; i++)
            // {
            //     float time = i * 0.1f;
            //     Vector3 futurePos = pos + velocity * time + 0.5f * Physics2D.gravity * time * time;
            //     Gizmos.DrawWireSphere(futurePos, 0.1f);
                
            //     if (i > 1)
            //     {
            //         Vector3 prevPos = pos + velocity * (time - 0.1f) + 0.5f * Physics2D.gravity * (time - 0.1f) * (time - 0.1f);
            //         Gizmos.DrawLine(prevPos, futurePos);
            //     }
            // }
        }
    }
}