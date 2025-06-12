using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInShootRange : Conditional
{
    public float shootRange = 3f;
    public SharedTransform player; // không dùng EnemyController nữa

     public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                player.Value = playerGO.transform;
            else
                Debug.LogWarning("[FacePlayer] Không tìm thấy Player với tag!");
        }
    }
    public override TaskStatus OnUpdate()
    {
        if (player == null || player.Value == null)
            return TaskStatus.Failure;

        float dist = Vector2.Distance(transform.position, player.Value.position);
        return dist <= shootRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}
