using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class IsBombDropperPlayerUnderneath : Conditional
{
    private BombDropperController controller;

    public override void OnAwake()
    {
        // Tự động gán controller
        controller = GetComponent<BombDropperController>();
        if (controller == null)
        {
            Debug.LogError($"[{gameObject.name}] IsBombDropperPlayerUnderneath: BombDropperController not found!");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (controller == null)
            return TaskStatus.Failure;

        // Sử dụng method từ controller để check player underneath
        if (controller.IsPlayerUnderneath())
            return TaskStatus.Success;

        return TaskStatus.Failure;
    }
}