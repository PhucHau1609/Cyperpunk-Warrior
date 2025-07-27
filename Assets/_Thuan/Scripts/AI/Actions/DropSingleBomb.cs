using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DropSingleBomb : Action
{
    [Header("Auto-assigned")]
    public SharedAnimator animator;
    public SharedFloat dropInterval = new SharedFloat { Value = 1f }; // 1 giây

    private float lastDropTime;
    private bool hasDoneInitialDrop = false;

    public override void OnAwake()
    {
        // Tự động gán animator nếu chưa được gán
        if (animator.Value == null)
        {
            Animator animatorComponent = GetComponent<Animator>();
            if (animatorComponent != null)
            {
                animator.Value = animatorComponent;
            }
        }
        
        // Tự động gán dropInterval từ BombDropperController nếu có
        BombDropperController controller = GetComponent<BombDropperController>();
        if (controller != null)
        {
            dropInterval.Value = controller.dropInterval;
        }
    }

    public override void OnStart()
    {
        // Nếu là lần đầu tiên hoặc đã đủ thời gian chờ, cho phép thả bomb
        if (!hasDoneInitialDrop || Time.time >= lastDropTime + dropInterval.Value)
        {
            hasDoneInitialDrop = true;
            lastDropTime = Time.time;
            
            // Trigger animation Attack - bomb sẽ được thả qua Animation Event
            if (animator != null && animator.Value != null)
            {
                animator.Value.SetTrigger("Attack");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] DropSingleBomb: Animator not found!");
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        // Task hoàn thành ngay sau khi trigger animation
        return TaskStatus.Success;
    }

    public override void OnEnd()
    {
        // Reset để có thể thả bomb lần tiếp theo
        // Nhưng vẫn giữ thời gian để đảm bảo interval
    }
}