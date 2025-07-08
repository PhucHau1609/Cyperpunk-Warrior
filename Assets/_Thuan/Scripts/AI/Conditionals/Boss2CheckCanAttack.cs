using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Boss2/Conditionals")]
public class Boss2CheckCanAttack : Conditional
{
    private Boss2Controller bossController;

    public override void OnStart()
    {
        bossController = GetComponent<Boss2Controller>();
    }

    public override TaskStatus OnUpdate()
    {
        if (bossController == null)
        {
            Debug.LogError("Boss2CheckCanAttack: Không tìm thấy Boss2Controller!");
            return TaskStatus.Failure;
        }

        bool canAttack = bossController.CanAttack();
        
        // Debug để theo dõi
        if (!canAttack)
        {
            string reason = "";
            if (bossController.isAttacking) reason += "Boss đang tấn công; ";
            if (bossController.IsAnyHandOrBossAttacking()) reason += "Hand đang tấn công; ";
            if (Time.time - bossController.lastAttackTime < 3f) reason += "Chưa đủ cooldown; ";
            
            Debug.Log($"Boss2CheckCanAttack: Không thể tấn công - {reason}");
        }
        else
        {
            Debug.Log("Boss2CheckCanAttack: Có thể tấn công!");
        }

        return canAttack ? TaskStatus.Success : TaskStatus.Failure;
    }
}
