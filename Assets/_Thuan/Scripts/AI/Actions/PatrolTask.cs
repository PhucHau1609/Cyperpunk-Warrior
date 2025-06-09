using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class PatrolTask : Action
{
    private EnemyController enemy;
    private Transform currentTarget;

    public override void OnStart()
    {
        enemy = GetComponent<EnemyController>();
        currentTarget = enemy.pointA;
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null)
            return TaskStatus.Failure;

        enemy.animator.SetBool("Run", true);

        Vector3 targetPos = currentTarget.position;
        float dist = Vector2.Distance(transform.position, targetPos);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(targetPos.x - transform.position.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, enemy.moveSpeed * Time.deltaTime);

        if (dist < 0.1f)
        {
            // Chuyển điểm tuần tra
            currentTarget = (currentTarget == enemy.pointA) ? enemy.pointB : enemy.pointA;
        }

        return TaskStatus.Running;
    }
}
