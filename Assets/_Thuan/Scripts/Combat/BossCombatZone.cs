using System.Collections.Generic;
using UnityEngine;

public class BossCombatZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Boss Combat Zone";
    public bool isZoneActive = true;
    public bool showGizmos = true;
    public bool hasBeenEnteredAfterRespawn = false; // Track if player entered after respawn
    
    [Header("Boss References")]
    public List<MonoBehaviour> bossControllers = new List<MonoBehaviour>(); // MiniBoss, BossPhuController, etc.
    public List<Rigidbody2D> bossRigidbodies = new List<Rigidbody2D>();
    public List<Collider2D> bossColliders = new List<Collider2D>();
    
    [Header("Zone Boundaries")]
    public Transform leftBoundary;
    public Transform rightBoundary;
    public Transform topBoundary;
    public Transform bottomBoundary;
    
    [Header("Alternative Fixed Boundaries")]
    public bool useFixedBoundaries = false;
    public float leftBound = -10f;
    public float rightBound = 10f;
    public float topBound = 5f;
    public float bottomBound = -5f;
    
    [Header("Player Detection")]
    public LayerMask playerLayerMask = 1 << 6; // Layer "TransparentFX" (Player layer)
    
    [Header("Boss Behavior Settings")]
    public bool freezeBossOutsideZone = true;
    public bool disableBossAIOutsideZone = true;
    
    [Header("Warning System")]
    public bool enableWarningOnFirstEntry = true;
    public BossWarningManager warningManager;
    
    [Header("Mini Game Integration")]
    public bool isMiniGameZone = false; // Đánh dấu zone này có mini game
    
    private bool playerInZone = false;
    private bool hasTriggeredWarningBefore = false;
    private Transform playerTransform;
    private Dictionary<MonoBehaviour, bool> originalBossEnabledStates = new Dictionary<MonoBehaviour, bool>();
    
    // Events
    public System.Action OnPlayerEnterZone;
    public System.Action OnPlayerExitZone;
    public System.Action<BossCombatZone> OnZoneActivated;
    public System.Action<BossCombatZone> OnZoneDeactivated;
    
    private void Start()
    {
        // Find player
        FindPlayer();
        
        // Store original boss states
        StoreOriginalBossStates();
        
        // Initially deactivate bosses if player not in zone
        if (!IsPlayerInZone())
        {
            DeactivateBosses();
        }
    }
    
    private void Update()
    {
        if (!isZoneActive) return;
        
        CheckPlayerInZone();
    }
    
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }
    
    private void StoreOriginalBossStates()
    {
        originalBossEnabledStates.Clear();
        foreach (var boss in bossControllers)
        {
            if (boss != null)
            {
                originalBossEnabledStates[boss] = boss.enabled;
            }
        }
    }
    
    private void CheckPlayerInZone()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }
        
        bool wasPlayerInZone = playerInZone;
        playerInZone = IsPlayerInZone();
        
        // Player entered zone
        if (playerInZone && !wasPlayerInZone)
        {
            OnPlayerEnteredZone();
        }
        // Player exited zone
        else if (!playerInZone && wasPlayerInZone)
        {
            OnPlayerExitedZone();
        }
    }
    
    private bool IsPlayerInZone()
    {
        if (playerTransform == null) return false;
        
        Vector3 playerPos = playerTransform.position;
        
        // Get zone boundaries
        float left, right, top, bottom;
        GetZoneBoundaries(out left, out right, out top, out bottom);
        
        // Check if player is within boundaries
        return playerPos.x >= left && playerPos.x <= right &&
               playerPos.y >= bottom && playerPos.y <= top;
    }
    
    private void GetZoneBoundaries(out float left, out float right, out float top, out float bottom)
    {
        if (useFixedBoundaries)
        {
            left = leftBound;
            right = rightBound;
            top = topBound;
            bottom = bottomBound;
        }
        else
        {
            left = leftBoundary != null ? leftBoundary.position.x : transform.position.x - 10f;
            right = rightBoundary != null ? rightBoundary.position.x : transform.position.x + 10f;
            top = topBoundary != null ? topBoundary.position.y : transform.position.y + 5f;
            bottom = bottomBoundary != null ? bottomBoundary.position.y : transform.position.y - 5f;
        }
    }
    
    private void OnPlayerEnteredZone()
    {
        Debug.Log($"[BossCombatZone] Player entered {zoneName}");
        
        // QUAN TRỌNG: Nếu là mini game zone và đang ở respawn state, kích hoạt mini game
        if (isMiniGameZone && hasBeenEnteredAfterRespawn)
        {
            Debug.Log("[BossCombatZone] Mini game zone entered after respawn - activating mini game");
            TriggerMiniGameAfterRespawn();
            hasBeenEnteredAfterRespawn = false;
            return;
        }
        
        // Check if this is first time entry or entry after respawn
        if (enableWarningOnFirstEntry && !hasTriggeredWarningBefore)
        {
            // Very first time entering this zone
            TriggerWarningSequence();
        }
        else if (!hasTriggeredWarningBefore && hasBeenEnteredAfterRespawn)
        {
            // First entry after respawn - trigger warning but mark as respawn
            TriggerWarningSequenceForRespawn();
        }
        else
        {
            // Already triggered before - immediately activate bosses or mini game
            if (isMiniGameZone)
            {
                TriggerMiniGame();
            }
            else
            {
                ActivateBosses();
            }
        }
        
        hasBeenEnteredAfterRespawn = false; // Reset flag
        
        OnPlayerEnterZone?.Invoke();
        OnZoneActivated?.Invoke(this);
    }
    
    // Method mới để trigger mini game sau respawn
    private void TriggerMiniGameAfterRespawn()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            // Trigger mini game ngay lập tức không cần warning
            sceneController.SwitchToPetControl();
            Debug.Log("[BossCombatZone] Mini game activated after respawn");
        }
        else
        {
            Debug.LogWarning("[BossCombatZone] SceneController not found for mini game activation");
        }
    }
    
    // Method mới để trigger mini game bình thường
    private void TriggerMiniGame()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            sceneController.SwitchToPetControl();
            Debug.Log("[BossCombatZone] Mini game activated");
        }
    }
    
    private void TriggerWarningSequence()
    {
        hasTriggeredWarningBefore = true;
        
        if (warningManager != null)
        {
            // First time ever - full sequence with dialogue
            warningManager.TriggerBossWarningForZone(this);
        }
        else
        {
            // Fallback: directly activate bosses or mini game after delay
            if (isMiniGameZone)
            {
                Invoke(nameof(TriggerMiniGame), 3f);
            }
            else
            {
                Invoke(nameof(ActivateBosses), 3f);
            }
        }
    }
    
    private void TriggerWarningSequenceForRespawn()
    {
        hasTriggeredWarningBefore = true;
        
        if (warningManager != null)
        {
            // Respawn entry - warning only, no dialogue
            warningManager.TriggerBossWarningOnRespawnInZone(this);
        }
        else
        {
            // Fallback: directly activate bosses or mini game after shorter delay  
            if (isMiniGameZone)
            {
                Invoke(nameof(TriggerMiniGame), 2f);
            }
            else
            {
                Invoke(nameof(ActivateBosses), 2f);
            }
        }
    }
    
    private void OnPlayerExitedZone()
    {
        Debug.Log($"[BossCombatZone] Player exited {zoneName}");
        
        // Nếu là mini game zone, return control về player
        if (isMiniGameZone)
        {
            SceneController sceneController = FindFirstObjectByType<SceneController>();
            if (sceneController != null && sceneController.IsInMiniGame())
            {
                sceneController.ReturnControlToPlayer();
                Debug.Log("[BossCombatZone] Player exited mini game zone - returning control");
            }
        }
        else
        {
            DeactivateBosses();
        }
        
        OnPlayerExitZone?.Invoke();
        OnZoneDeactivated?.Invoke(this);
    }
    
    public void ActivateBosses()
    {
        // Nếu là mini game zone, trigger mini game thay vì activate bosses
        if (isMiniGameZone)
        {
            TriggerMiniGame();
            return;
        }
        
        foreach (var boss in bossControllers)
        {
            if (boss != null)
            {
                boss.enabled = true;
                
                // Special handling for different boss types
                HandleBossActivation(boss);
            }
        }
        
        // Activate rigidbodies
        foreach (var rb in bossRigidbodies)
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        
        // Activate colliders
        foreach (var col in bossColliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
        
        Debug.Log($"[BossCombatZone] Activated {bossControllers.Count} boss(es) in {zoneName}");
    }
    
    public void DeactivateBosses()
    {
        foreach (var boss in bossControllers)
        {
            if (boss != null)
            {
                if (disableBossAIOutsideZone)
                {
                    boss.enabled = false;
                }
                
                // Special handling for different boss types
                HandleBossDeactivation(boss);
            }
        }
        
        // Freeze rigidbodies
        if (freezeBossOutsideZone)
        {
            foreach (var rb in bossRigidbodies)
            {
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        
        Debug.Log($"[BossCombatZone] Deactivated {bossControllers.Count} boss(es) in {zoneName}");
    }
    
    private void HandleBossActivation(MonoBehaviour boss)
    {
        // Handle MiniBoss
        if (boss is MiniBoss miniBoss)
        {
            // Ensure boss can find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // Use reflection to set player field if accessible
                var playerField = typeof(MiniBoss).GetField("player", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerField != null)
                {
                    playerField.SetValue(miniBoss, playerObj.transform);
                }
            }
        }
        
        // Handle BossPhuController
        else if (boss is BossPhuController bossPhu)
        {
            // BossPhuController has public player field
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                bossPhu.player = playerObj.transform;
            }
            
            // CRITICAL: Ensure boss starts in correct state
            var rb = bossPhu.GetComponent<Rigidbody2D>();
            var animator = bossPhu.GetComponent<Animator>();
            var behaviorTree = bossPhu.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero; // Start with no movement
                rb.angularVelocity = 0f;
            }
            
            if (animator != null)
            {
                // Start with Idle animation
                animator.SetBool("IsRunning", false);
                animator.Update(0f); // Force immediate update
            }
            
            // Reset boss internal states
            bossPhu.isAttacking = false;
            
            // Enable Behavior Tree AFTER setting up states
            if (behaviorTree != null)
            {
                behaviorTree.enabled = true;
                // Small delay before enabling to ensure states are set
                StartCoroutine(EnableBehaviorTreeDelayed(behaviorTree, 0.1f));
            }
        }
        
        Debug.Log($"[BossCombatZone] Activated boss: {boss.name}");
    }
    
    private System.Collections.IEnumerator EnableBehaviorTreeDelayed(BehaviorDesigner.Runtime.BehaviorTree behaviorTree, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (behaviorTree != null)
        {
            behaviorTree.EnableBehavior();
            behaviorTree.RestartWhenComplete = true;
        }
    }
    
    private void HandleBossDeactivation(MonoBehaviour boss)
    {
        // Stop any attacks or special behaviors
        if (boss is MiniBoss miniBoss)
        {
            // Stop any ongoing coroutines or attacks
            var rb = miniBoss.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        
        else if (boss is BossPhuController bossPhu)
        {
            var rb = bossPhu.GetComponent<Rigidbody2D>();
            var animator = bossPhu.GetComponent<Animator>();
            var behaviorTree = bossPhu.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            
            // CRITICAL: Disable Behavior Tree FIRST
            if (behaviorTree != null)
            {
                behaviorTree.DisableBehavior();
                behaviorTree.enabled = false;
            }
            
            // CRITICAL: Stop movement completely
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                if (freezeBossOutsideZone)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }
            }
            
            // QUAN TRỌNG: Reset animator về trạng thái Idle
            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
                // Reset các trigger khác nếu có
                animator.ResetTrigger("Attack1");
                animator.ResetTrigger("Attack2");
                animator.ResetTrigger("Attack3");
                animator.ResetTrigger("Attack4");
                animator.ResetTrigger("PhaseChange");
                animator.ResetTrigger("Hurt");
                
                // Force update animator để áp dụng ngay lập tức
                for (int i = 0; i < 3; i++)
                {
                    animator.Update(0.02f);
                }
            }
            
            // Reset internal boss states
            bossPhu.isAttacking = false;
            
            // Stop audio
            var audioSource = bossPhu.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        Debug.Log($"[BossCombatZone] Deactivated boss: {boss.name}");
    }
    
    // Public methods for external control
    public void SetZoneActive(bool active)
    {
        isZoneActive = active;
        if (!active)
        {
            DeactivateBosses();
        }
    }
    
    public void ResetZone()
    {
        hasTriggeredWarningBefore = false;
        playerInZone = false;
        hasBeenEnteredAfterRespawn = true; // Mark that this zone will be entered after respawn
        
        Debug.Log($"[BossCombatZone] Resetting zone: {zoneName}, isMiniGameZone: {isMiniGameZone}");
        
        // QUAN TRỌNG: Nếu là mini game zone, đảm bảo player control được return
        if (isMiniGameZone)
        {
            SceneController sceneController = FindFirstObjectByType<SceneController>();
            if (sceneController != null && sceneController.IsInMiniGame())
            {
                sceneController.ReturnControlToPlayer();
                Debug.Log("[BossCombatZone] Returned control to player during zone reset");
            }
        }
        
        // Reset all bosses in zone with null checks
        foreach (var boss in bossControllers)
        {
            if (boss != null)
            {
                try
                {
                    // Reset boss state if implements IBossResettable
                    if (boss is IBossResettable resettable)
                    {
                        resettable.ResetBoss();
                    }
                    
                    // Force stop animations and movement
                    ForceStopBossAnimationsAndMovement(boss);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BossCombatZone] Error resetting boss {boss.name}: {e.Message}");
                }
            }
        }
        
        // Deactivate bosses initially
        DeactivateBosses();
        
        Debug.Log($"[BossCombatZone] Reset zone: {zoneName} - Will trigger warning on next entry");
    }
    
    private void ForceStopBossAnimationsAndMovement(MonoBehaviour boss)
    {
        // Get common components
        var rb = boss.GetComponent<Rigidbody2D>();
        var animator = boss.GetComponent<Animator>();
        
        // Stop movement FIRST - very important
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // Force static to prevent any movement
        }
        
        // Reset animator to idle state
        if (animator != null)
        {
            // Reset all common animation parameters
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
            
            // Reset all common triggers
            string[] commonTriggers = { "Attack1", "Attack2", "Attack3", "Attack4", 
                                       "PhaseChange", "Hurt", "Death", "MeleeAttack", "RangedAttack" };
            
            foreach (string trigger in commonTriggers)
            {
                try
                {
                    animator.ResetTrigger(trigger);
                }
                catch
                {
                    // Ignore if trigger doesn't exist
                }
            }
        }
        
        // Special handling for different boss types
        if (boss is BossPhuController bossPhu)
        {
            // CRITICAL: Stop any BossPhu specific behaviors and movement tracking
            bossPhu.isAttacking = false;
            
            // Reset internal states that affect animation
            var rbPhu = bossPhu.GetComponent<Rigidbody2D>();
            if (rbPhu != null)
            {
                rbPhu.linearVelocity = Vector2.zero;
                rbPhu.angularVelocity = 0f;
                rbPhu.bodyType = RigidbodyType2D.Static;
            }
            
            // Force animator to Idle immediately
            var animatorPhu = bossPhu.GetComponent<Animator>();
            if (animatorPhu != null)
            {
                animatorPhu.SetBool("IsRunning", false);
                // Force update animator state
                animatorPhu.Update(0f);
            }
            
            // Stop audio if playing
            var audioSource = bossPhu.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Disable the script temporarily to prevent Update() from interfering
            bossPhu.enabled = false;
        }
        
        if (boss is MiniBoss miniBoss)
        {
            // Stop any MiniBoss specific coroutines
            miniBoss.StopAllCoroutines();
            
            var rbMini = miniBoss.GetComponent<Rigidbody2D>();
            if (rbMini != null)
            {
                rbMini.linearVelocity = Vector2.zero;
                rbMini.bodyType = RigidbodyType2D.Static;
            }
        }
    }
    
    public void ForceActivateBosses()
    {
        ActivateBosses();
    }
    
    public void ForceDeactivateBosses()
    {
        DeactivateBosses();
    }
    
    public bool IsPlayerCurrentlyInZone()
    {
        return playerInZone;
    }
    
    public void AddBoss(MonoBehaviour boss)
    {
        if (boss != null && !bossControllers.Contains(boss))
        {
            bossControllers.Add(boss);
            originalBossEnabledStates[boss] = boss.enabled;
        }
    }
    
    public void RemoveBoss(MonoBehaviour boss)
    {
        if (boss != null && bossControllers.Contains(boss))
        {
            bossControllers.Remove(boss);
            if (originalBossEnabledStates.ContainsKey(boss))
            {
                originalBossEnabledStates.Remove(boss);
            }
        }
    }
    
    // Gizmos for visualization
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        float left, right, top, bottom;
        GetZoneBoundaries(out left, out right, out top, out bottom);
        
        // Draw zone boundaries
        Gizmos.color = playerInZone ? Color.green : Color.red;
        
        Vector3 center = new Vector3((left + right) / 2f, (top + bottom) / 2f, 0f);
        Vector3 size = new Vector3(right - left, top - bottom, 1f);
        
        Gizmos.DrawWireCube(center, size);
        
        // Draw zone label
        Gizmos.color = Color.white;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(center + Vector3.up * (size.y / 2f + 1f), zoneName + (isMiniGameZone ? " (MiniGame)" : ""));
        #endif
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        float left, right, top, bottom;
        GetZoneBoundaries(out left, out right, out top, out bottom);
        
        // Draw filled zone when selected
        Gizmos.color = isMiniGameZone ? new Color(0f, 0f, 1f, 0.2f) : new Color(0f, 1f, 0f, 0.2f);
        
        Vector3 center = new Vector3((left + right) / 2f, (top + bottom) / 2f, 0f);
        Vector3 size = new Vector3(right - left, top - bottom, 1f);
        
        Gizmos.DrawCube(center, size);
    }
}