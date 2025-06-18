using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss/Actions")]
public class BossStopMoving : Action
{
    private Rigidbody2D rb;
    private Animator animator;
    
    public override void OnStart()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Giữ gravity
        }
        
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
        }
        
        return TaskStatus.Success;
    }
}