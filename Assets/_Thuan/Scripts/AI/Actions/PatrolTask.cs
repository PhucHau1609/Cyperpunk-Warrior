using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

public class PatrolTask : Action 
{
    public enum MoveMode { TransformPosition, RigidbodyVelocity }
    
    [Header("Setup")] 
    public MoveMode movementType = MoveMode.TransformPosition;
    
    [Header("Auto Reference Settings")]
    [Tooltip("Prefix để tìm patrol points (vd: Point, PatrolPoint)")]
    public string pointPrefix = "Point";
    
    [Header("Manual Override (optional)")]
    public SharedTransform pointA; 
    public SharedTransform pointB; 
    
    [Header("Movement")]
    public SharedFloat moveSpeed; 
    public SharedAnimator animator;
    
    private Transform currentTarget; 
    private Rigidbody2D rb;
    private Transform autoPointA;
    private Transform autoPointB;
    
    public override void OnStart() 
    {
        // Tự động tìm patrol points nếu không được gán manual
        if (pointA.Value == null || pointB.Value == null)
        {
            FindPatrolPoints();
        }
        
        // Sử dụng auto points nếu manual points không có
        Transform startPoint = pointA.Value != null ? pointA.Value : autoPointA;
        currentTarget = startPoint;
        
        if (movementType == MoveMode.RigidbodyVelocity)
            rb = GetComponent<Rigidbody2D>(); 
    }
    
    private void FindPatrolPoints()
    {
        // Lấy tên của Enemy để xác định index
        string enemyName = gameObject.name;
        string enemyIndex = ExtractEnemyIndex(enemyName);
        
        // Tìm các patrol points với naming convention
        string pointAName = $"{pointPrefix}A{enemyIndex}";
        string pointBName = $"{pointPrefix}B{enemyIndex}";
        
        // Tìm trong scene
        GameObject pointAObj = GameObject.Find(pointAName);
        GameObject pointBObj = GameObject.Find(pointBName);
        
        if (pointAObj != null) autoPointA = pointAObj.transform;
        if (pointBObj != null) autoPointB = pointBObj.transform;
        
        // Log để debug
        if (autoPointA == null || autoPointB == null)
        {
            Debug.LogWarning($"[{enemyName}] Không tìm thấy patrol points: {pointAName}, {pointBName}");
            
            // Fallback: tìm points không có suffix
            if (autoPointA == null)
            {
                pointAObj = GameObject.Find($"{pointPrefix}A");
                if (pointAObj != null) autoPointA = pointAObj.transform;
            }
            if (autoPointB == null)
            {
                pointBObj = GameObject.Find($"{pointPrefix}B");
                if (pointBObj != null) autoPointB = pointBObj.transform;
            }
        }
    }
    
    private string ExtractEnemyIndex(string enemyName)
    {
        string index = "";
        
        // Tìm số ở cuối tên
        for (int i = enemyName.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(enemyName[i]))
            {
                index = enemyName[i] + index;
            }
            else
            {
                break;
            }
        }
        
        // Trả về với underscore prefix nếu có số
        return string.IsNullOrEmpty(index) ? "" : "_" + index;
    }
    
    public override TaskStatus OnUpdate() 
    {
        // Sử dụng points (manual override hoặc auto-found)
        Transform activePointA = pointA.Value != null ? pointA.Value : autoPointA;
        Transform activePointB = pointB.Value != null ? pointB.Value : autoPointB;
        
        if (activePointA == null || activePointB == null) 
            return TaskStatus.Failure;
        
        Vector3 targetPos = currentTarget.position; 
        float dist = Vector2.Distance(transform.position, targetPos); 
        Vector2 dir = (targetPos - transform.position).normalized;
        
        switch (movementType) 
        { 
            case MoveMode.TransformPosition: 
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed.Value * Time.deltaTime); 
                break;
                
            case MoveMode.RigidbodyVelocity: 
                if (rb == null) 
                { 
                    rb = GetComponent<Rigidbody2D>(); 
                    if (rb == null) return TaskStatus.Failure; 
                }
                
                rb.linearVelocity = dir * moveSpeed.Value; 
                break; 
        }
        
        // Flip sprite
        if (dir.x != 0) 
        { 
            Vector3 scale = transform.localScale; 
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x); 
            transform.localScale = scale; 
        }
        
        animator?.Value?.SetBool("Run", true); 
        animator?.Value?.SetBool("Fly", movementType == MoveMode.RigidbodyVelocity);
        
        // Đổi target khi đến nơi
        if (dist < 0.2f) 
        { 
            currentTarget = (currentTarget == activePointA) ? activePointB : activePointA; 
        }
        
        return TaskStatus.Running; 
    }
    
    public override void OnEnd() 
    { 
        if (movementType == MoveMode.RigidbodyVelocity && rb != null) 
        { 
            rb.linearVelocity = Vector2.zero; 
        }
        
        animator?.Value?.SetBool("Run", false); 
        animator?.Value?.SetBool("Fly", false); 
    } 
}