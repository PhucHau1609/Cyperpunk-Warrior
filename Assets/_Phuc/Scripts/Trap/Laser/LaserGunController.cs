using UnityEngine;

public class LaserGunController : MonoBehaviour
{
    [SerializeField] private float minX = -0.5f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireIntervalMin = 1f;
    [SerializeField] private float fireIntervalMax = 3f;

    private bool movingRight = true;
    private float fireTimer;

    void Start()
    {
        ResetFireTimer();
    }

    void Update()
    {
        Move();
        HandleShooting();
    }

    void Move()
    {
        float moveStep = moveSpeed * Time.deltaTime;
        if (movingRight)
        {
            transform.position += Vector3.right * moveStep;
            if (transform.position.x >= maxX)
                movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * moveStep;
            if (transform.position.x <= minX)
                movingRight = true;
        }
    }

    void HandleShooting()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            ResetFireTimer();
        }
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint)
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }

    void ResetFireTimer()
    {
        fireTimer = Random.Range(fireIntervalMin, fireIntervalMax);
    }
}
