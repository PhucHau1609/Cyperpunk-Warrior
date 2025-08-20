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
        // Special handling for Map_Boss02
        else if (sceneName == "Map_Boss02")
        {
            HandleMapBoss02Checkpoint(id);
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

    private void HandleMapBoss02Checkpoint(CheckPointEnum checkpointID)
    {
        Debug.Log($"[CheckpointManager] Map_Boss02: Reached checkpoint {checkpointID}");

        // Map_Boss02 chỉ có 1 checkpoint cho boss area
        // Disable pet following vì không có pet trong scene này
        DisablePetFollowing();

        // Force GameStateManager to Gameplay
        ForceResetGameStateToGameplay();

        Debug.Log("[CheckpointManager] Map_Boss02: Boss checkpoint setup completed");
    }

    public void RespawnPlayer(GameObject player)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"[CheckpointManager] ⚠️ RespawnPlayer called in scene: {currentScene}");

        // QUAN TRỌNG: FORCE RESET GAMESTATE NGAY LẬP TỨC cho tất cả scenes
        ForceResetGameStateToGameplay();

        // Special handling for different scenes
        if (currentScene == "MapBoss_01Test")
        {
            HandleMapBoss01TestRespawn(player);
            return;
        }
        else if (currentScene == "Map_Boss02")
        {
            HandleMapBoss02Respawn(player);
            return;
        }

        // Original respawn logic for other scenes
        HandleNormalRespawn(player, currentScene);
    }

    private void HandleMapBoss02Respawn(GameObject player)
    {
        Debug.Log("[CheckpointManager] === HANDLING MAP_BOSS02 RESPAWN ===");

        // QUAN TRỌNG: Force reset GameState trước
        ForceResetGameStateToGameplay();

        // Ensure no pet following (Map_Boss02 doesn't have pets)
        DisablePetFollowing();

        // Reset Boss2 và hands
        ResetBoss2AndHands();

        // Reset Combat Zones
        ResetAllCombatZones();

        // Reset triggers 
        ResetTriggersInCurrentScene();

        // Move player to checkpoint
        player.transform.position = lastCheckpointPosition;
        Debug.Log($"[CheckpointManager] Player moved to Map_Boss02 checkpoint: {lastCheckpointPosition}");

        // Finish respawn (restore health, etc.)
        FinishRespawn(player);

        Debug.Log("[CheckpointManager] === MAP_BOSS02 RESPAWN COMPLETED ===");
    }

    private void ResetBoss2AndHands()
    {
        Debug.Log("[CheckpointManager] Resetting Boss2 and hands...");

        // Reset Boss2Controller
        Boss2Controller boss2 = FindFirstObjectByType<Boss2Controller>();
        if (boss2 != null)
        {
            Debug.Log("[CheckpointManager] Resetting Boss2Controller");
            boss2.ResetBoss();

            // QUAN TRỌNG: Disable boss initially (sẽ được activate khi player vào zone)
            boss2.enabled = false;

            // Reset shield state
            if (boss2.shield != null)
            {
                boss2.shield.SetActive(false);
            }

            // Reset Rigidbody
            Rigidbody2D boss2Rb = boss2.GetComponent<Rigidbody2D>();
            if (boss2Rb != null)
            {
                boss2Rb.bodyType = RigidbodyType2D.Static;
                boss2Rb.linearVelocity = Vector2.zero;
            }

            // QUAN TRỌNG: Reset colliders về trạng thái mặc định (disabled)
            Collider2D[] boss2Colliders = boss2.GetComponents<Collider2D>();
            foreach (var col in boss2Colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                    Debug.Log($"[CheckpointManager] Boss2 collider disabled: {col.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[CheckpointManager] Boss2Controller not found!");
        }

        // Reset tất cả Boss2HandController
        Boss2HandController[] hands = FindObjectsByType<Boss2HandController>(FindObjectsSortMode.None);
        int handResetCount = 0;

        foreach (var hand in hands)
        {
            if (hand != null)
            {
                Debug.Log($"[CheckpointManager] Resetting hand: {hand.gameObject.name}");
                hand.ResetHand();

                // QUAN TRỌNG: Disable hand initially
                hand.enabled = false;

                // Reset hand colliders
                Collider2D handCollider = hand.GetComponent<Collider2D>();
                if (handCollider != null)
                {
                    handCollider.enabled = false;
                    Debug.Log($"[CheckpointManager] Hand collider disabled: {hand.gameObject.name}");
                }

                // Reset hand rigidbody
                Rigidbody2D handRb = hand.GetComponent<Rigidbody2D>();
                if (handRb != null)
                {
                    handRb.bodyType = RigidbodyType2D.Static;
                    handRb.linearVelocity = Vector2.zero;
                }

                handResetCount++;
            }
        }

        // Reset Boss2 health bar
        Boss2HealthBar healthBar = FindFirstObjectByType<Boss2HealthBar>();
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(1f); // Full health
            Debug.Log("[CheckpointManager] Boss2 health bar reset to full");
        }

        Debug.Log($"[CheckpointManager] Boss2 reset completed. Reset {handResetCount} hand(s)");
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

        Debug.Log($"[CheckpointManager] === SMART DISABLE COMPLETED: {disabledCount} trigger(s) disabled ===");
    }

    private void DisablePetFollowing()
    {
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            pet.Disappear(); // Use existing Disappear method
            Debug.Log("[CheckpointManager] Pet disabled");
        }
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

    // [REST OF THE ORIGINAL METHODS REMAIN THE SAME...]
    // Copy all other existing methods from original CheckpointManager here...

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

    private void SetMiniGameProtection(bool active)
    {
        miniGameProtectionActive = active;
        Debug.Log($"[CheckpointManager] Mini game protection: {(active ? "ACTIVE" : "INACTIVE")}");
    }

    public bool IsMiniGameProtected()
    {
        return miniGameProtectionActive;
    }

    private System.Collections.IEnumerator DelayedResetProtection()
    {
        yield return new WaitForSeconds(3f);
        SetMiniGameProtection(false);
        Debug.Log("[CheckpointManager] Mini game protection reset after delay");
    }

    private void ForceResetAllMiniGameStates()
    {
        Debug.Log("[CheckpointManager] === FORCE RESETTING ALL MINI GAME STATES ===");

        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            Debug.Log("[CheckpointManager] Force resetting SceneController state");
            sceneController.ForceResetMiniGameState();

            if (sceneController.IsInMiniGame())
            {
                Debug.Log("[CheckpointManager] SceneController still thinks in mini game - force returning control");
                sceneController.ReturnControlToPlayer(true);
            }
        }

        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[CheckpointManager] GameStateManager current state: {GameStateManager.Instance.CurrentState}");
            if (GameStateManager.Instance.CurrentState == GameState.MiniGame)
            {
                Debug.Log("[CheckpointManager] Force resetting GameStateManager to Gameplay");
                GameStateManager.Instance.ResetToGameplay();
            }
        }

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

        if (InventoryUIHandler.Instance != null)
        {
            InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && CameraFollow.Instance != null)
        {
            Debug.Log("[CheckpointManager] Force setting camera target to player");
            CameraFollow.Instance.Target = playerObj.transform;
        }

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
            if (checkpoint.checkpointID == CheckPointEnum.CheckPoint_1)
            {
                return checkpoint;
            }
        }

        return null;
    }

    public void RestartMiniGame()
    {
        Debug.Log("[CheckpointManager] 🎮 RestartMiniGame called (MINI GAME RESTART)");
        Debug.Log("[CheckpointManager] ========== STARTING MINI GAME RESTART ==========");

        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && sceneController.IsInMiniGame())
        {
            Debug.Log("[CheckpointManager] Force stopping current mini game...");
            sceneController.ReturnControlToPlayer(false);
        }

        ResetLyraForMiniGame();
        ResetMiniGameObstacles();

        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            Debug.Log("[CheckpointManager] Resetting CoreManager while preserving progress...");
            coreManager.ResetCoreManagerKeepProgress();
        }

        ResetTriggersInCurrentScene();
        ResetAllCombatZones();

        StartCoroutine(DelayedRestartMiniGame(sceneController));
    }

    public void ResetMiniGameCompletion()
    {
        miniGameCompleted = false;
        Debug.Log("[CheckpointManager] Mini game completion status reset");
    }

    public void DebugMapBoss01TestStatus()
    {
        if (SceneManager.GetActiveScene().name == "MapBoss_01Test")
        {
            Debug.Log($"[CheckpointManager] === MapBoss_01Test Status ===");
            Debug.Log($"  - Mini Game Completed: {miniGameCompleted}");
            Debug.Log($"  - Last Checkpoint ID: {lastCheckpointID}");
            Debug.Log($"  - Last Checkpoint Position: {lastCheckpointPosition}");

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

            DebugBossColliderStates();

            Debug.Log($"[CheckpointManager] === END MapBoss_01Test Status ===");
        }
    }

    // Method để debug Map_Boss02 status
    public void DebugMapBoss02Status()
    {
        if (SceneManager.GetActiveScene().name == "Map_Boss02")
        {
            Debug.Log($"[CheckpointManager] === Map_Boss02 Status ===");
            Debug.Log($"  - Last Checkpoint ID: {lastCheckpointID}");
            Debug.Log($"  - Last Checkpoint Position: {lastCheckpointPosition}");

            if (GameStateManager.Instance != null)
            {
                Debug.Log($"  - GameStateManager Current State: {GameStateManager.Instance.CurrentState}");
                Debug.Log($"  - Is Gameplay: {GameStateManager.Instance.IsGameplay}");
            }

            // Debug Boss2 state
            Boss2Controller boss2 = FindFirstObjectByType<Boss2Controller>();
            if (boss2 != null)
            {
                Debug.Log($"  - Boss2 Enabled: {boss2.enabled}");
                Debug.Log($"  - Boss2 Is Attacking: {boss2.isAttacking}");
                Debug.Log($"  - Boss2 Shield Active: {boss2.IsShieldActive()}");

                Boss2DamageReceiver boss2Damage = boss2.GetComponent<Boss2DamageReceiver>();
                if (boss2Damage != null)
                {
                    Debug.Log($"  - Boss2 Current HP: {boss2Damage.CurrentHP}/{boss2Damage.MaxHP}");
                }
            }
            else
            {
                Debug.Log("  - Boss2Controller: NOT FOUND");
            }

            // Debug hands
            Boss2HandController[] hands = FindObjectsByType<Boss2HandController>(FindObjectsSortMode.None);
            Debug.Log($"  - Hand Count: {hands.Length}");
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i] != null)
                {
                    Debug.Log($"    Hand {i}: {hands[i].name} - Enabled: {hands[i].enabled} - Dead: {hands[i].IsDead()}");
                }
            }

            Debug.Log($"[CheckpointManager] === END Map_Boss02 Status ===");
        }
    }

    public void ManualForceResetGameStateToGameplay()
    {
        Debug.Log("[CheckpointManager] === MANUAL FORCE RESET GAMESTATE ===");
        ForceResetGameStateToGameplay();
        Debug.Log("[CheckpointManager] === MANUAL RESET COMPLETED ===");
    }

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

    private void ResetLyraForMiniGame()
    {
        LyraHealth lyraHealth = FindFirstObjectByType<LyraHealth>();
        if (lyraHealth != null)
        {
            lyraHealth.ResetLyra();
            Debug.Log("[CheckpointManager] Lyra health reset for mini game");
        }

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

    private void ResetMiniGameObstacles()
    {
        Debug.Log("[CheckpointManager] Resetting mini game obstacles...");

        CoreZone[] coreZones = FindObjectsByType<CoreZone>(FindObjectsSortMode.None);
        foreach (var core in coreZones)
        {
            if (core != null)
            {
                core.ResetCoreForMiniGame();
            }
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

        BombDefuseMiniGame[] allMiniGames = Object.FindObjectsByType<BombDefuseMiniGame>(FindObjectsSortMode.None);
        foreach (var miniGame in allMiniGames)
        {
            miniGame.ResetState();
        }

        Debug.Log("[CheckpointManager] Mini game obstacles reset completed");
    }

    public void ResetMiniGameCompletely()
    {
        Debug.Log("[CheckpointManager] 🔄 FULL MINI GAME RESET - Losing all progress");

        CoreZone[] coreZones = FindObjectsByType<CoreZone>(FindObjectsSortMode.None);
        foreach (var core in coreZones)
        {
            if (core != null)
            {
                core.ResetCompletion();
                core.ResetCoreForMiniGame();
            }
        }

        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            coreManager.ResetCoreManager();
        }

        ResetMiniGameObstacles();

        Debug.Log("[CheckpointManager] Full mini game reset completed");
    }

    private void ResetBossesInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Special handling for Map_Boss02
        if (currentScene == "Map_Boss02")
        {
            ResetBoss2AndHands();
            return;
        }

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

    private void ResetAllCombatZonesForBossArea()
    {
        BossCombatZone[] combatZones = FindObjectsByType<BossCombatZone>(FindObjectsSortMode.None);

        int resetCount = 0;
        foreach (var zone in combatZones)
        {
            if (zone != null)
            {
                zone.ResetZoneOnly();
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

    private void ResetBossesInCurrentScenePreserveState()
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

                var colliders = miniBoss.GetComponents<Collider2D>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                    Debug.Log($"[CheckpointManager] Force disabled MiniBoss {miniBoss.name} collider (back to default state)");
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
}