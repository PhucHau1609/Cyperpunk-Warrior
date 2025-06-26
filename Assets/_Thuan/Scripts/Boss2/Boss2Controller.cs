using UnityEngine;
using BehaviorDesigner.Runtime;

public class Boss2Controller : MonoBehaviour
{
    [Header("Boss2 Attack Settings")]
    public Transform laserPoint;
    public Transform[] bombPoints;
    public GameObject laserPrefab;
    public GameObject bombPrefab;
    public float attackCooldown = 3f;
    public float laserDuration = 3f;
    public float laserSweepSpeed = 90f;
    public int bombCount = 3;

    [Header("State")]
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public float lastAttackTime = -999f; // Cho phép attack ngay từ đầu
    [HideInInspector] public Transform player;

    private Animator animator;
    private BehaviorTree behaviorTree;
    private Boss2AttackType currentAttackType;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();
        FindPlayer();
    }

    private void Start()
    {
        // Cho phép attack ngay khi scene load
        lastAttackTime = -attackCooldown;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown && !isAttacking;
    }

    public void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Random giữa Laser và Bomb, ưu tiên Laser
        int totalWeight = 7 + 3;
        int randomValue = Random.Range(0, totalWeight);
        currentAttackType = (randomValue < 7) ? Boss2AttackType.Laser : Boss2AttackType.Bomb;

        if (animator != null)
        {
            if (currentAttackType == Boss2AttackType.Laser)
                animator.SetTrigger("LaserAttack");
            else
                animator.SetTrigger("BombAttack");
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    // Animation Event
    public void OnLaserAttackTrigger()
    {
        ExecuteLaserAttack();
    }

    public void OnBombAttackTrigger()
    {
        ExecuteBombAttack();
    }

    public void OnAttackAnimationEnd()
    {
        EndAttack();
    }

    private void ExecuteLaserAttack()
    {
        if (laserPrefab == null || laserPoint == null) return;

        GameObject laser = Instantiate(laserPrefab, laserPoint.position, laserPoint.rotation);
        Boss2Laser laserScript = laser.GetComponent<Boss2Laser>();
        if (laserScript != null)
        {
            laserScript.SetupLaser(240f, 120f, laserSweepSpeed, laserDuration);
        }
    }

    private void ExecuteBombAttack()
    {
        if (bombPrefab == null || bombPoints == null) return;

        int spawnCount = Mathf.Min(bombCount, bombPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            if (bombPoints[i] != null)
            {
                Instantiate(bombPrefab, bombPoints[i].position, Quaternion.identity);
            }
        }
    }
}

public enum Boss2AttackType
{
    Laser,
    Bomb
}
