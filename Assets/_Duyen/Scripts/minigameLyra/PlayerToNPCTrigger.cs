using UnityEngine;

public class PlayerToNPCTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // QUAN TRỌNG: Check protection flag trước khi trigger
            if (CheckpointManager.Instance != null && CheckpointManager.Instance.IsMiniGameProtected())
            {
                Debug.LogWarning("[PlayerToNPCTrigger] Mini game is protected - preventing trigger");
                return;
            }
            
            // QUAN TRỌNG: Check nếu mini game đã hoàn thành
            if (CheckpointManager.Instance != null && CheckpointManager.Instance.miniGameCompleted)
            {
                Debug.LogWarning("[PlayerToNPCTrigger] Mini game already completed - preventing trigger");
                return;
            }
            
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                Debug.Log("[PlayerToNPCTrigger] Triggering mini game start");
                controller.SwitchToPetControl();
                //gameObject.SetActive(false); // tắt trigger nếu chỉ dùng 1 lần
            }
        }
    }
}