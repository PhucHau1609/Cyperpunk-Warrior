using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss/Actions")]
public class BossRangedAttack : Action
{
    [Tooltip("Thời gian delay trước khi bắn")]
    public float shootDelay = 0.5f;
    
    [Tooltip("Thời gian hoàn thành attack")]
    public float attackDuration = 1.5f;
    
    [Tooltip("Cooldown giữa các lần tấn công")]
    public float attackCooldown = 2f;
    
    private Animator animator;
    private BossPhuController bossAI;
    private float startTime;
    private bool hasFired = false;
    
    public override void OnStart()
    {
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossPhuController>();
        startTime = Time.time;
        hasFired = false;
        
        if (animator != null)
        {
            // Random giữa Attack1 và Attack2 cho tấn công tầm xa
            string[] rangedAttacks = {"Attack1", "Attack2"};
            string selectedAttack = rangedAttacks[Random.Range(0, rangedAttacks.Length)];
            animator.SetTrigger(selectedAttack);
            //Debug.Log($"Boss thực hiện {selectedAttack}");
        }
        
        if (bossAI != null)
        {
            bossAI.StartAttack();
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        float elapsedTime = Time.time - startTime;
        
        // Bắn đạn sau delay
        if (!hasFired && elapsedTime >= shootDelay)
        {
            if (bossAI != null)
            {
                bossAI.FireBullet();
            }
            hasFired = true;
        }
        
        // Hoàn thành sau attackDuration
        if (elapsedTime >= attackDuration)
        {
            if (bossAI != null)
            {
                bossAI.EndAttack();
            }
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}