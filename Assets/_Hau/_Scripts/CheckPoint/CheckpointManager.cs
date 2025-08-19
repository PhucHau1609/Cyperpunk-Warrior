using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CheckpointManager : HauSingleton<CheckpointManager>
{
    public CheckPointEnum lastCheckpointID;
    public Vector3 lastCheckpointPosition;
    public string lastCheckpointScene;
    
    [Header("Boss Reset Settings")]
    public bool enableBossReset = true;
    public bool resetOnlyActiveBosses = true;
    
    [Header("MapBoss_01Test Special Settings")]
    public bool miniGameCompleted = false; // Track if mini game is completed
    
    // Cache để tránh tìm kiếm lại liên tục
    private Dictionary<string, List<IBossResettable>> cachedBosses = new Dictionary<string, List<IBossResettable>>();
    private float lastCacheTime = 0f;
    private const float CACHE_DURATION = 2f;

    public void SetCurrentCheckpoint(CheckPointEnum id, Vector3 position, string sceneName)
    {
        lastCheckpointID = id;
        lastCheckpointPosition = position;
        lastCheckpointScene = sceneName;

        // Special handling for MapBoss_01Test
        if (sceneName == "MapBoss_01Test")
        {
            HandleMapBoss01TestCheckpoint(id);
        }

        // >>> GỬI SAVE LÊN SERVER
        //TrySaveToServer();
    }

    private void HandleMapBoss01TestCheckpoint(CheckPointEnum checkpointID)
    {
        // CheckPoint_0 = Mini game area (start of map)
        // CheckPoint_1 = Boss area (end of map) 
        
        if (checkpointID == CheckPointEnum.CheckPoint_1)
        {
            // Reached boss checkpoint - mini game must be completed
            miniGameCompleted = true;
            
            // QUAN TRỌNG: Force disable any mini game triggers/auto-start
            DisableAutoMiniGameTriggers();
            
            // Disable pet following since mini game is done
            DisablePetFollowing();
            
            // QUAN TRỌNG: Force GameStateManager to Gameplay immediately
            ForceResetGameStateToGameplay();
            
            Debug.Log("[CheckpointManager] MapBoss_01Test: Reached boss checkpoint, mini game completed, triggers disabled");
        }
        else if (checkpointID == CheckPointEnum.CheckPoint_0)
        {
            // At mini game checkpoint - reset completion status if needed
            Debug.Log("[CheckpointManager] MapBoss_01Test: At mini game checkpoint");
            
            // Don't automatically set miniGameCompleted = false here
            // Let the natural flow handle it
        }
    }

    // Method để disable auto mini game triggers - SMART VERSION (No tags needed)
    private void DisableAutoMiniGameTriggers()
    {
        Debug.Log("[CheckpointManager] === SMART DISABLING AUTO MINI GAME TRIGGERS ===");
        
        int disabledCount = 0;
        
        // 1. ALWAYS disable PlayerToNPCTrigger scripts (100% accurate)
        PlayerToNPCTrigger[] npcTriggers = FindObjectsByType<PlayerToNPCTrigger>(FindObjectsSortMode.None);
        foreach (var trigger in npcTriggers)
        {
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
                Debug.Log($"[CheckpointManager] ✅ Disabled PlayerToNPCTrigger: {trigger.gameObject.name}");
                disabledCount++;
            }
        }
        
        // 2. Find trigger colliders with suspicious names
        Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var collider in allColliders)
        {
            if (collider != null && collider.isTrigger && collider.gameObject.activeInHierarchy)
            {
                string objName = collider.gameObject.name.ToLower();
                
                // Smart name detection
                bool isSuspiciousName = objName.Contains("minigame") || 
                                       objName.Contains("startgame") ||
                                       objName.Contains("petcontrol") ||
                                       objName.Contains("npccontrol") ||
                                       objName.Contains("triggertonpc") ||
                                       objName.Contains("switchpet") ||
                                       objName.Contains("playertopet") ||
                                       (objName.Contains("trigger") && objName.Contains("npc")) ||
                                       (objName.Contains("zone") && objName.Contains("start"));
                
                if (isSuspiciousName)
                {
                    collider.gameObject.SetActive(false);
                    Debug.Log($"[CheckpointManager] ✅ Disabled suspicious trigger: {collider.gameObject.name}");
                    disabledCount++;
                }
            }
        }
        
        // 3. Find objects with scripts that might trigger mini game
        MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in allScripts)
        {
            if (script != null && script.gameObject.activeInHierarchy)
            {
                string scriptType = script.GetType().Name.ToLower();
                
                // Check for mini game related scripts (exclude CheckpointManager itself)
                bool isMiniGameScript = (scriptType.Contains("minigame") ||
                                        scriptType.Contains("petcontrol") ||
                                        scriptType.Contains("npccontrol") ||
                                        scriptType.Contains("triggertopet") ||
                                        scriptType.Contains("switchtonpc")) &&
                                       script != this; // Don't disable CheckpointManager
                
                if (isMiniGameScript)
                {
                    script.gameObject.SetActive(false);
                    Debug.Log($"[CheckpointManager] ✅ Disabled mini game script: {script.gameObject.name} ({script.GetType().Name})");
                    disabledCount++;
                }
            }
        }
        
        // 4. Find objects that have Events calling SceneController methods
        // This checks for objects with UnityEvents that might call SwitchToPetControl
        Component[] allComponents = FindObjectsByType<Component>(FindObjectsSortMode.None);
        foreach (var component in allComponents)
        {
            if (component != null && component.gameObject.activeInHierarchy)
            {
                // Check if this component might have events that trigger mini game
                var componentType = component.GetType();
                
                // Look for common event-triggering components
                if (componentType.Name.Contains("Trigger") && 
                    !componentType.Name.Contains("Collider") &&
                    component.gameObject.GetComponent<Collider2D>() != null &&
                    component.gameObject.GetComponent<Collider2D>().isTrigger)
                {
                    // This might be a trigger script
                    string objName = component.gameObject.name.ToLower();
                    if (objName.Contains("start") || objName.Contains("begin") || objName.Contains("init"))
                    {
                        component.gameObject.SetActive(false);
                        Debug.Log($"[CheckpointManager] ✅ Disabled event trigger: {component.gameObject.name}");
                        disabledCount++;
                    }
                }
            }
        }
        
        // 5. Fallback: Disable any remaining known trigger tags (if they exist)
        try 
        {
            GameObject[] triggerZones = GameObject.FindGameObjectsWithTag("TriggerZone");
            GameObject[] miniGameTriggers = GameObject.FindGameObjectsWithTag("MiniGameTrigger");
            
            foreach (var zone in triggerZones)
            {
                if (zone != null && zone.activeInHierarchy)
                {
                    zone.SetActive(false);
                    Debug.Log($"[CheckpointManager] ✅ Disabled TriggerZone: {zone.name}");
                    disabledCount++;
                }
            }
            
            foreach (var trigger in miniGameTriggers)
            {
                if (trigger != null && trigger.activeInHierarchy)
                {
                    trigger.SetActive(false);
                    Debug.Log($"[CheckpointManager] ✅ Disabled MiniGameTrigger: {trigger.name}");
                    disabledCount++;
                }
            }
        }
        catch 
        {
            // Tags don't exist, that's fine
            Debug.Log("[CheckpointManager] No custom tags found (this is OK)");
        }
        
        Debug.Log($"[CheckpointManager] === SMART DISABLE COMPLETED: {disabledCount} trigger(s) disabled ===");
        
        // 6. Extra safety: Force reset any active mini game components
        ForceDisableActiveMiniGameComponents();
    }
    
    // Method để force disable các components mini game đang active
    private void ForceDisableActiveMiniGameComponents()
    {
        Debug.Log("[CheckpointManager] Force disabling active mini game components...");
        
        // Disable pet controls
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            PetManualControl petControl = pet.GetComponent<PetManualControl>();
            if (petControl != null && petControl.enabled)
            {
                petControl.enabled = false;
                Debug.Log("[CheckpointManager] ✅ Force disabled PetManualControl");
            }
            
            PetShooting petShooting = pet.GetComponent<PetShooting>();
            if (petShooting != null && petShooting.enabled)
            {
                petShooting.enabled = false;
                Debug.Log("[CheckpointManager] ✅ Force disabled PetShooting");
            }
            
            LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
            if (lyraHealth != null && lyraHealth.enabled)
            {
                lyraHealth.enabled = false;
                if (lyraHealth.healthBarUI != null)
                    lyraHealth.healthBarUI.gameObject.SetActive(false);
                Debug.Log("[CheckpointManager] ✅ Force disabled LyraHealth");
            }
        }
        
        // Ensure player can move
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
            Debug.Log("[CheckpointManager] ✅ Force enabled PlayerMovement");
        }
    }

    private void DisablePetFollowing()
    {
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            pet.Disappear(); // Use existing Disappear method
            Debug.Log("[CheckpointManager] Pet disabled for boss area");
        }
    }

    public void RespawnPlayer(GameObject player)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"[CheckpointManager] ⚠️ RespawnPlayer called in scene: {currentScene}");

        // QUAN TRỌNG: FORCE RESET GAMESTATE NGAY LẬP TỨC cho tất cả scenes
        ForceResetGameStateToGameplay();

        // Special handling for MapBoss_01Test
        if (currentScene == "MapBoss_01Test")
        {
            HandleMapBoss01TestRespawn(player);
            return;
        }

        // Original respawn logic for other scenes
        HandleNormalRespawn(player, currentScene);
    }

    // Method mới để force reset GameState về Gameplay
    private void ForceResetGameStateToGameplay()
    {
        Debug.Log("[CheckpointManager] === FORCE RESETTING GAMESTATE TO GAMEPLAY ===");
        
        if (GameStateManager.Instance != null)
        {
            GameState currentState = GameStateManager.Instance.CurrentState;
            Debug.Log($"[CheckpointManager] GameStateManager current state: {currentState}");
            
            if (currentState != GameState.Gameplay)
            {
                Debug.Log("[CheckpointManager] Forcing GameStateManager to Gameplay");
                GameStateManager.Instance.ForceResetToGameplay();
                
                // Double check
                GameState newState = GameStateManager.Instance.CurrentState;
                Debug.Log($"[CheckpointManager] GameStateManager after reset: {newState}");
                
                if (newState != GameState.Gameplay)
                {
                    Debug.LogError("[CheckpointManager] CRITICAL: GameStateManager failed to reset to Gameplay!");
                    // Last resort - directly set
                    GameStateManager.Instance.ForceSetState(GameState.Gameplay);
                }
            }
            else
            {
                Debug.Log("[CheckpointManager] GameStateManager already in Gameplay state");
            }
        }
        else
        {
            Debug.LogError("[CheckpointManager] GameStateManager.Instance is null!");
        }
        
        Debug.Log("[CheckpointManager] === GAMESTATE RESET COMPLETED ===");
    }

    private void HandleMapBoss01TestRespawn(GameObject player)
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        
        // QUAN TRỌNG: Debug state trước khi xử lý
        Debug.Log($"[CheckpointManager] === PRE-RESPAWN STATE DEBUG ===");
        Debug.Log($"GameState: {(GameStateManager.Instance != null ? GameStateManager.Instance.CurrentState.ToString() : "NULL")}");
        Debug.Log($"miniGameCompleted: {miniGameCompleted}");
        Debug.Log($"lastCheckpointID: {lastCheckpointID}");
        if (sceneController != null)
        {
            sceneController.DebugCurrentState();
        }
        Debug.Log($"[CheckpointManager] === END PRE-RESPAWN DEBUG ===");
        
        // QUAN TRỌNG: Force ensure GameStateManager is in Gameplay
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Gameplay)
        {
            Debug.LogWarning($"[CheckpointManager] WARNING: GameStateManager still not in Gameplay ({GameStateManager.Instance.CurrentState}) - forcing again");
            GameStateManager.Instance.ForceResetToGameplay();
        }
        
        // QUAN TRỌNG: Kiểm tra chính xác mini game state NHƯNG ưu tiên miniGameCompleted
        bool isCurrentlyInMiniGame = false;
        
        // Nếu mini game đã hoàn thành, KHÔNG BAO GIỜ coi như đang trong mini game
        if (miniGameCompleted)
        {
            Debug.Log("[CheckpointManager] Mini game completed - will NOT treat as in mini game regardless of other states");
            isCurrentlyInMiniGame = false;
        }
        else
        {
            // Chỉ check các state khác khi mini game chưa hoàn thành
            isCurrentlyInMiniGame = (sceneController != null && coreManager != null && 
                                    sceneController.IsInMiniGame());
        }
        
        Debug.Log($"[CheckpointManager] MapBoss_01Test Debug - miniGameCompleted: {miniGameCompleted}, isCurrentlyInMiniGame: {isCurrentlyInMiniGame}, lastCheckpointID: {lastCheckpointID}");
        
        // Case 1: Currently in mini game AND mini game not completed
        if (isCurrentlyInMiniGame && !miniGameCompleted)
        {
            Debug.Log("[CheckpointManager] MapBoss_01Test: In mini game - calling RestartMiniGame()");
            RestartMiniGame();
            return;
        }
        
        // Case 2: Mini game completed, player died in boss area
        if (miniGameCompleted && lastCheckpointID == CheckPointEnum.CheckPoint_1)
        {
            Debug.Log("[CheckpointManager] MapBoss_01Test: Mini game completed, respawning at boss checkpoint");
            RespawnAtBossCheckpoint(player);
            return;
        }
        
        // Case 3: Mini game completed but player died before reaching boss checkpoint
        // Force respawn at boss checkpoint
        if (miniGameCompleted)
        {
            Debug.Log("[CheckpointManager] MapBoss_01Test: Mini game completed, but died before boss - respawn at boss checkpoint");
            
            // Force respawn at boss checkpoint
            Checkpoint bossCheckpoint = FindBossCheckpoint();
            if (bossCheckpoint != null)
            {
                player.transform.position = bossCheckpoint.spawnPoint.position;
                SetCurrentCheckpoint(bossCheckpoint.checkpointID, bossCheckpoint.spawnPoint.position, "MapBoss_01Test");
                RespawnAtBossCheckpoint(player);
            }
            else
            {
                Debug.LogError("[CheckpointManager] MapBoss_01Test: Boss checkpoint not found!");
                HandleNormalRespawn(player, "MapBoss_01Test");
            }
            return;
        }
        
        // Case 4: Mini game not completed - normal respawn at mini game checkpoint
        Debug.Log("[CheckpointManager] MapBoss_01Test: Mini game not completed - normal respawn");
        HandleNormalRespawn(player, "MapBoss_01Test");
    }

    private void RespawnAtBossCheckpoint(GameObject player)
    {
        Debug.Log("[CheckpointManager] MapBoss_01Test: Respawning at boss checkpoint");
        
        // QUAN TRỌNG: FORCE RESET tất cả mini game states TRƯỚC khi làm gì khác
        ForceResetAllMiniGameStates();
        
        // QUAN TRỌNG: Disable auto mini game triggers để prevent auto-restart
        DisableAutoMiniGameTriggers();
        
        // QUAN TRỌNG: Set protection flag để prevent mini game auto-start
        SetMiniGameProtection(true);
        
        // Reset Combat Zones first but preserve boss states
        ResetAllCombatZonesForBossArea();
        
        // Reset bosses but preserve their disabled colliders
        if (enableBossReset)
        {
            ResetBossesInCurrentScenePreserveState();
        }
        
        // Reset triggers
        ResetTriggersInCurrentScene();
        
        // Move player to boss checkpoint
        player.transform.position = lastCheckpointPosition;
        Debug.Log($"[CheckpointManager] Player moved to boss checkpoint: {lastCheckpointPosition}");
        
        // Ensure pet is not following in boss area
        DisablePetFollowing();
        
        // Finish respawn (restore health, etc.)
        FinishRespawn(player);
        
        // QUAN TRỌNG: Delay reset protection để cho mọi thứ settle
        StartCoroutine(DelayedResetProtection());
    }

    // Protection flag để prevent auto mini game start
    private bool miniGameProtectionActive = false;
    
    // Method để set protection
    private void SetMiniGameProtection(bool active)
    {
        miniGameProtectionActive = active;
        Debug.Log($"[CheckpointManager] Mini game protection: {(active ? "ACTIVE" : "INACTIVE")}");
    }
    
    // Method để check protection
    public bool IsMiniGameProtected()
    {
        return miniGameProtectionActive;
    }
    
    // Coroutine để delay reset protection
    private System.Collections.IEnumerator DelayedResetProtection()
    {
        yield return new WaitForSeconds(3f); // Đợi 3 giây để mọi thứ settle
        
        SetMiniGameProtection(false);
        Debug.Log("[CheckpointManager] Mini game protection reset after delay");
    }

    // Method mới để force reset tất cả mini game states
    private void ForceResetAllMiniGameStates()
    {
        Debug.Log("[CheckpointManager] === FORCE RESETTING ALL MINI GAME STATES ===");
        
        // 1. Force reset SceneController state
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            Debug.Log("[CheckpointManager] Force resetting SceneController state");
            sceneController.ForceResetMiniGameState();
            
            // Double check: nếu vẫn nghĩ đang trong mini game thì force return control
            if (sceneController.IsInMiniGame())
            {
                Debug.Log("[CheckpointManager] SceneController still thinks in mini game - force returning control");
                sceneController.ReturnControlToPlayer(true); // Force complete
            }
        }
        
        // 2. Force reset GameStateManager
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[CheckpointManager] GameStateManager current state: {GameStateManager.Instance.CurrentState}");
            if (GameStateManager.Instance.CurrentState == GameState.MiniGame)
            {
                Debug.Log("[CheckpointManager] Force resetting GameStateManager to Gameplay");
                GameStateManager.Instance.ResetToGameplay();
            }
        }
        
        // 3. Force disable pet controls
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            PetManualControl petControl = pet.GetComponent<PetManualControl>();
            if (petControl != null && petControl.enabled)
            {
                Debug.Log("[CheckpointManager] Force disabling pet manual control");
                petControl.enabled = false;
            }
            
            PetShooting petShooting = pet.GetComponent<PetShooting>();
            if (petShooting != null && petShooting.enabled)
            {
                Debug.Log("[CheckpointManager] Force disabling pet shooting");
                petShooting.enabled = false;
            }
            
            LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
            if (lyraHealth != null && lyraHealth.enabled)
            {
                Debug.Log("[CheckpointManager] Force disabling lyra health");
                lyraHealth.enabled = false;
                if (lyraHealth.healthBarUI != null)
                    lyraHealth.healthBarUI.gameObject.SetActive(false);
            }
        }
        
        // 4. Force reset UI state
        if (InventoryUIHandler.Instance != null)
        {
            // Reset UI to normal state
            InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame(); // This should reset to normal
        }
        
        // 5. Force reset camera target
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && CameraFollow.Instance != null)
        {
            Debug.Log("[CheckpointManager] Force setting camera target to player");
            CameraFollow.Instance.Target = playerObj.transform;
        }
        
        // 6. Ensure player can move
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            Debug.Log("[CheckpointManager] Force enabling player movement");
            playerMovement.SetCanMove(true);
        }
        
        Debug.Log("[CheckpointManager] === FORCE RESET COMPLETED ===");
    }

    private Checkpoint FindBossCheckpoint()
    {
        Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        
        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint.checkpointID == CheckPointEnum.CheckPoint_1) // Boss checkpoint
            {
                return checkpoint;
            }
        }
        
        return null;
    }

    private void HandleNormalRespawn(GameObject player, string currentScene)
    {
        if (lastCheckpointScene != currentScene)
        {
            LoadSceneWithCleanup(lastCheckpointScene, player);
        }
        else
        {
            // Reset Combat Zones first
            ResetAllCombatZones();
            
            // Reset bosses if enabled
            if (enableBossReset)
            {
                ResetBossesInCurrentScene();
            }
            
            // Reset triggers
            ResetTriggersInCurrentScene();
            
            // Move player
            player.transform.position = lastCheckpointPosition;
            Debug.Log($"[CheckpointManager] Player moved to checkpoint: {lastCheckpointPosition}");
            
            FinishRespawn(player);
        }
    }

    // Method mới để restart mini game - IMPROVED to preserve progress
    public void RestartMiniGame()
    {
        Debug.Log("[CheckpointManager] 🎮 RestartMiniGame called (MINI GAME RESTART)");
        Debug.Log("[CheckpointManager] ========== STARTING MINI GAME RESTART ==========");
        
        // Find and force stop any running mini game
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && sceneController.IsInMiniGame())
        {
            Debug.Log("[CheckpointManager] Force stopping current mini game...");
            sceneController.ReturnControlToPlayer(false);
        }
        
        // Reset Lyra health and position first
        ResetLyraForMiniGame();
        
        // Reset obstacles and traps
        ResetMiniGameObstacles();
        
        // QUAN TRỌNG: Chỉ reset CoreManager mà KHÔNG reset progress
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            Debug.Log("[CheckpointManager] Resetting CoreManager while preserving progress...");
            coreManager.ResetCoreManagerKeepProgress(); // Method mới
        }
        
        // Reset triggers and combat zones
        ResetTriggersInCurrentScene();
        ResetAllCombatZones();
        
        // Delay restart mini game
        StartCoroutine(DelayedRestartMiniGame(sceneController));
    }
    
    // Method to reset mini game completion status (call when returning to mini game area)
    public void ResetMiniGameCompletion()
    {
        miniGameCompleted = false;
        Debug.Log("[CheckpointManager] Mini game completion status reset");
    }

    // Method to check current respawn behavior for MapBoss_01Test - IMPROVED
    public void DebugMapBoss01TestStatus()
    {
        if (SceneManager.GetActiveScene().name == "MapBoss_01Test")
        {
            Debug.Log($"[CheckpointManager] === MapBoss_01Test Status ===");
            Debug.Log($"  - Mini Game Completed: {miniGameCompleted}");
            Debug.Log($"  - Last Checkpoint ID: {lastCheckpointID}");
            Debug.Log($"  - Last Checkpoint Position: {lastCheckpointPosition}");
            
            // Debug GameStateManager
            if (GameStateManager.Instance != null)
            {
                Debug.Log($"  - GameStateManager Current State: {GameStateManager.Instance.CurrentState}");
                Debug.Log($"  - Is Gameplay: {GameStateManager.Instance.IsGameplay}");
                Debug.Log($"  - Is Busy: {GameStateManager.Instance.IsBusy()}");
            }
            
            SceneController sceneController = FindFirstObjectByType<SceneController>();
            CoreManager coreManager = FindFirstObjectByType<CoreManager>();
            
            if (sceneController != null)
            {
                sceneController.DebugCurrentState();
                
                bool isInMiniGame = sceneController.IsInMiniGame();
                Debug.Log($"  - Currently in Mini Game: {isInMiniGame}");
                
                if (coreManager != null)
                {
                    coreManager.DebugProgress();
                }
            }
            else
            {
                Debug.Log("  - SceneController: NOT FOUND");
            }
            
            // Debug boss collider states
            DebugBossColliderStates();
            
            Debug.Log($"[CheckpointManager] === END MapBoss_01Test Status ===");
        }
    }

    // Method để manual force reset GameStateManager (for testing)
    public void ManualForceResetGameStateToGameplay()
    {
        Debug.Log("[CheckpointManager] === MANUAL FORCE RESET GAMESTATE ===");
        ForceResetGameStateToGameplay();
        Debug.Log("[CheckpointManager] === MANUAL RESET COMPLETED ===");
    }

    // Method để manual test respawn logic
    public void TestRespawnLogic()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("[CheckpointManager] === TESTING RESPAWN LOGIC ===");
            RespawnPlayer(player);
            Debug.Log("[CheckpointManager] === TEST COMPLETED ===");
        }
        else
        {
            Debug.LogError("[CheckpointManager] Player not found for test!");
        }
    }
    
    // Method mới để debug boss collider states
    private void DebugBossColliderStates()
    {
        Debug.Log($"[CheckpointManager] === BOSS COLLIDER STATES ===");
        
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
                var colliders = miniBoss.GetComponents<Collider2D>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    Debug.Log($"  MiniBoss {miniBoss.name} Collider {i}: {(colliders[i].enabled ? "ENABLED" : "DISABLED")}");
                }
            }
        }
        
        // Debug other bosses
        List<IBossResettable> bosses = GetCachedBosses(SceneManager.GetActiveScene().name);
        foreach (var boss in bosses)
        {
            if (boss != null && !(boss is MiniBoss))
            {
                var mono = boss as MonoBehaviour;
                if (mono != null)
                {
                    var colliders = mono.GetComponents<Collider2D>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Debug.Log($"  Boss {mono.name} Collider {i}: {(colliders[i].enabled ? "ENABLED" : "DISABLED")}");
                    }
                }
            }
        }
        
        Debug.Log($"[CheckpointManager] === END BOSS COLLIDER STATES ===");
    }

    // Copy all other existing methods from original CheckpointManager here...
    // (ResetLyraForMiniGame, ResetMiniGameObstacles, DelayedRestartMiniGame, etc.)
    
    private async void TrySaveToServer()
    {
        try
        {
            var player = FindFirstObjectByType<CharacterController2D>();
            if (player == null)
            {
                Debug.LogWarning("[Save] No CharacterController2D found.");
                return;
            }

            var dto = new PlayerSaveService.SaveGameDTO
            {
                userId = UserSession.Instance.UserId,
                health = player.life,
                maxHealth = player.maxLife,
                posX = player.transform.position.x,
                posY = player.transform.position.y,
                posZ = player.transform.position.z,
                lastCheckpointID = (int)CheckpointManager.Instance.lastCheckpointID,
                lastCheckpointScene = CheckpointManager.Instance.lastCheckpointScene,
            };

            var ok = await PlayerSaveService.SaveGameAsync(dto);
            Debug.Log(ok ? "[Save] Save uploaded." : "[Save] Save failed.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Save] Exception: " + e);
        }
    }

    // Coroutine để delay restart mini game
    private System.Collections.IEnumerator DelayedRestartMiniGame(SceneController sceneController)
    {
        yield return null;
        
        Debug.Log("[CheckpointManager] Starting delayed mini game restart...");
        
        if (sceneController != null)
        {
            sceneController.RestartMiniGame();
            Debug.Log("[CheckpointManager] Mini game restarted via SceneController");
        }
        else
        {
            Debug.LogWarning("[CheckpointManager] No SceneController found for mini game restart!");
        }
        
        Debug.Log("[CheckpointManager] ========== MINI GAME RESTART COMPLETED ==========");
    }

    // Method để reset Lyra về trạng thái ban đầu cho mini game
    private void ResetLyraForMiniGame()
    {
        LyraHealth lyraHealth = FindFirstObjectByType<LyraHealth>();
        if (lyraHealth != null)
        {
            lyraHealth.ResetLyra();
            Debug.Log("[CheckpointManager] Lyra health reset for mini game");
        }
        
        // Reset position của pet về vị trí spawn point
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("spw");
            if (spawnPoint != null)
            {
                pet.transform.position = spawnPoint.transform.position;
                Debug.Log($"[CheckpointManager] Pet moved to spawn point: {spawnPoint.transform.position}");
                
                Rigidbody2D petRb = pet.GetComponent<Rigidbody2D>();
                if (petRb != null)
                {
                    petRb.linearVelocity = Vector2.zero;
                }
            }
        }
    }

    // Method để reset các obstacles trong mini game
    private void ResetMiniGameObstacles()
    {
        Debug.Log("[CheckpointManager] Resetting mini game obstacles...");
        
        // QUAN TRỌNG: Reset Core Zones nhưng không reset completion status
        CoreZone[] coreZones = FindObjectsByType<CoreZone>(FindObjectsSortMode.None);
        foreach (var core in coreZones)
        {
            if (core != null)
            {
                core.ResetCoreForMiniGame();
            }
        }
        
        // Reset Laser Traps
        LaserManagerTrap[] laserTraps = Object.FindObjectsByType<LaserManagerTrap>(FindObjectsSortMode.None);
        foreach (var trap in laserTraps)
        {
            trap.ResetTrap();
        }

        // Reset Falling Blocks
        FallingBlockManager.ResetAllBlocks();

        // Reset Laser Activators
        LaserActivator[] activators = Object.FindObjectsByType<LaserActivator>(FindObjectsSortMode.None);
        foreach (var activator in activators)
        {
            activator.ResetTrigger();
        }
        
        // Reset Bomb Defuse Mini Games
        BombDefuseMiniGame[] allMiniGames = Object.FindObjectsByType<BombDefuseMiniGame>(FindObjectsSortMode.None);
        foreach (var miniGame in allMiniGames)
        {
            miniGame.ResetState();
        }
        
        Debug.Log("[CheckpointManager] Mini game obstacles reset completed");
    }

    // Method mới để reset hoàn toàn mini game (chỉ dùng khi bắt đầu lại từ đầu)
    public void ResetMiniGameCompletely()
    {
        Debug.Log("[CheckpointManager] 🔄 FULL MINI GAME RESET - Losing all progress");
        
        // Reset completion status của tất cả cores
        CoreZone[] coreZones = FindObjectsByType<CoreZone>(FindObjectsSortMode.None);
        foreach (var core in coreZones)
        {
            if (core != null)
            {
                core.ResetCompletion(); // Reset completion status
                core.ResetCoreForMiniGame();
            }
        }
        
        // Reset CoreManager hoàn toàn
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            coreManager.ResetCoreManager(); // Full reset
        }
        
        // Reset obstacles
        ResetMiniGameObstacles();
        
        Debug.Log("[CheckpointManager] Full mini game reset completed");
    }

    private void ResetBossesInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        int miniBossResetCount = 0;
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
                miniBoss.ResetBoss();
                miniBoss.enabled = false;
                
                MiniBossDamageReceiver damageReceiver = miniBoss.GetComponent<MiniBossDamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.enabled = false;
                }
                
                Rigidbody2D rb = miniBoss.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }
                
                miniBossResetCount++;
            }
        }
        
        List<IBossResettable> bosses = GetCachedBosses(currentScene);
        
        int resetCount = 0;
        foreach (var boss in bosses)
        {
            if (boss != null && ShouldResetBoss(boss) && !(boss is MiniBoss))
            {
                boss.ResetBoss();
                resetCount++;
            }
        }
        
        ResetTriggeredBosses();
        
        if (miniBossResetCount > 0 || resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {miniBossResetCount} MiniBoss(es) and {resetCount} other boss(es) in scene: {currentScene}");
        }
    }
    
    private void ResetTriggersInCurrentScene()
    {
        CameraZoomTrigger[] oldTriggers = Resources.FindObjectsOfTypeAll<CameraZoomTrigger>();
        SimpleCameraZoomTrigger[] simpleTriggers = Resources.FindObjectsOfTypeAll<SimpleCameraZoomTrigger>();
        BossZoneTrigger[] zoneTriggers = Resources.FindObjectsOfTypeAll<BossZoneTrigger>();
        
        int resetCount = 0;
        
        foreach (var trigger in oldTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
            }
        }
        
        foreach (var trigger in simpleTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
            }
        }
        
        foreach (var trigger in zoneTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
            }
        }
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} trigger(s) in scene");
        }
    }

    private List<IBossResettable> GetCachedBosses(string sceneName)
    {
        if (cachedBosses.ContainsKey(sceneName) && 
            Time.time - lastCacheTime < CACHE_DURATION)
        {
            cachedBosses[sceneName].RemoveAll(boss => boss == null);
            return cachedBosses[sceneName];
        }

        List<IBossResettable> foundBosses = new List<IBossResettable>();
        
        var allResettables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allResettables)
        {
            if (obj is IBossResettable boss)
            {
                foundBosses.Add(boss);
            }
        }

        cachedBosses[sceneName] = foundBosses;
        lastCacheTime = Time.time;
        
        return foundBosses;
    }

    private bool ShouldResetBoss(IBossResettable boss)
    {
        if (!resetOnlyActiveBosses) return true;
        
        if (boss is MonoBehaviour mono)
        {
            return mono.gameObject.activeInHierarchy && mono.enabled;
        }
        
        return true;
    }

    private void FinishRespawn(GameObject player)
    {
        CharacterController2D controller = player.GetComponent<CharacterController2D>();
        if (controller != null)
        {
            controller.RestoreFullLife();
        }
        
        BombDefuseMiniGame[] allMiniGames = Object.FindObjectsByType<BombDefuseMiniGame>(FindObjectsSortMode.None);
        foreach (var miniGame in allMiniGames)
        {
            miniGame.ResetState();
        }

        LaserManagerTrap[] laserTraps = Object.FindObjectsByType<LaserManagerTrap>(FindObjectsSortMode.None);
        foreach (var trap in laserTraps)
        {
            trap.ResetTrap();
        }

        FallingBlockManager.ResetAllBlocks();

        LaserActivator[] activators = Object.FindObjectsByType<LaserActivator>(FindObjectsSortMode.None);
        foreach (var activator in activators)
        {
            activator.ResetTrigger();
        }
        
        Debug.Log("[CheckpointManager] FinishRespawn completed");
    }
    
    private void ResetAllCombatZones()
    {
        BossCombatZone[] combatZones = FindObjectsByType<BossCombatZone>(FindObjectsSortMode.None);
        
        int resetCount = 0;
        foreach (var zone in combatZones)
        {
            if (zone != null)
            {
                zone.ResetZone();
                resetCount++;
            }
        }
        
        BossWarningManager warningManager = FindFirstObjectByType<BossWarningManager>();
        if (warningManager != null)
        {
            Debug.Log("[CheckpointManager] Reset BossWarningManager for respawn");
        }
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} Combat Zone(s)");
        }
    }

    // Method mới để reset Combat Zones cho boss area (preserve boss states)
    private void ResetAllCombatZonesForBossArea()
    {
        BossCombatZone[] combatZones = FindObjectsByType<BossCombatZone>(FindObjectsSortMode.None);
        
        int resetCount = 0;
        foreach (var zone in combatZones)
        {
            if (zone != null)
            {
                // QUAN TRỌNG: Chỉ reset zone state, không reset boss physical state
                zone.ResetZoneOnly(); // Method mới sẽ cần thêm vào BossCombatZone
                resetCount++;
            }
        }
        
        BossWarningManager warningManager = FindFirstObjectByType<BossWarningManager>();
        if (warningManager != null)
        {
            Debug.Log("[CheckpointManager] Reset BossWarningManager for boss area respawn");
        }
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} Combat Zone(s) for boss area");
        }
    }

    // Method mới để reset bosses nhưng preserve disabled colliders - IMPROVED
    private void ResetBossesInCurrentScenePreserveState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // QUAN TRỌNG: Thay vì preserve collider states, force DISABLE tất cả colliders
        // để đảm bảo boss về trạng thái mặc định (colliders tắt)
        
        // Reset MiniBoss
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        int miniBossResetCount = 0;
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
                miniBoss.ResetBoss();
                miniBoss.enabled = false;
                
                MiniBossDamageReceiver damageReceiver = miniBoss.GetComponent<MiniBossDamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.enabled = false;
                }
                
                Rigidbody2D rb = miniBoss.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }
                
                // QUAN TRỌNG: Force DISABLE tất cả colliders về trạng thái mặc định
                var colliders = miniBoss.GetComponents<Collider2D>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                    Debug.Log($"[CheckpointManager] Force disabled MiniBoss {miniBoss.name} collider (back to default state)");
                }
                
                miniBossResetCount++;
            }
        }
        
        // Reset other bosses
        List<IBossResettable> bosses = GetCachedBosses(currentScene);
        int resetCount = 0;
        foreach (var boss in bosses)
        {
            if (boss != null && ShouldResetBoss(boss) && !(boss is MiniBoss))
            {
                boss.ResetBoss();
                
                // QUAN TRỌNG: Force DISABLE tất cả colliders về trạng thái mặc định
                var mono = boss as MonoBehaviour;
                if (mono != null)
                {
                    var colliders = mono.GetComponents<Collider2D>();
                    foreach (var col in colliders)
                    {
                        col.enabled = false;
                        Debug.Log($"[CheckpointManager] Force disabled Boss {mono.name} collider (back to default state)");
                    }
                }
                
                resetCount++;
            }
        }
        
        ResetTriggeredBosses();
        
        if (miniBossResetCount > 0 || resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {miniBossResetCount} MiniBoss(es) and {resetCount} other boss(es) with DISABLED colliders (default state)");
        }
    }

    private void LoadSceneWithCleanup(string sceneName, GameObject player)
    {
        cachedBosses.Clear();
        CleanupScene();

        SceneManager.LoadSceneAsync(sceneName).completed += (op) =>
        {
            player.transform.position = lastCheckpointPosition;
            FinishRespawn(player);
        };
    }

    private void CleanupScene()
    {
        var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("PersistentObject")) continue;
            if (obj == this.gameObject) continue;

            if (obj.name.Contains("UIRoot") || obj.name.Contains("AudioManager"))
            {
                Destroy(obj);
            }
        }
    }

    public void RefreshBossCache()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (cachedBosses.ContainsKey(currentScene))
        {
            cachedBosses.Remove(currentScene);
        }
        lastCacheTime = 0f;
    }

    public void ResetSpecificBoss(IBossResettable boss)
    {
        if (boss != null && enableBossReset)
        {
            boss.ResetBoss();
            Debug.Log($"[CheckpointManager] Manually reset boss: {(boss as MonoBehaviour)?.name}");
        }
    }

    public void SetBossResetEnabled(bool enabled)
    {
        enableBossReset = enabled;
        Debug.Log($"[CheckpointManager] Boss reset {(enabled ? "enabled" : "disabled")}");
    }

    private void ResetTriggeredBosses()
    {
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (var spawner in spawners)
        {
            var resetMethod = spawner.GetType().GetMethod("ResetSpawnedBoss");
            if (resetMethod != null)
            {
                try
                {
                    resetMethod.Invoke(spawner, null);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[CheckpointManager] Failed to reset spawner {spawner.name}: {e.Message}");
                }
            }
        }
    }
}