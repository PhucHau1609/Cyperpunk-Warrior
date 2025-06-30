using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using static EnemyMini;

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

    public Boss2HealthBar healthBar;
    private State currentState = State.Idle;
    public Boss2DamageReceiver damageReceiver;
    
    // Hand Management
    private List<Boss2HandController> hands = new List<Boss2HandController>();
    private Boss2HandController attackingHand = null;

    private void Awake()
    {
        damageReceiver = GetComponent<Boss2DamageReceiver>();
        animator = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();
        FindPlayer();
    }

    private void Start()
    {
        float normalizedHealth = GetNormalizedHealth();
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(normalizedHealth);
        }

        // Cho phép attack ngay khi scene load
        lastAttackTime = -attackCooldown;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        CheckDeathState();
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
        // Boss2 chỉ có thể tấn công khi không có cánh tay nào đang tấn công
        return Time.time - lastAttackTime >= attackCooldown && 
               !isAttacking && 
               attackingHand == null;
    }
    
    // Hand Management Methods
    public void RegisterHand(Boss2HandController hand)
    {
        if (!hands.Contains(hand))
        {
            hands.Add(hand);
            Debug.Log($"Đã đăng ký {hand.gameObject.name} với Boss2");
        }
    }
    
     public void OnHandStartAttack(Boss2HandController hand)
    {
        // Method này giờ chỉ để log, attackingHand đã được set trong ExecuteHandAttack
        Debug.Log($"Boss2: {hand.gameObject.name} bắt đầu tấn công");
    }
    
    private void ExecuteHandAttack(Boss2AttackType attackType)
    {
        if (player == null || hands.Count == 0)
        {
            // Nếu không có player hoặc cánh tay, kết thúc attack
            EndAttack();
            return;
        }

        // Tìm cánh tay gần player nhất
        Boss2HandController closestHand = null;
        float closestDistance = float.MaxValue;

        foreach (var hand in hands)
        {
            if (hand != null)
            {
                float distance = Vector2.Distance(hand.transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHand = hand;
                }
            }
        }

        if (closestHand != null)
        {
            closestHand.StartHandAttack(attackType, player.position);
            attackingHand = closestHand;
            Debug.Log($"Boss2: {closestHand.gameObject.name} thực hiện {attackType}");
        }
        else
        {
            // Không có cánh tay khả dụng, kết thúc attack
            EndAttack();
        }
    }
    
    public void OnHandEndAttack(Boss2HandController hand)
    {
        if (attackingHand == hand)
        {
            attackingHand = null;
            Debug.Log($"Boss2: {hand.gameObject.name} kết thúc tấn công");
        }
    }
    
    public bool IsAnyHandOrBossAttacking()
    {
        return isAttacking || attackingHand != null;
    }
    
    public Boss2HandController GetOtherHand(Boss2HandController currentHand)
    {
        foreach (var hand in hands)
        {
            if (hand != currentHand)
                return hand;
        }
        return null;
    }

    public void StartAttack()
    {
        // Chỉ cho phép Boss2 tấn công khi không có cánh tay nào đang tấn công
        if (attackingHand != null) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;

        // Random 4 attack: Laser(40%), Bomb(20%), Attack3(20%), Attack4(20%)
        int[] weights = {40, 20, 20, 20}; // Laser, Bomb, Attack3, Attack4
        int totalWeight = 100;
        int randomValue = Random.Range(0, totalWeight);
        
        Boss2AttackType selectedAttack;
        if (randomValue < weights[0]) // 0-39: Laser
        {
            selectedAttack = Boss2AttackType.Laser;
        }
        else if (randomValue < weights[0] + weights[1]) // 40-59: Bomb
        {
            selectedAttack = Boss2AttackType.Bomb;
        }
        else if (randomValue < weights[0] + weights[1] + weights[2]) // 60-79: Attack3
        {
            selectedAttack = Boss2AttackType.Attack3;
        }
        else // 80-99: Attack4
        {
            selectedAttack = Boss2AttackType.Attack4;
        }
        
        currentAttackType = selectedAttack;

        if (animator != null && (selectedAttack == Boss2AttackType.Laser || selectedAttack == Boss2AttackType.Bomb))
        {
            if (selectedAttack == Boss2AttackType.Laser)
            {
                animator.SetTrigger("LaserAttack");
                CameraFollow.Instance?.ShakeCamera();
            }
            else if (selectedAttack == Boss2AttackType.Bomb)
            {
                animator.SetTrigger("BombAttack");
                CameraFollow.Instance?.ShakeCamera();
            }
        }
        else if (selectedAttack == Boss2AttackType.Attack3 || selectedAttack == Boss2AttackType.Attack4)
        {
            // Tìm cánh tay gần player nhất để tấn công
            ExecuteHandAttack(selectedAttack);
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

        Instantiate(laserPrefab, laserPoint.position, laserPoint.rotation);
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

    void CheckPhaseChange()
    {
       
    }

    void CheckDeathState()
    {
        if (damageReceiver != null && damageReceiver.IsDead() && currentState != State.Dead)
        {
            OnDead();
        }
    }

    public void StartPhaseChange()
    {
      
    }

    void EndPhaseChange()
    {
        
    }

    private enum State
    {
        Idle,
        Attacking,
        Hurt,
        Dead
    }

    private float GetNormalizedHealth()
    {
        if (damageReceiver != null && damageReceiver.MaxHP > 0)
            return damageReceiver.CurrentHP / (float)damageReceiver.MaxHP;
        else
            return 1f;
    }
    
    public void TakeDamage(int damage)
    {
        if (damageReceiver != null)
        {
            damageReceiver.TakeDamage(damage);
            OnDamageReceived();
        }
    }

    public void OnDamageReceived()
    {
        // Cập nhật thanh máu
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(damageReceiver.CurrentHP);
        }

        // Xử lý khi bị thương (nếu chưa chết)
        if (!damageReceiver.IsDead())
        {
            OnHurt();
        }
    }

    public void OnHurt()
    {
        if (currentState == State.Dead) return;

        animator.SetTrigger("Hurt");
        CameraFollow.Instance?.ShakeCamera();

        if (healthBar != null)
        {
            healthBar.ShowHealthBar(GetNormalizedHealth());
        }
    }

    public void OnDead()
    {
        animator.SetTrigger("Death");
        currentState = State.Dead;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        healthBar?.HideHealthBar();

        var behavior = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        if (behavior != null) behavior.DisableBehavior();
        
        // Buộc tất cả cánh tay dừng tấn công
        foreach (var hand in hands)
        {
            if (hand != null)
            {
                hand.ForceEndAttack();
            }
        }

        Destroy(gameObject, 2f);
    }
}

public enum Boss2AttackType
{
    Laser,
    Bomb,
    Attack3,
    Attack4
}