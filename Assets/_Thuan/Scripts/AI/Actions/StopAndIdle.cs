using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class StopAndIdle : Action
{
    [Header("Auto-assigned")]
    public SharedAnimator animator;
    
    private Rigidbody2D rb;

    public override void OnAwake()
    {
        // Tự động gán animator nếu chưa được gán
        if (animator.Value == null)
        {
            Animator animatorComponent = GetComponent<Animator>();
            if (animatorComponent != null)
            {
                animator.Value = animatorComponent;
            }
        }
    }

    public override void OnStart()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Dừng di chuyển
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Set animation về Idle
        if (animator != null && animator.Value != null)
        {
            animator.Value.SetBool("Run", false);
        }
    }

    public override TaskStatus OnUpdate()
    {
        // Giữ trạng thái dừng
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        return TaskStatus.Success;
    }

    public override void OnEnd()
    {
        // Không cần làm gì đặc biệt khi kết thúc
    }
}