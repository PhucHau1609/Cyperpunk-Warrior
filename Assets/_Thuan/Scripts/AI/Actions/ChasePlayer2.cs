using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ChasePlayer2 : Action
{
    private SpiderEnemyController enemy;

    public override void OnStart()
    {
        enemy = GetComponent<SpiderEnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null)
            return TaskStatus.Failure;

        enemy.animator.SetBool("Run", true);

        Vector2 dir = enemy.player.position - transform.position;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;

        transform.position = Vector2.MoveTowards(transform.position, enemy.player.position, enemy.moveSpeed * Time.deltaTime);
        return TaskStatus.Running;
    }
}
