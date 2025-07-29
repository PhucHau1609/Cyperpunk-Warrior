using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInShootRange : Conditional
{
    public float shootRange = 3f;
    public SharedTransform player;
    private PlayerShader playerShader; // Thêm reference

    public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                player.Value = playerGO.transform;
                playerShader = playerGO.GetComponentInChildren<PlayerShader>(); // Lấy PlayerShader
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (player == null || player.Value == null)
            return TaskStatus.Failure;

        // Kiểm tra tàng hình - nếu tàng hình thì trả về Failure
        if (playerShader != null && playerShader.IsInvisible())
        {
            return TaskStatus.Failure;
        }

        float dist = Vector2.Distance(transform.position, player.Value.position);
        return dist <= shootRange ? TaskStatus.Success : TaskStatus.Failure;
    }
}