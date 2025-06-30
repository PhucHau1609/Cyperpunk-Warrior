using UnityEngine;

public class EnemyShooterBossTarget : MonoBehaviour, IExplodable
{
    public enum State { Sleep, Awaken, MoveToShoot, Returning }
    private State currentState = State.Sleep;

    public float moveSpeed = 3f;
    public Vector2 offset = new Vector2(4f, 4f);
    public float shootingInterval = 1.5f;
    public float chaseDuration = 10f;

    public GameObject bulletPrefab;
    public Transform firePoint;

    public Vector2 moveAreaMin;
    public Vector2 moveAreaMax;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject explosionSoundPrefab;

    private Transform bossTarget;
    private Animator animator;
    private float shootTimer = 0f;
    private float chaseTimer = 0f;
    private int targetDirection = 1;

    private Vector3 startPos;

    private void Start()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.MoveToShoot:
                if (bossTarget == null) return;

                chaseTimer += Time.deltaTime;
                if (chaseTimer >= chaseDuration)
                {
                    currentState = State.Returning;
                    animator.SetTrigger("Sleep");
                    return;
                }

                UpdateTargetDirection();

                Vector3 targetOffset = new Vector3(-targetDirection * offset.x, offset.y, 0f);
                Vector3 targetPos = bossTarget.position + targetOffset;
                targetPos.x = Mathf.Clamp(targetPos.x, moveAreaMin.x, moveAreaMax.x);
                targetPos.y = Mathf.Clamp(targetPos.y, moveAreaMin.y, moveAreaMax.y);

                MoveTowards(targetPos);
                TryShoot();
                break;

            case State.Returning:
                MoveTowards(startPos);
                if (Vector3.Distance(transform.position, startPos) < 0.1f)
                {
                    currentState = State.Sleep;
                    animator.SetTrigger("Sleep");
                    bossTarget = null;
                }
                break;
        }
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 moveDir = target - transform.position;
        float distance = moveDir.magnitude;

        if (distance > 0.1f)
        {
            Vector3 direction = moveDir.normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Mathf.Abs(direction.x) > 0.05f)
            {
                Vector3 scale = transform.localScale;
                scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

    private void TryShoot()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootingInterval)
        {
            ShootAtBoss();
            shootTimer = 0f;
        }
    }

    private void ShootAtBoss()
    {
        if (bulletPrefab == null || firePoint == null || bossTarget == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletEnemyShooter bulletScript = bullet.GetComponent<BulletEnemyShooter>();
        if (bulletScript != null)
        {
            Vector2 shootDir = (bossTarget.position - firePoint.position).normalized;
            bulletScript.SetDirection(shootDir);
        }
    }

    private void UpdateTargetDirection()
    {
        if (bossTarget == null) return;

        Rigidbody2D rb = bossTarget.GetComponent<Rigidbody2D>();
        float xVelocity = rb != null ? rb.linearVelocity.x : 0f;

        if (Mathf.Abs(xVelocity) > 0.05f)
        {
            targetDirection = xVelocity > 0 ? 1 : -1;
        }
    }

    public void Activate(Transform targetBoss)
    {
        if (currentState == State.Sleep)
        {
            bossTarget = targetBoss;
            currentState = State.Awaken;
            animator.SetTrigger("Awaken");
        }
    }

    public void OnAwakenComplete()
    {
        currentState = State.MoveToShoot;
        chaseTimer = 0f;
        shootTimer = 0f;
        animator.SetTrigger("Chase");
    }

    public void Explode()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (explosionSoundPrefab != null)
            Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
