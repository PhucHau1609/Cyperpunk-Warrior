using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class StopMoving : Action
{
    private EnemyController enemy;
    private Rigidbody2D rb;

    public override void OnStart()
    {
        enemy = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        enemy.animator.SetBool("Run", false);
    }

    public override TaskStatus OnUpdate()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        return TaskStatus.Success;
    }
}
