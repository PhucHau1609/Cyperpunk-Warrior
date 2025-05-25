using UnityEngine;

public class DroidController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform gunPoint;
    public GameObject bulletPrefab;
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float shootCooldown = 1.5f;
    public float shootingRange = 6f;

    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector3 nextPoint;
    private bool isPlayerDetected = false;
    private float lastShootTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nextPoint = pointB.position;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;

        if (isPlayerDetected)
        {
            if (distanceToPlayer <= shootingRange)
            {
                rb.velocity = Vector2.zero;
                anim.SetBool("Run", false);
                anim.SetTrigger("Shoot");

                FacePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("Run", true);
        transform.position = Vector2.MoveTowards(transform.position, nextPoint, moveSpeed * Time.deltaTime);

        Vector2 dir = nextPoint - transform.position;
        spriteRenderer.flipX = dir.x < 0;

        if (Vector2.Distance(transform.position, nextPoint) < 0.1f)
        {
            nextPoint = nextPoint == pointA.position ? pointB.position : pointA.position;
        }
    }

    void FacePlayer()
    {
        if (player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    // Gọi từ Event trong Animation Shoot
    public void ShootBullet()
    {
        if (Time.time - lastShootTime < shootCooldown) return;
        lastShootTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);
        Vector2 dir = (player.position - gunPoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bullet.GetComponent<Droid01Bullet>().SetDirection(dir);
    }

    public void TakeDamage()
    {
        anim.SetTrigger("Hurt");
        // Xử lý máu nếu cần
    }

    public void Die()
    {
        anim.SetTrigger("Death");
        Destroy(gameObject, 1.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
