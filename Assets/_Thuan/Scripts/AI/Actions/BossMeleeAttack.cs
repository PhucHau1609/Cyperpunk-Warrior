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

    [Tooltip("Delay trước khi phát âm thanh tấn công")]
    public float audioDelay = 0.3f;
    
    private Animator animator;
    private BossPhuController bossAI;
    private BossPhuAttackHitDetector hitDetector;
    private float startTime;
    private bool hasPlayedAudio = false;
    
    public override void OnStart()
    {
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossPhuController>();
        hitDetector = GetComponent<BossPhuAttackHitDetector>();
        startTime = Time.time;
        hasPlayedAudio = false;

        if (animator != null)
        {
            // Bao gồm tất cả 4 loại tấn công cận chiến
            string[] meleeAttacks = {"Attack3", "Attack4" };
            string selectedAttack = meleeAttacks[Random.Range(0, meleeAttacks.Length)];
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
        
        if (!hasPlayedAudio && elapsedTime >= audioDelay)
        {
            if (bossAI != null)
            {
                bossAI.PlayMeleeAttackSound();
            }
            hasPlayedAudio = true;
        }

        if (elapsedTime >= attackDuration)
        {
            if (bossAI != null)
            {
                bossAI.EndAttack();
            }
            
            // Đảm bảo reset damage detector
            if (hitDetector != null)
            {
                hitDetector.OnAttackEnd();
            }
            
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}