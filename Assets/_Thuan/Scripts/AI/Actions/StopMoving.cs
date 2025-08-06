using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class StopMoving : Action
{
    private Rigidbody2D rb;
    private Animator animator;

    public override void OnStart()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator?.SetBool("Run", false);
    }

    public override TaskStatus OnUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        return TaskStatus.Success;
    }
}
