using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public LyraHealth lyra;

    public void Retry()
    {
        Debug.Log("[GameOverManager] Retry button clicked - checking if in mini game");
        
        // Tắt Game Over Panel trước
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Kiểm tra xem có đang trong mini game không
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && (sceneController.IsInMiniGame() || FindFirstObjectByType<CoreManager>() != null))
        {
            Debug.Log("[GameOverManager] In mini game - calling CheckpointManager.RestartMiniGame()");
            
            // Nếu đang trong mini game, gọi RestartMiniGame
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.RestartMiniGame();
            }
            else
            {
                Debug.LogError("[GameOverManager] CheckpointManager.Instance is null!");
                // Fallback: reset lyra thông thường
                if (lyra != null)
                    lyra.ResetLyra();
            }
        }
        else
        {
            Debug.Log("[GameOverManager] Not in mini game - doing normal reset");
            
            // Không phải mini game, reset lyra thông thường
            if (lyra != null)
                lyra.ResetLyra();
        }
    }
}