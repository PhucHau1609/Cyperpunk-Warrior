using UnityEngine;

public class TeleportBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float explosionRadius = 2f;
    public float explosionDamage = 20f;
    
    private Animator animator;
    private bool hasExploded = false;

    [Header("Audio Settings")]
    public AudioClip explosionSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.8f;

    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;
    }
    
    public void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        // Kích hoạt animation nổ
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
        
        // Gây damage cho Player nếu trong vùng nổ
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, explosionRadius, LayerMask.GetMask("TransparentFX"));
        
        if (playerCollider != null && playerCollider.CompareTag("Player"))
        {
            CharacterController2D playerController = playerCollider.GetComponent<CharacterController2D>();
            if (playerController != null)
            {
                playerController.ApplyDamage(explosionDamage, transform.position);
            }
        }
        
        // Hủy bomb sau khi nổ
        Destroy(gameObject, 1f);
    }
    
    // Gọi từ Animation Event khi kết thúc animation nổ
    public void OnExplosionComplete()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ vùng nổ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}