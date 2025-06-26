using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Boss2/Actions")]
public class Boss2Wait : Action
{
    [Tooltip("Thời gian chờ")]
    public float waitTime = 2f;
    
    [Tooltip("Thời gian chờ ngẫu nhiên (min, max)")]
    public Vector2 randomWaitRange = new Vector2(1f, 3f);
    
    [Tooltip("Sử dụng thời gian chờ ngẫu nhiên")]
    public bool useRandomWait = false;
    
    private float startTime;
    private float actualWaitTime;
    
    public override void OnStart()
    {
        startTime = Time.time;
        
        if (useRandomWait)
        {
            actualWaitTime = Random.Range(randomWaitRange.x, randomWaitRange.y);
        }
        else
        {
            actualWaitTime = waitTime;
        }
        
        Debug.Log($"Boss2 chờ {actualWaitTime:F1} giây");
    }
    
    public override TaskStatus OnUpdate()
    {
        if (Time.time - startTime >= actualWaitTime)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}