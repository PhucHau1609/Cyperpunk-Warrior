using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInTargetRange2 : Conditional
{
    public float targetRange = 6f;
    private SpiderEnemyController enemy;

    public override void OnStart()
    {
        enemy = GetComponent<SpiderEnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (enemy == null || enemy.player == null)
            return TaskStatus.Failure;

        float dist = Vector2.Distance(transform.position, enemy.player.position);
        return dist <= targetRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
