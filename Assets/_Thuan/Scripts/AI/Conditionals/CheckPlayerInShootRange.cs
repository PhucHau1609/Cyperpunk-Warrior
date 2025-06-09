using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInShootRange : Conditional
{
    public float shootRange = 3f;
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
        //Debug.Log($"[CheckShootRange] Distance: {dist}, Success: {dist <= shootRange}");
        return dist <= shootRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
