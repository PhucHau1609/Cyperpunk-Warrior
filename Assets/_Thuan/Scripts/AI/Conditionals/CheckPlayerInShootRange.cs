/*using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInShootRange : Conditional
{
    [Header("Line of Sight Settings")]
    public float shootRange = 8f;              // Khoảng cách tối đa
    public float lineWidth = 2f;                // Độ rộng của đường thẳng (tolerance)
    public LayerMask obstacleLayer = -1;        // Layer của vật cản (wall, obstacles)
    public bool drawDebugRay = true;            // Hiển thị debug ray
    
    public SharedTransform player;
    private PlayerShader playerShader;

    public override void OnStart()
    {
        if (player == null || player.Value == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                player.Value = playerGO.transform;
                playerShader = playerGO.GetComponentInChildren<PlayerShader>();
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (player == null || player.Value == null)
            return TaskStatus.Failure;

        // Kiểm tra tàng hình
        if (playerShader != null && playerShader.IsInvisible())
        {
            return TaskStatus.Failure;
        }

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.Value.position;
        
        // Kiểm tra khoảng cách
        float distance = Vector2.Distance(enemyPos, playerPos);
        if (distance > shootRange)
        {
            return TaskStatus.Failure;
        }

        // Kiểm tra xem player có nằm trên đường thẳng ngang không
        bool isInLineOfSight = CheckLineOfSight(enemyPos, playerPos);
        
        if (drawDebugRay)
        {
            DrawDebugLine(enemyPos, playerPos, isInLineOfSight);
        }

        return isInLineOfSight ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool CheckLineOfSight(Vector2 enemyPos, Vector2 playerPos)
    {
        // Kiểm tra độ chênh lệch theo trục Y (để xác định có nằm trên đường thẳng ngang không)
        float yDifference = Mathf.Abs(playerPos.y - enemyPos.y);
        if (yDifference > lineWidth)
        {
            return false; // Player không nằm trong "đường thẳng"
        }

        // Kiểm tra xem có vật cản giữa Enemy và Player không
        Vector2 direction = (playerPos - enemyPos).normalized;
        float distance = Vector2.Distance(enemyPos, playerPos);
        
        // Raycast để kiểm tra vật cản
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, direction, distance, obstacleLayer);
        
        // Nếu không có vật cản hoặc vật cản là chính Player thì OK
        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    private void DrawDebugLine(Vector2 enemyPos, Vector2 playerPos, bool canSee)
    {
        Color lineColor = canSee ? Color.green : Color.red;
        Debug.DrawLine(enemyPos, playerPos, lineColor, 0.1f);
        
        // Vẽ vùng "đường thẳng" (line width)
        Vector2 upOffset = Vector2.up * (lineWidth / 2f);
        Debug.DrawLine(enemyPos + upOffset, enemyPos + Vector2.right * shootRange + upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos - upOffset, enemyPos + Vector2.right * shootRange - upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos + upOffset, enemyPos + Vector2.left * shootRange + upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos - upOffset, enemyPos + Vector2.left * shootRange - upOffset, Color.yellow, 0.1f);
    }
}*/

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CheckPlayerInShootRange : Conditional
{
    [Header("Line of Sight Settings")]
    public float shootRange = 8f;
    public float lineWidth = 2f;
    public LayerMask obstacleLayer;
    public bool drawDebugRay = true;

    public SharedTransform player;
    private PlayerShader playerShader;

    // ==== Subscriptions ====
    public override void OnAwake()
    {
        // đăng ký lắng nghe khi Player được set / đổi
        PlayerLocator.OnChanged += HandlePlayerChanged;
    }

    public override void OnStart()
    {
        TryResolvePlayer(); // lần đầu vào tree
    }

    public override void OnEnd()
    {
        PlayerLocator.OnChanged -= HandlePlayerChanged;
    }

    private void HandlePlayerChanged(Transform t)
    {
        if (t == null) return;
        player.Value = t;
        playerShader = t.GetComponentInChildren<PlayerShader>();
    }

    private void TryResolvePlayer()
    {
        // Ưu tiên PlayerLocator
        var t = PlayerLocator.Current;
        if (t == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) t = go.transform;
        }

        if (t != null)
        {
            player.Value = t;
            playerShader = t.GetComponentInChildren<PlayerShader>();
        }
    }

    public override TaskStatus OnUpdate()
    {
        // nếu mất tham chiếu (player bị destroy khi đổi scene hoặc vừa respawn) → thử lấy lại
        if (player == null || player.Value == null || !player.Value.gameObject.activeInHierarchy)
        {
            TryResolvePlayer();
            if (player == null || player.Value == null) return TaskStatus.Failure;
        }

        // Tàng hình?
        if (playerShader != null && playerShader.IsInvisible())
            return TaskStatus.Failure;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.Value.position;

        // Khoảng cách
        float distance = Vector2.Distance(enemyPos, playerPos);
        if (distance > shootRange) return TaskStatus.Failure;

        // LOS theo "hành lang ngang" + raycast chướng ngại
        bool isInLineOfSight = CheckLineOfSight(enemyPos, playerPos);

        if (drawDebugRay) DrawDebugLine(enemyPos, playerPos, isInLineOfSight);

        return isInLineOfSight ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool CheckLineOfSight(Vector2 enemyPos, Vector2 playerPos)
    {
        // kiểm tra bề rộng "hành lang" theo trục Y
        float yDifference = Mathf.Abs(playerPos.y - enemyPos.y);
        if (yDifference > lineWidth) return false;

        // Raycast chỉ vào layer chướng ngại (KHÔNG gồm Player)
        Vector2 dir = (playerPos - enemyPos).normalized;
        float dist = Vector2.Distance(enemyPos, playerPos);

        // đảm bảo obstacleLayer không chứa layer của Player
        // (nếu bạn để Player ở layer "Player", hãy loại nó ra khỏi mask trong Inspector)
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, dir, dist, obstacleLayer);

        // nếu không bị chặn → thấy
        return hit.collider == null;
    }

    private void DrawDebugLine(Vector2 enemyPos, Vector2 playerPos, bool canSee)
    {
        Color lineColor = canSee ? Color.green : Color.red;
        Debug.DrawLine(enemyPos, playerPos, lineColor, 0.1f);

        Vector2 upOffset = Vector2.up * (lineWidth * 0.5f);
        Debug.DrawLine(enemyPos + upOffset, enemyPos + Vector2.right * shootRange + upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos - upOffset, enemyPos + Vector2.right * shootRange - upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos + upOffset, enemyPos + Vector2.left * shootRange + upOffset, Color.yellow, 0.1f);
        Debug.DrawLine(enemyPos - upOffset, enemyPos + Vector2.left * shootRange - upOffset, Color.yellow, 0.1f);
    }
}
