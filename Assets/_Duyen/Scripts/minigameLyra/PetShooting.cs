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
            rb.linearVelocity = direction * bulletSpeed;
        }

        // Xoay viên đạn theo hướng
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

//using UnityEngine;

//public class PetShooting : MonoBehaviour
//{
//    public GameObject bulletPrefab;
//    public float bulletSpeed = 10f;
//    public Transform firePoint;

//    void Update()
//    {
//        if (Input.GetMouseButtonDown(0)) // Chuột trái
//        {
//            Shoot();
//        }
//    }

//    void Shoot()
//    {
//        if (bulletPrefab == null || firePoint == null) return;

//        // Bắn theo hướng chuột
//        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        mouseWorldPos.z = 0f;

//        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

//        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

//        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.linearVelocity = direction * bulletSpeed;
//        }

//        // Xoay viên đạn theo hướng bắn nếu cần
//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); //Quaternion.Euler(0, 0, angle);
//    }
//}
