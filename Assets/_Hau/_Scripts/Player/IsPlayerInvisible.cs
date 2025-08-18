using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class IsPlayerInvisible : Conditional
{
    private PlayerShader playerShader;

    public override void OnStart()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerShader = player.GetComponentInChildren<PlayerShader>();
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (playerShader != null && playerShader.IsInvisible())
        {
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
    }
}
