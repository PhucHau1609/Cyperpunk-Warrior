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
            return TaskStatus.Failure;
        }

        bool canAttack = bossController.CanAttack();
        
        if (!canAttack)
        {
            string reason = "";
            if (bossController.isAttacking) reason += "Boss đang tấn công; ";
            if (bossController.IsAnyHandOrBossAttacking()) reason += "Hand đang tấn công; ";
            if (Time.time - bossController.lastAttackTime < 3f) reason += "Chưa đủ cooldown; ";
        }

        return canAttack ? TaskStatus.Success : TaskStatus.Failure;
    }
}
