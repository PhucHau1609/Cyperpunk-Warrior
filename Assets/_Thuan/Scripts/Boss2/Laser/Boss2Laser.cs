using UnityEngine;
using System.Collections;

public class Boss2Laser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float phase2Duration = 1f;

    private Boss2Controller boss2Controller;

    [Header("Damage Settings")]
    public float laserDamage = 5f;
    public float damageInterval = 0.5f; // Damage mỗi 0.5 giây

    private bool isPlayerInLaser = false;
    private Coroutine damageCoroutine;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip laserBeamSound;
    public AudioClip laserHitSound;

    private void Start()
    {
        // Tìm Boss2Controller để kiểm tra Phase
        boss2Controller = FindFirstObjectByType<Boss2Controller>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Phát âm thanh laser beam
        if (audioSource != null && laserBeamSound != null)
        {
            audioSource.clip = laserBeamSound;
            audioSource.loop = true;
            audioSource.volume = 0.7f;
            audioSource.Play();
        }

        if (boss2Controller != null && boss2Controller.isPhase2)
        {
            // Phase 2: Laser cố định, không quét
            StartCoroutine(Phase2LaserBehavior());
        }
        else
        {
            // Phase 1: Laser quét như cũ
            StartCoroutine(SweepLaser());
        }
    }

    private System.Collections.IEnumerator Phase2LaserBehavior()
    {
        // Laser cố định trong thời gian phase2Duration
        yield return new WaitForSeconds(phase2Duration);
        Destroy(gameObject);
    }

    private IEnumerator SweepLaser()
    {
        float startAngle = 290f;      // Góc khoảng 10 giờ (dưới trái)
        float endAngle = 70f;         // Góc khoảng 2 giờ (dưới phải)
        float sweepDuration = 1f;

        float elapsedTime = 0f;
        while (elapsedTime < sweepDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sweepDuration;
            float currentAngle = Mathf.LerpAngle(startAngle, endAngle, progress);

            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            yield return null;
        }

        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
             if (audioSource != null && laserHitSound != null)
            {
                audioSource.PlayOneShot(laserHitSound, 0.6f);
            }

            isPlayerInLaser = true;
            StartDamageCoroutine(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInLaser = false;
            StopDamageCoroutine();
        }
    }

    private void StartDamageCoroutine(GameObject player)
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        damageCoroutine = StartCoroutine(DamagePlayerCoroutine(player));
    }

    private void StopDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private System.Collections.IEnumerator DamagePlayerCoroutine(GameObject player)
    {
        while (isPlayerInLaser && player != null)
        {
            // Gọi hàm ApplyDamage của Player
            var playerScript = player.GetComponent<MonoBehaviour>();
            if (playerScript != null)
            {
                var applyDamageMethod = playerScript.GetType().GetMethod("ApplyDamage",
                    new System.Type[] { typeof(float), typeof(Vector3) });

                if (applyDamageMethod != null)
                {
                    applyDamageMethod.Invoke(playerScript, new object[] { laserDamage, transform.position });
                }
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }
    
    void OnDestroy()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Cleanup coroutine khi laser bị destroy
        StopDamageCoroutine();
    }
}