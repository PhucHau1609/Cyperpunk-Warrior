using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public BossWarningManager warningManager;
    public BossCombatZone targetCombatZone;
    public bool isFirstTimeTrigger = true;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (warningManager == null)
        {
            warningManager = FindFirstObjectByType<BossWarningManager>();
        }
        
        if (targetCombatZone == null)
        {
            targetCombatZone = GetComponentInParent<BossCombatZone>();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered && isFirstTimeTrigger)
        {
            hasTriggered = true;
            
            if (warningManager != null && targetCombatZone != null)
            {
                // Set the target zone in warning manager
                warningManager.currentCombatZone = targetCombatZone;
                
                // Trigger first time warning sequence
                warningManager.TriggerBossWarningForZone(targetCombatZone);
            }
            else
            {
                Debug.LogWarning("[BossZoneTrigger] WarningManager or CombatZone not found!");
                
                // Fallback: directly activate the combat zone
                if (targetCombatZone != null)
                {
                    targetCombatZone.ActivateBosses();
                }
            }
            
            // Disable this trigger after first use
            gameObject.SetActive(false);
        }
    }
    
    // Method to reset trigger (called by CheckpointManager)
    public void ResetTrigger()
    {
        hasTriggered = false;
        isFirstTimeTrigger = false; // After respawn, it's no longer first time
        gameObject.SetActive(true);
        
        Debug.Log($"[BossZoneTrigger] {gameObject.name} reset - FirstTime: {isFirstTimeTrigger}");
    }
}