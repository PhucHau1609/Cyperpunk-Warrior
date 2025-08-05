using UnityEngine;

public class EnemyMini_01 : MonoBehaviour
{
    [Header("Tuần tra")]
    public float patrolXLeft = -3f;
    public float patrolXRight = 3f;
    public float patrolSpeed = 2f;

    [Header("Giới hạn bay Y (tùy chọn)")]
    public float patrolYMin = 0f;
    public float patrolYMax = 5f;

    [Header("Phát hiện và tấn công NPC")]
    public float chaseRange = 7f;
    public float chaseSpeed = 3.5f;
    public float chaseDuration = 5f;
    public float attackDistance = 9f; // Enemy sẽ KHÔNG tiến gần hơn khoảng cách này

    [Header("Tấn công")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;

    private enum EnemyState { Patrol, Chase, Return }
    private EnemyState currentState = EnemyState.Patrol;

    private Transform npcTarget;
    private Vector3 patrolStartPos;
    private Vector3 patrolTarget;
    private float chaseTimer = 0f;
    private float nextFireTime;
    private Vector3 returnPoint;

    private void Start()
    {
        GameObject npcObj = GameObject.FindGameObjectWithTag("NPC");
        if (npcObj != null)
        {
            npcTarget = npcObj.transform;
        }

        patrolStartPos = transform.position;
        patrolTarget = new Vector3(patrolXRight, transform.position.y, transform.position.z);
        FlipTo(patrolTarget.x);
    }

    private void Update()
    {
        if (npcTarget == null) return;

        float distanceToNPC = Vector2.Distance(transform.position, npcTarget.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();

                if (distanceToNPC <= chaseRange)
                {
                    currentState = EnemyState.Chase;
                    chaseTimer = 0f;
                    returnPoint = transform.position; // Ghi nhớ để quay lại sau
                }
                break;

            case EnemyState.Chase:
                Chase(distanceToNPC);
                break;

            case EnemyState.Return:
                ReturnToPatrol();
                break;
        }
    }

    void Patrol()
    {
        Vector3 target = new Vector3(patrolTarget.x, Mathf.Clamp(transform.position.y, patrolYMin, patrolYMax), transform.position.z);
        transform.position = Vector2.MoveTowards(transform.position, target, patrolSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.1f)
        {
            patrolTarget.x = Mathf.Approximately(patrolTarget.x, patrolXLeft) ? patrolXRight : patrolXLeft;
            FlipTo(patrolTarget.x);
        }
    }

    void Chase(float distanceToNPC)
    {
        chaseTimer += Time.deltaTime;

        // Chỉ di chuyển nếu chưa đến khoảng cách giới hạn
        if (distanceToNPC > attackDistance)
        {
            Vector3 target = new Vector3(npcTarget.position.x, npcTarget.position.y, transform.position.z);
            transform.position = Vector2.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
        }

        FlipTo(npcTarget.position.x);

        // Tấn công (bắn hoặc phun lửa)
        // Tấn công (luôn bắn đạn)
        if (Time.time >= nextFireTime)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Vector2 dir = (npcTarget.position - firePoint.position).normalized;

            BulletEnemyToNPC bulletScript = bullet.GetComponent<BulletEnemyToNPC>();
            if (bulletScript != null)
                bulletScript.SetDirection(dir);

            nextFireTime = Time.time + 1f / fireRate;
        }


        if (chaseTimer >= chaseDuration)
        {
            currentState = EnemyState.Return;
        }
    }

    void ReturnToPatrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, returnPoint, patrolSpeed * Time.deltaTime);

        FlipTo(returnPoint.x);

        if (Vector2.Distance(transform.position, returnPoint) < 0.1f)
        {
            patrolTarget = new Vector3(patrolTarget.x, transform.position.y, transform.position.z);
            currentState = EnemyState.Patrol;
        }
    }

    void FlipTo(float targetX)
    {
        bool shouldFaceRight = targetX > transform.position.x;
        if ((shouldFaceRight && transform.localScale.x < 0) ||
            (!shouldFaceRight && transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}