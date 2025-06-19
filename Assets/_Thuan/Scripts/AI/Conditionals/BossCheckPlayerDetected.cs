using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss/Conditionals")]
public class BossCheckPlayerDetected : Conditional
{
    [Tooltip("Transform của Player")]
    public SharedTransform player;
    
    [Tooltip("Player đã được phát hiện chưa")]
    public SharedBool playerDetected;
    
    [Tooltip("Boss còn sống không")]
    public SharedBool isDead;
    private Transform playerTransform;
    
    // Biến trạng thái do script quản lý
    [SerializeField] private bool isPlayerDetected = false;
    [SerializeField] private bool isBossDead = false;

    public override void OnStart()
    {
        FindPlayer();
    }
    private void FindPlayer()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        isPlayerDetected = playerTransform != null;
    }
    public override TaskStatus OnUpdate()
    {
        if (isBossDead) return TaskStatus.Failure;
        
        if (playerTransform != null && isPlayerDetected)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}