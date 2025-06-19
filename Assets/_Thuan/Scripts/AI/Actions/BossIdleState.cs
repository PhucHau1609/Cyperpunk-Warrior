using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss/Actions")]
public class BossIdleState : Action
{
    [Tooltip("Thời gian idle")]
    public float idleTime = 1f;
    
    private float startTime;
    private Animator animator;
    private Rigidbody2D rb;
    
    public override void OnStart()
    {
        startTime = Time.time;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Dừng di chuyển và chuyển về trạng thái idle
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        float elapsedTime = Time.time - startTime;
        
        if (elapsedTime >= idleTime)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}