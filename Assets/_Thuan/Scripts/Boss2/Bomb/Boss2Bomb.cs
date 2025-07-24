using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class Boss2Bomb : MonoBehaviour
{
    [Header("Fire Area Settings")]
    public GameObject fireAreaPrefab; // Gán Fire Area Prefab
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasExploded = false;
    private Boss2Controller boss2Controller;

    [Header("Damage Settings")]
    public float explosionDamage = 5f;
    public float explosionRadius = 2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip explosionSound;
    public AudioClip fireAreaCreateSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Tìm Boss2Controller
        boss2Controller = FindFirstObjectByType<Boss2Controller>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound, 1f);
        }

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // Dừng vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Tắt collider
        if (col != null)
        {
            col.enabled = false;
        }

        // Gây damage cho Player nếu trong phạm vi nổ
        DamagePlayerInExplosion();

        // Tạo vùng lửa nếu Boss dưới 50% máu
        CreateFireArea();

        Destroy(gameObject, 1f);
    }

    private void DamagePlayerInExplosion()
    {
        // Tìm Player trong phạm vi nổ
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, explosionRadius, LayerMask.GetMask("TransparentFX"));

        if (playerCollider != null && playerCollider.CompareTag("Player"))
        {

            // Gọi hàm ApplyDamage của Player
            var playerScript = playerCollider.GetComponent<MonoBehaviour>();
            if (playerScript != null)
            {
                var applyDamageMethod = playerScript.GetType().GetMethod("ApplyDamage",
                    new System.Type[] { typeof(float), typeof(Vector3) });

                if (applyDamageMethod != null)
                {
                    applyDamageMethod.Invoke(playerScript, new object[] { explosionDamage, transform.position });
                    Debug.Log($"[Boss2Bomb] Explosion dealt {explosionDamage} damage to Player!");
                }
            }
        }
    }

    private void CreateFireArea()
    {
        // Kiểm tra Boss2 có dưới 50% máu không (thay vì kiểm tra Shield)
        if (boss2Controller != null && fireAreaPrefab != null)
        {
            // Lấy thông tin HP từ DamageReceiver
            var damageReceiver = boss2Controller.GetComponent<Boss2DamageReceiver>();
            if (damageReceiver != null && damageReceiver.CurrentHP < damageReceiver.MaxHP / 2)
            {
                if (audioSource != null && fireAreaCreateSound != null)
                {
                    audioSource.PlayOneShot(fireAreaCreateSound, 0.8f);
                }

                // Tạo vùng lửa tại vị trí bomb nổ
                Vector3 firePosition = transform.position;
                // Có thể điều chỉnh vị trí Y để vùng lửa nằm sát mặt đất
                firePosition.y -= 0f; // Điều chỉnh theo nhu cầu

                GameObject fireArea = Instantiate(fireAreaPrefab, firePosition, Quaternion.identity);
                Debug.Log("[Boss2Bomb] Created fire area at explosion site!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Hiển thị phạm vi nổ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}