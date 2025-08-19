using UnityEngine;
using System.Collections.Generic;

public class CoreManager : MonoBehaviour
{
    public List<CoreZone> cores; // gán trong Inspector theo thứ tự lõi 1 → 2 → 3
    private int currentCoreIndex = 0;
    private SceneController sceneController;
    
    // Lưu trữ trạng thái ban đầu để reset
    private int initialCoreIndex = 0;
    
    // QUAN TRỌNG: Lưu trữ progress để không mất khi respawn
    private List<bool> coreCompletionStatus = new List<bool>();

    void Start()
    {
        sceneController = FindAnyObjectByType<SceneController>();
        initialCoreIndex = 0; // Luôn bắt đầu từ core đầu tiên

        // Khởi tạo trạng thái completion cho tất cả cores
        InitializeCoreCompletionStatus();

        // Khóa tất cả lõi trừ lõi đầu tiên
        ResetCoreManager();
    }

    // Method để khởi tạo trạng thái completion
    private void InitializeCoreCompletionStatus()
    {
        coreCompletionStatus.Clear();
        for (int i = 0; i < cores.Count; i++)
        {
            coreCompletionStatus.Add(false); // Tất cả cores chưa hoàn thành
        }
        Debug.Log($"[CoreManager] Initialized completion status for {cores.Count} cores");
    }

    public void MarkCoreAsComplete(CoreZone completedCore)
    {
        Debug.Log($"[CoreManager] MarkCoreAsComplete called for core index {currentCoreIndex}");
        
        // QUAN TRỌNG: Đánh dấu core hiện tại đã hoàn thành
        if (currentCoreIndex < coreCompletionStatus.Count)
        {
            coreCompletionStatus[currentCoreIndex] = true;
            Debug.Log($"[CoreManager] Marked core {currentCoreIndex} as completed");
        }

        currentCoreIndex++;

        if (currentCoreIndex < cores.Count)
        {
            // Mở lõi tiếp theo
            Debug.Log($"[CoreManager] Activating next core: index {currentCoreIndex}");
            cores[currentCoreIndex].SetActiveLogic(true);
        }
        else
        {
            // Gỡ xong tất cả
            Debug.Log("[CoreManager] All cores completed! Returning control to player");
            sceneController?.ReturnControlToPlayer(true); // true = hoàn thành mini game
            
            // QUAN TRỌNG: Đánh dấu mini game đã hoàn thành
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.miniGameCompleted = true;
                Debug.Log("[CoreManager] Mini game completion status set to true");
            }
        }
    }
    
    // Method cũ để reset hoàn toàn (chỉ dùng khi bắt đầu mini game lần đầu)
    public void ResetCoreManager()
    {
        Debug.Log("[CoreManager] Full reset to initial state...");
        
        // Reset hoàn toàn
        currentCoreIndex = initialCoreIndex;
        InitializeCoreCompletionStatus(); // Reset tất cả completion status
        
        // Reset tất cả cores
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                cores[i].ResetCoreForMiniGame();
            }
        }
        
        // Delay việc set active logic
        StartCoroutine(DelayedSetActiveLogic());
    }
    
    // Method mới để reset mà giữ lại progress
    public void ResetCoreManagerKeepProgress()
    {
        Debug.Log("[CoreManager] Resetting while keeping progress...");
        Debug.Log($"[CoreManager] Current progress: Core index {currentCoreIndex}");
        
        // QUAN TRỌNG: KHÔNG reset currentCoreIndex và completion status
        // Chỉ reset trạng thái visual/physical của cores
        
        // Reset tất cả cores về trạng thái visual ban đầu
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                cores[i].ResetCoreForMiniGame();
            }
        }
        
        // Delay việc restore progress
        StartCoroutine(DelayedRestoreProgress());
    }
    
    // Coroutine để restore progress sau khi reset
    private System.Collections.IEnumerator DelayedRestoreProgress()
    {
        yield return new WaitForSeconds(1.5f); // Đợi cores reset xong
        
        Debug.Log("[CoreManager] Restoring progress...");
        
        // Restore trạng thái dựa trên completion status
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                if (i < currentCoreIndex && coreCompletionStatus[i])
                {
                    // Core đã hoàn thành - set as completed và mở cửa
                    cores[i].SetAsCompleted();
                    Debug.Log($"[CoreManager] Restored completed core {i}");
                }
                else if (i == currentCoreIndex)
                {
                    // Core hiện tại - set active
                    cores[i].SetActiveLogic(true);
                    Debug.Log($"[CoreManager] Restored active core {i}");
                }
                else
                {
                    // Core chưa tới - keep inactive
                    cores[i].SetActiveLogic(false);
                }
            }
        }
        
        Debug.Log($"[CoreManager] Progress restoration completed. Current active core: {currentCoreIndex}");
    }
    
    // Coroutine để delay việc set active logic (cho reset hoàn toàn)
    private System.Collections.IEnumerator DelayedSetActiveLogic()
    {
        yield return new WaitForSeconds(1.5f); // Đợi cores reset xong
        
        // Set active logic cho reset hoàn toàn
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                cores[i].SetActiveLogic(i == currentCoreIndex);
            }
        }
        
        Debug.Log($"[CoreManager] Full reset completed. Current active core index: {currentCoreIndex}");
    }
    
    // Method để get current core index
    public int GetCurrentCoreIndex()
    {
        return currentCoreIndex;
    }
    
    // Method để get completion status
    public bool IsCoreCompleted(int index)
    {
        if (index >= 0 && index < coreCompletionStatus.Count)
        {
            return coreCompletionStatus[index];
        }
        return false;
    }
    
    // Method để check tổng progress
    public float GetCompletionPercentage()
    {
        if (cores.Count == 0) return 0f;
        
        int completedCount = 0;
        for (int i = 0; i < coreCompletionStatus.Count; i++)
        {
            if (coreCompletionStatus[i]) completedCount++;
        }
        
        return (float)completedCount / cores.Count * 100f;
    }
    
    // Method để debug progress
    public void DebugProgress()
    {
        Debug.Log($"[CoreManager] === PROGRESS DEBUG ===");
        Debug.Log($"Current Core Index: {currentCoreIndex}");
        Debug.Log($"Total Cores: {cores.Count}");
        Debug.Log($"Completion Percentage: {GetCompletionPercentage():F1}%");
        
        for (int i = 0; i < coreCompletionStatus.Count; i++)
        {
            string status = coreCompletionStatus[i] ? "COMPLETED" : "NOT COMPLETED";
            string active = (i == currentCoreIndex) ? " [ACTIVE]" : "";
            Debug.Log($"  Core {i}: {status}{active}");
        }
        Debug.Log($"[CoreManager] === END DEBUG ===");
    }
}