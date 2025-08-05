using UnityEngine;

/// <summary>
/// Interface cho tất cả các loại Boss có thể reset
/// </summary>
public interface IBossResettable
{
    /// <summary>
    /// Reset boss về trạng thái ban đầu
    /// </summary>
    void ResetBoss();
    
    /// <summary>
    /// Kiểm tra xem boss có đang active/spawned không
    /// </summary>
    bool IsActive();
    
    /// <summary>
    /// Lấy tên boss để debug
    /// </summary>
    string GetBossName();
}