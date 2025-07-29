using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerVisible : Conditional
{
    public SharedTransform player;
    private PlayerShader playerShader;

    public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                player.Value = playerGO.transform;
                playerShader = playerGO.GetComponentInChildren<PlayerShader>();
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (player == null || player.Value == null || playerShader == null)
            return TaskStatus.Failure;

        // Return Success nếu Player KHÔNG tàng hình
        return !playerShader.IsInvisible() ? TaskStatus.Success : TaskStatus.Failure;
    }
}