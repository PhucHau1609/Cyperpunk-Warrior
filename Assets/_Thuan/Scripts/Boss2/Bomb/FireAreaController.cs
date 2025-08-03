using UnityEngine;
using System.Collections;

public class FireAreaController : MonoBehaviour
{
    [Header("Fire Settings")]
    public float fireDuration = 5f; // Thời gian lửa tồn tại
    public int damagePerSecond = 2; // Damage mỗi giây
    public float damageInterval = 1f; // Khoảng thời gian giữa các lần damage
    
    [Header("Visual Effects")]
    public AudioClip fireSound; // Âm thanh lửa cháy
    
    private bool isActive = false;
    private AudioSource audioSource;
    private Coroutine fireCoroutine;
    private GameObject currentPlayer; // Thay đổi từ DamageReceiver sang GameObject
    private Coroutine damageCoroutine;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        StartFire();
    }
    
    public void StartFire()
    {
        if (isActive) return;
        
        isActive = true;
        
        // Phát âm thanh
        if (fireSound != null && audioSource != null)
        {
            audioSource.clip = fireSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Bắt đầu timer để tắt lửa
        fireCoroutine = StartCoroutine(FireDurationCoroutine());
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;
            StartDamageCoroutine();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            currentPlayer = null;
            StopDamageCoroutine();
        }
    }
    
    void StartDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        damageCoroutine = StartCoroutine(DamagePlayerCoroutine());
    }
    
    void StopDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }
    
    IEnumerator DamagePlayerCoroutine()
    {
        while (isActive && currentPlayer != null)
        {
            // Sử dụng hàm ApplyDamageToPlayer
            ApplyDamageToPlayer(damagePerSecond, transform.position);
            yield return new WaitForSeconds(damageInterval);
        }
    }
    
    // Hàm gây damage cho Player sử dụng reflection
    public void ApplyDamageToPlayer(float damage, Vector3 attackPosition)
    {
        if (currentPlayer != null)
        {
            var playerScript = currentPlayer.GetComponent<MonoBehaviour>();
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
    
    IEnumerator FireDurationCoroutine()
    {
        yield return new WaitForSeconds(fireDuration);
        ExtinguishFire();
    }
    
    void ExtinguishFire()
    {
        if (!isActive) return;
        
        isActive = false;
        
        // Tắt âm thanh
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Dừng damage
        StopDamageCoroutine();
        
        // Dừng fire coroutine
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
        }
        
        // Destroy object sau một chút để đảm bảo tất cả effect đã tắt
        Destroy(gameObject, 0.5f);
    }
    void OnDestroy()
    {
        // Cleanup khi object bị destroy
        StopDamageCoroutine();
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
        }
    }
}