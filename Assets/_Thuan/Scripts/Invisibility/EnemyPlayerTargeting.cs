using UnityEngine;

public class EnemyPlayerTargeting : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Có cho phép Enemy nhìn thấy Player khi tàng hình không")]
    public bool canSeeInvisiblePlayer = false;
    
    [Tooltip("Khoảng cách tối đa có thể nhìn thấy Player tàng hình")]
    public float invisibleDetectionRange = 10f;
    
    // Cache
    private Transform cachedPlayer;
    private PlayerShader playerShader;
    
    // Static reference cho performance
    private static Transform staticPlayerTransform;
    private static PlayerShader staticPlayerShader;
    
    void Start()
    {
        InitializePlayerReferences();
    }
    
    void InitializePlayerReferences()
    {
        // Chỉ tìm Player một lần cho tất cả Enemies
        if (staticPlayerTransform == null)
        {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                staticPlayerTransform = playerGO.transform;
                staticPlayerShader = playerGO.GetComponent<PlayerShader>();
                
                if (staticPlayerShader == null)
                {
                    Debug.LogWarning("Player không có component PlayerShader!");
                }
            }
        }
        
        cachedPlayer = staticPlayerTransform;
        playerShader = staticPlayerShader;
    }
    
    /// <summary>
    /// Hàm chính để kiểm tra có thể target Player không
    /// </summary>
    public bool CanTargetPlayer()
    {
        if (cachedPlayer == null || playerShader == null)
            return false;
        
        // Nếu Player không tàng hình, có thể target bình thường
        if (!playerShader.IsInvisible())
            return true;
        
        // Nếu Player tàng hình và Enemy không thể nhìn thấy, return false
        if (!canSeeInvisiblePlayer)
            return false;
        
        // Nếu Enemy có thể nhìn thấy Player tàng hình, check khoảng cách
        float distanceToPlayer = Vector2.Distance(transform.position, cachedPlayer.position);
        return distanceToPlayer <= invisibleDetectionRange;
    }
    
    /// <summary>
    /// Lấy Player Transform nếu có thể target được
    /// </summary>
    public Transform GetTargetablePlayer()
    {
        return CanTargetPlayer() ? cachedPlayer : null;
    }
    
    /// <summary>
    /// Kiểm tra Player có trong tầm bắn không
    /// </summary>
    public bool IsPlayerInRange(float range)
    {
        if (!CanTargetPlayer())
            return false;
            
        float distance = Vector2.Distance(transform.position, cachedPlayer.position);
        return distance <= range;
    }
    
    /// <summary>
    /// Lấy hướng tới Player (nếu có thể target)
    /// </summary>
    public Vector2 GetDirectionToPlayer()
    {
        if (!CanTargetPlayer())
            return Vector2.zero;
            
        return (cachedPlayer.position - transform.position).normalized;
    }
    
    /// <summary>
    /// Lấy khoảng cách tới Player (nếu có thể target)
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (!CanTargetPlayer())
            return float.MaxValue;
            
        return Vector2.Distance(transform.position, cachedPlayer.position);
    }
    
    /// <summary>
    /// Update lại Player reference (gọi khi cần thiết)
    /// </summary>
    public void RefreshPlayerReference()
    {
        InitializePlayerReferences();
    }
    
    /// <summary>
    /// Static method để reset references khi scene thay đổi
    /// </summary>
    public static void ResetStaticReferences()
    {
        staticPlayerTransform = null;
        staticPlayerShader = null;
    }
}