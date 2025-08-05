using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss2/Actions")]
public class Boss2Idle : Action
{
    [Tooltip("Thời gian idle")]
    public float idleTime = 1f;
    
    private Animator animator;
    private float startTime;
    
    public override void OnStart()
    {
        animator = GetComponent<Animator>();
        startTime = Time.time;
        
        if (animator != null)
        {
            // Reset về trạng thái Idle
            animator.SetTrigger("Idle");
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (Time.time - startTime >= idleTime)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}