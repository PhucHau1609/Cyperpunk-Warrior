using UnityEngine;

public class PetShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Click chuột trái
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Lấy vị trí chuột trong thế giới
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Tính hướng từ Pet đến chuột
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // Tạo đạn tại firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Gán vận tốc
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f; // Tắt trọng lực
            rb.linearVelocity = direction * bulletSpeed;
        }
    }
}
