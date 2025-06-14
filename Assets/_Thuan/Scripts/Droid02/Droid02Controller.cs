using UnityEngine;

public class Droid02Controller : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;
    public Transform gunPoint;
    public GameObject bulletPrefab;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;
    public float stopDistance = 0.1f;
    public float detectionRange = 6f;
    public float shootingRange = 5f;
    public float bulletInterval = 1f;
    public float shootCooldown = 3f;
    public int maxBulletsBeforeReload = 5;
    public int maxHealth = 100;
    public float maxVerticalOffsetToTarget = 1.0f;

    private int currentHealth;
    private int bulletsFired = 0;
    private bool isReloading = false;
    private bool canShoot = true;

    private Vector2 originalPosition;
    private Vector2 currentTarget;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private enum State { Patrolling, Chasing, Returning, Reloading, Dead }
    private State currentState = State.Patrolling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        originalPosition = transform.position;
        currentTarget = pointB.position;
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        if (currentState == State.Dead) return;

        if (currentState == State.Reloading)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Run", false);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                if (distToPlayer <= detectionRange)
                {
                    float verticalOffset = player.position.y - transform.position.y;

                    if (verticalOffset < maxVerticalOffsetToTarget)
                    {
                        currentState = State.Chasing;
                    }
                }
                break;

            case State.Chasing:
                HandleChase(distToPlayer);
                if (distToPlayer > detectionRange)
                    currentState = State.Returning;
                break;

            case State.Returning:
                ReturnToOrigin();
                if (Vector2.Distance(transform.position, originalPosition) < stopDistance)
                {
                    currentState = State.Patrolling;
                    currentTarget = ClosestPatrolPoint();
                }
                break;
        }

        // 🔁 Flip sprite
        Vector2 dir = Vector2.zero;
        if (currentState == State.Patrolling)
            dir = currentTarget - (Vector2)transform.position;
        else if (currentState == State.Chasing)
            dir = player.position - transform.position;
        else if (currentState == State.Returning)
            dir = originalPosition - (Vector2)transform.position;

        if (dir.x != 0)
            sprite.flipX = dir.x < 0;

        anim.SetBool("Run", Mathf.Abs(rb.linearVelocity.x) > 0.05f);

        // ✅ Đảm bảo GunPoint luôn ở phía trước mặt Enemy
        if (gunPoint != null)
        {
            Vector3 localPos = gunPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (sprite.flipX ? 1 : -1);
            gunPoint.localPosition = localPos;
        }

    }

    void Patrol()
    {
        MoveTo(currentTarget, patrolSpeed);
        if (Vector2.Distance(transform.position, currentTarget) < stopDistance)
        {
            currentTarget = Vector2.Distance(currentTarget, pointA.position) < 0.1f ? pointB.position : pointA.position;
        }
    }

    void HandleChase(float distToPlayer)
    {
        if (isReloading) return;

        if (distToPlayer > shootingRange)
        {
            Vector2 target = new Vector2(player.position.x, rb.position.y);
            MoveTo(target, chaseSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (!canShoot) return;

        anim.SetTrigger("Shoot"); // 🔫 Gọi animation bắn
        canShoot = false;
        Invoke(nameof(ResetShoot), bulletInterval);

        bulletsFired++;
        if (bulletsFired >= maxBulletsBeforeReload)
        {
            StartCoroutine(Reload());
        }
    }

    // 🔥 Gọi từ Animation Event
    public void FireBullet()
    {
        if (bulletPrefab == null || gunPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);
        Vector2 dir = sprite.flipX ? Vector2.left : Vector2.right;
        bullet.GetComponent<Droid02Bullet>()?.SetDirection(dir);
    }

    void ResetShoot()
    {
        canShoot = true;
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        currentState = State.Reloading;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("CountDown");
        
        yield return new WaitForSeconds(shootCooldown);

        bulletsFired = 0;
        isReloading = false;

        anim.ResetTrigger("CountDown");

        // Quay lại trạng thái phù hợp
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= detectionRange)
            currentState = State.Chasing;
        else if (Vector2.Distance(transform.position, originalPosition) > stopDistance)
            currentState = State.Returning;
        else
            currentState = State.Patrolling;
    }
    void ReturnToOrigin()
    {
        Vector2 target = new Vector2(originalPosition.x, rb.position.y);
        MoveTo(target, patrolSpeed);
    }

    void MoveTo(Vector2 destination, float speed)
    {
        Vector2 flatDestination = new Vector2(destination.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, flatDestination, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    Vector2 ClosestPatrolPoint()
    {
        float distA = Vector2.Distance(transform.position, pointA.position);
        float distB = Vector2.Distance(transform.position, pointB.position);
        return (distA < distB) ? pointA.position : pointB.position;
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        anim.SetTrigger("Hurt");

        //HealthBarEnemy.Instance?.ShowHealthBar(transform, currentHealth / (float)maxHealth);

        if (currentHealth <= 0)
        {
            anim.SetTrigger("Death");
            currentState = State.Dead;
            rb.linearVelocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
            this.enabled = false;
            //HealthBarEnemy.Instance?.HideHealthBar();
        }

        CameraFollow.Instance?.ShakeCamera();
    }

   void OnDrawGizmosSelected()
    {
        // Detection Range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Shooting Range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // Forward Shooting Zone (blue rectangle)
        Gizmos.color = Color.cyan;

        // Vertical Target Height (green)
        Gizmos.color = Color.green;
        Vector3 heightBoxCenter = transform.position + Vector3.up * (maxVerticalOffsetToTarget / 2f);
        Vector3 heightBoxSize = new Vector3(1f, maxVerticalOffsetToTarget, 1f);
        Gizmos.DrawWireCube(heightBoxCenter, heightBoxSize);

    #if UNITY_EDITOR
        // Nếu đang chạy trong Editor thì lấy flipX từ SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool isFacingLeft = sr != null && sr.flipX;
    #else
        bool isFacingLeft = false;
    #endif

        Vector3 forwardOffset = isFacingLeft ? Vector3.left : Vector3.right;
        Vector3 boxCenter = transform.position + forwardOffset * (shootingRange / 2f);
        Vector3 boxSize = new Vector3(shootingRange, 1f, 1f);

        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
