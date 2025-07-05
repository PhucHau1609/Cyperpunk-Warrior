using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class GunController : MonoBehaviour
{
    [Header("Gun Setup")]
    public Animator gunAnimator;               // Animator cho animation súng (Gun_Attack)
    public GameObject bulletPrefab;            // Prefab viên đạn
    public Transform muzzlePoint;              // Vị trí nòng súng (bắn đạn)
    public GameObject muzzleFlash;             // GameObject hiệu ứng tia lửa

    [Header("Thông số đạn")]
    public float bulletSpeed = 10f;

    [Header("Bắn đạn")]
    public float shootInterval = 2f;           // Thời gian delay giữa mỗi lần bắn

    [Header("Âm thanh")]
    public AudioClip shootSound;               // Âm thanh khi bắn
    private AudioSource audioSource;

    private float shootTimer = 0f;

    void Start()
    {
        shootTimer = 0f;

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        // Gọi animation Gun_Attack
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger("Attack");
            gunAnimator.SetTrigger("Attack");
        }

        // Gọi hiệu ứng tia lửa
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false); // reset lại
            muzzleFlash.SetActive(true);
            StartCoroutine(DisableFlash());
        }

        // Phát âm thanh
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Tạo viên đạn
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = -muzzlePoint.right * bulletSpeed; // Bắn sang trái
            }
        }
    }

    IEnumerator DisableFlash()
    {
        yield return new WaitForSeconds(0.2f); // Sau 0.2s tắt hiệu ứng
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }
}
