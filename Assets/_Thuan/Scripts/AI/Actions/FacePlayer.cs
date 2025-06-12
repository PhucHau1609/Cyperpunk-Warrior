using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class FacePlayer : Action
{
    public SharedTransform player;

    public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                player.Value = playerGO.transform;
            else
                Debug.LogWarning("[CheckPlayerInShootRange] Không tìm thấy Player với tag!");
        }
    }
    public override TaskStatus OnUpdate()
    {
        if (player.Value == null)
            return TaskStatus.Failure;

        Vector2 dir = player.Value.position - transform.position;

        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        return TaskStatus.Success;
    }
}
