using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss/Actions")]
public class BossMeleeAttack : Action
{
    [Tooltip("Thời gian hoàn thành attack")]
    public float attackDuration = 1.5f;
    
    [Tooltip("Cooldown giữa các lần tấn công")]
    public float attackCooldown = 2f;
    
    private Animator animator;
    private BossPhuController bossAI;
    private float startTime;
    
    public override void OnStart()
    {
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossPhuController>();
        startTime = Time.time;
        
        if (animator != null)
        {
            // Random giữa Attack3 và Attack4 cho tấn công cận chiến
            string[] meleeAttacks = {"Attack3", "Attack4"};
            string selectedAttack = meleeAttacks[Random.Range(0, meleeAttacks.Length)];
            animator.SetTrigger(selectedAttack);
            Debug.Log($"Boss thực hiện {selectedAttack}");
        }
        
        if (bossAI != null)
        {
            bossAI.StartAttack();
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        float elapsedTime = Time.time - startTime;
        
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