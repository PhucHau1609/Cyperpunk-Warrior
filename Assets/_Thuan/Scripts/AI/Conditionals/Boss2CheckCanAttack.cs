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

        return bossController.CanAttack() ? TaskStatus.Success : TaskStatus.Failure;
    }
}
