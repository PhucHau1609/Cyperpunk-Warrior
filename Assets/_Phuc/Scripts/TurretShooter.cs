using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [Header("Bắn đạn")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootingInterval = 0.5f;
    public float attackDuration = 5f;
    public float bulletSpeed = 10f;

    private Transform currentTarget;
    private float shootTimer = 0f;
    private float attackTimer = 0f;
    private bool isActive = false;

    // Gọi hàm này khi phát hiện NPC
    public void Activate(Transform target)
    {
        currentTarget = target;
        shootTimer = 0f;
        attackTimer = 0f;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive || currentTarget == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDuration)
        {
            isActive = false;
            return;
        }

        // Xoay turret về phía mục tiêu
        Vector2 dir = currentTarget.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward); // +90 vì sprite hướng xuống

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootingInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || currentTarget == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (currentTarget.position - firePoint.position).normalized;
            rb.linearVelocity = dir * bulletSpeed;

            // Quay viên đạn đúng hướng bay
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
