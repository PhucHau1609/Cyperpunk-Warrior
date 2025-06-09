using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FacePlayer : Action
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

        Vector2 dir = enemy.player.position - enemy.transform.position;
        if (dir.x != 0)
        {
            Vector3 scale = enemy.transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            enemy.transform.localScale = scale;
        }

        return TaskStatus.Success;
    }
}
