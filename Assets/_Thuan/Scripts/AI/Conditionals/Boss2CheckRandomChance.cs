using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss2/Conditionals")]
public class Boss2CheckRandomChance : Conditional
{
    [Tooltip("Tỷ lệ thành công (0-100%)")]
    [Range(0f, 100f)]
    public float successChance = 50f;
    
    [Tooltip("Tính toán lại mỗi lần Update")]
    public bool reevaluateEachUpdate = false;
    
    private bool? cachedResult = null;
    
    public override void OnStart()
    {
        if (!reevaluateEachUpdate)
        {
            cachedResult = null;
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!reevaluateEachUpdate && cachedResult.HasValue)
        {
            return cachedResult.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
        
        bool result = Random.Range(0f, 100f) < successChance;
        
        if (!reevaluateEachUpdate)
        {
            cachedResult = result;
        }
        
        return result ? TaskStatus.Success : TaskStatus.Failure;
    }
}