using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float damage = 30f;                    // Sát thương gây ra cho Player
    public float explosionRadius = 2f;            // Bán kính nổ

    private Rigidbody2D rb;
    private Animator anim;
    private bool hasExploded = false;

    [Header("Audio Settings")]
    public AudioClip explosionSound;
    public AudioClip fallSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;
        
        // Play fall sound
        if (fallSound != null)
        {
            audioSource.clip = fallSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Bắt đầu rơi
        rb.gravityScale = 1f;
    }

    void Update()
    {
        // Không cần xử lý gì thêm trong Update
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play explosion sound
            if (explosionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }

            // Kiểm tra sát thương trong bán kính nổ
            CheckExplosionDamage();
            
            anim.SetTrigger("Explode");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu bomb chạm Player trước khi nổ (trường hợp hiếm)
        if (!hasExploded && collision.CompareTag("Player"))
        {
            hasExploded = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Play explosion sound
            if (explosionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }

            // Gây sát thương trực tiếp
            CharacterController2D playerController = collision.GetComponent<CharacterController2D>();
            if (playerController != null)
            {
                playerController.ApplyDamage(damage, transform.position);
            }

            CheckExplosionDamage();
            anim.SetTrigger("Explode");
        }

        if (!hasExploded && collision.CompareTag("Ground"))
        {
            hasExploded = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Play explosion sound
            if (explosionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }

            anim.SetTrigger("Explode");
        }
    }

    void CheckExplosionDamage()
    {
        // Tìm tất cả Player trong bán kính nổ
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                CharacterController2D playerController = col.GetComponent<CharacterController2D>();
                if (playerController != null)
                {
                    // Tính khoảng cách để giảm sát thương theo khoảng cách
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    float damageFactor = 1f - (distance / explosionRadius);
                    float finalDamage = damage * Mathf.Clamp01(damageFactor);
                    
                    playerController.ApplyDamage(finalDamage, transform.position);
                }
            }
        }
    }

    // Để debug bán kính nổ trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireCircle(transform.position, explosionRadius);
    }

    // Gọi từ Event trong animation nổ
    public void DestroyBomb()
    {
        Destroy(gameObject);
    }
}