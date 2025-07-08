using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Boss2/Actions")]
public class Boss2RangedAttack : Action
{
    public float attackDelay = 0.5f;
    public float attackDuration = 3f;

    private Boss2Controller boss2Controller;
    private float startTime;
    private bool hasStartedAttackAnimation = false;

   public override void OnStart()
    {
        boss2Controller = GetComponent<Boss2Controller>();
        startTime = Time.time;
        hasStartedAttackAnimation = false;

        if (boss2Controller != null)
        {
            // Debug thêm thông tin về Shield
            bool shieldActive = boss2Controller.IsShieldActive();
            Debug.Log($"Boss2RangedAttack: Bắt đầu attack - Shield Active: {shieldActive}");
            
            boss2Controller.StartAttack();
        }
        else
        {
            Debug.LogError("Boss2RangedAttack: Không tìm thấy Boss2Controller!");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (boss2Controller == null) return TaskStatus.Failure;

        float elapsedTime = Time.time - startTime;

        // Chờ qua attackDelay thì mới coi như animation bắt đầu
        if (!hasStartedAttackAnimation && elapsedTime >= attackDelay)
        {
            hasStartedAttackAnimation = true;
            Debug.Log("Boss2RangedAttack: Đã qua delay, đang attack...");
        }

        // Khi đủ thời gian attackDuration thì kết thúc
        if (elapsedTime >= attackDuration)
        {
            boss2Controller.EndAttack();
            Debug.Log("Boss2RangedAttack: Kết thúc attack!");
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        if (boss2Controller != null)
        {
            boss2Controller.EndAttack();
        }
    }
}
