using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInTargetRange : Conditional
{
    public float targetRange = 6f;
    private EnemyController enemy;

    public override void OnStart()
    {
        enemy = GetComponent<EnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null)
            return TaskStatus.Failure;

        float dist = Vector2.Distance(transform.position, enemy.player.position);
        return dist <= targetRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
