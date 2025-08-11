using UnityEngine;
using System.Collections.Generic;

public class CoreManager : MonoBehaviour
{
    public List<CoreZone> cores; // gán trong Inspector theo thứ tự lõi 1 → 2 → 3
    private int currentCoreIndex = 0;
    private SceneController sceneController;
    
    // Lưu trữ trạng thái ban đầu để reset
    private int initialCoreIndex = 0;

    void Start()
    {
        sceneController = FindAnyObjectByType<SceneController>();
        initialCoreIndex = 0; // Luôn bắt đầu từ core đầu tiên

        // Khóa tất cả lõi trừ lõi đầu tiên
        ResetCoreManager();
    }

    public void MarkCoreAsComplete(CoreZone completedCore)
    {
        Debug.Log($"[CoreManager] MarkCoreAsComplete called for core index {currentCoreIndex}");
        
        //completedCore.SetAsCompletedVisual();

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
            sceneController?.ReturnControlToPlayer();
        }
    }
    
    // Method mới để reset CoreManager về trạng thái ban đầu
    public void ResetCoreManager()
    {
        Debug.Log("[CoreManager] Resetting to initial state...");
        
        // QUAN TRỌNG: Reset currentCoreIndex về 0 trước
        currentCoreIndex = initialCoreIndex;
        
        // Reset tất cả cores trước
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                // Reset core về trạng thái ban đầu trước
                cores[i].ResetCoreForMiniGame();
            }
        }
        
        // Sau đó mới set active logic
        for (int i = 0; i < cores.Count; i++)
        {
            if (cores[i] != null)
            {
                cores[i].SetActiveLogic(i == currentCoreIndex);
            }
        }
        
        Debug.Log($"[CoreManager] Reset completed. Current active core index: {currentCoreIndex}");
    }
}