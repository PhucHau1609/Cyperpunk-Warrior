using UnityEngine;

public class CannonShooter : MonoBehaviour
{
    public GameObject bulletPrefab;     // Prefab viên đạn
    public Transform firePoint;         // Vị trí đầu nòng pháo
    public float bulletSpeed = 10f;     // Tốc độ đạn
    public float fireRate = 1f;         // Số lần bắn/giây
    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            FireBullet();
            fireCooldown = 1f / fireRate;
        }
    }

    void FireBullet()
    {
        // Tạo viên đạn tại firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Lấy Rigidbody2D và đẩy đạn theo hướng transform.up
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.up * bulletSpeed;
        }
    }
}
