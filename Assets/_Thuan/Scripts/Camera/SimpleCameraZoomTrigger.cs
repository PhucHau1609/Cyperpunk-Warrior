using UnityEngine;

public class SimpleCameraZoomTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public bool isFirstTimeTrigger = true;
    
    private bool hasTriggered = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            BossWarningManager bossWarningManager = BossWarningManager.Instance;
            
            if (bossWarningManager != null)
            {
                if (isFirstTimeTrigger)
                {
                    // First time: Warning + Dialogue + Enable bosses
                    //bossWarningManager.TriggerBossWarningFirstTime();
                }
                else
                {
                    // Respawn: Warning + Enable bosses (no dialogue)
                    //bossWarningManager.TriggerBossWarningOnRespawn();
                }
            }
            else
            {
                Debug.LogWarning("[SimpleCameraZoomTrigger] BossWarningManager not found!");
            }
            
            // Disable this trigger after use
            gameObject.SetActive(false);
        }
    }
    
    // Method to reset trigger (called by CheckpointManager)
    public void ResetTrigger()
    {
        hasTriggered = false;
        isFirstTimeTrigger = false; // After respawn, it's no longer first time
        gameObject.SetActive(true);
        
        Debug.Log($"[SimpleCameraZoomTrigger] {gameObject.name} reset - FirstTime: {isFirstTimeTrigger}");
    }
}