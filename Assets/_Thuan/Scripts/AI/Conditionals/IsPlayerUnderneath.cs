using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class IsPlayerUnderneath : Conditional
{
    private EnemyController enemy;

    public override void OnStart()
    {
        enemy = GetComponent<EnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null)
            return TaskStatus.Failure;

        float xDist = Mathf.Abs(enemy.player.position.x - transform.position.x);
        float yDiff = transform.position.y - enemy.player.position.y;

        if (xDist <= 1.5f && yDiff > 0) // khoảng dưới và nằm bên dưới
            return TaskStatus.Success;

        return TaskStatus.Failure;
    }
}
