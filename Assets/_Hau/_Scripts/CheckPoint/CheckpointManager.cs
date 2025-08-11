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
    public bool resetOnlyActiveBosses = true; // Chỉ reset boss đã active/spawned
    
    // Cache để tránh tìm kiếm lại liên tục
    private Dictionary<string, List<IBossResettable>> cachedBosses = new Dictionary<string, List<IBossResettable>>();
    private float lastCacheTime = 0f;
    private const float CACHE_DURATION = 2f; // Cache trong 2 giây

    public void SetCurrentCheckpoint(CheckPointEnum id, Vector3 position, string sceneName)
    {
        lastCheckpointID = id;
        lastCheckpointPosition = position;
        lastCheckpointScene = sceneName;
    }

    // Method mới để restart mini game
    public void RestartMiniGame()
    {
        Debug.Log("[CheckpointManager] ========== STARTING MINI GAME RESTART ==========");
        
        // QUAN TRỌNG: Reset theo thứ tự để tránh trigger hoàn thành
        
        // 1. Tìm và force stop bất kỳ mini game nào đang chạy
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && sceneController.IsInMiniGame())
        {
            Debug.Log("[CheckpointManager] Force stopping current mini game...");
            sceneController.ReturnControlToPlayer();
        }
        
        // 2. Reset Lyra health và position trước
        ResetLyraForMiniGame();
        
        // 3. Reset các obstacles và traps trước
        ResetMiniGameObstacles();
        
        // 4. Reset CoreManager sau cùng
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            Debug.Log("[CheckpointManager] Resetting CoreManager...");
            coreManager.ResetCoreManager();
        }
        
        // 5. Reset các triggers và combat zones
        ResetTriggersInCurrentScene();
        ResetAllCombatZones();
        
        // 6. Đợi 1 frame rồi mới restart mini game
        StartCoroutine(DelayedRestartMiniGame(sceneController));
    }
    
    // Coroutine để delay restart mini game
    private System.Collections.IEnumerator DelayedRestartMiniGame(SceneController sceneController)
    {
        yield return null; // Đợi 1 frame để đảm bảo tất cả reset xong
        
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
            }
        }
    }

    // Method để reset các obstacles trong mini game
    private void ResetMiniGameObstacles()
    {
        Debug.Log("[CheckpointManager] Resetting mini game obstacles...");
        
        // QUAN TRỌNG: Reset Core Zones trước để đảm bảo chúng về trạng thái ban đầu
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

    public void RespawnPlayer(GameObject player)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"[CheckpointManager] Starting respawn in scene: {currentScene}");

        if (lastCheckpointScene != currentScene)
        {
            LoadSceneWithCleanup(lastCheckpointScene, player);
        }
        else
        {
            // QUAN TRỌNG: Reset Combat Zones TRƯỚC để đảm bảo boss có thể được kích hoạt lại
            ResetAllCombatZones();
            
            // Reset bosses trong scene hiện tại (nếu có)
            if (enableBossReset)
            {
                ResetBossesInCurrentScene();
            }
            
            // Reset triggers về trạng thái respawn - QUAN TRỌNG: Gọi trước khi move player
            ResetTriggersInCurrentScene();
            
            player.transform.position = lastCheckpointPosition;
            Debug.Log($"[CheckpointManager] Player moved to checkpoint position: {lastCheckpointPosition}");
            
            FinishRespawn(player);
        }
    }

    private void ResetBossesInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Reset MiniBoss trực tiếp
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        int miniBossResetCount = 0;
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
                miniBoss.ResetBoss();
                // Tắt script MiniBoss sau khi reset
                miniBoss.enabled = false;
                
                // Tắt damage receiver
                MiniBossDamageReceiver damageReceiver = miniBoss.GetComponent<MiniBossDamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.enabled = false;
                }
                
                // Set Rigidbody về Static
                Rigidbody2D rb = miniBoss.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }
                
                miniBossResetCount++;
            }
        }
        
        // Sử dụng cache cho các boss khác
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
        
        // Reset cả những boss được spawn bởi trigger (nếu có BossSpawner system của bạn)
        ResetTriggeredBosses();
        
        if (miniBossResetCount > 0 || resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {miniBossResetCount} MiniBoss(es) and {resetCount} other boss(es) in scene: {currentScene}");
        }
    }
    
    // Method mới để reset các Trigger
    private void ResetTriggersInCurrentScene()
    {
        // Reset old triggers
        CameraZoomTrigger[] oldTriggers = Resources.FindObjectsOfTypeAll<CameraZoomTrigger>();
        SimpleCameraZoomTrigger[] simpleTriggers = Resources.FindObjectsOfTypeAll<SimpleCameraZoomTrigger>();
        BossZoneTrigger[] zoneTriggers = Resources.FindObjectsOfTypeAll<BossZoneTrigger>();
        
        int resetCount = 0;
        
        // Reset old triggers
        foreach (var trigger in oldTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
                Debug.Log($"[CheckpointManager] Reset old trigger: {trigger.gameObject.name}");
            }
        }
        
        // Reset simple triggers  
        foreach (var trigger in simpleTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
                Debug.Log($"[CheckpointManager] Reset simple trigger: {trigger.gameObject.name}");
            }
        }
        
        // Reset zone triggers
        foreach (var trigger in zoneTriggers)
        {
            if (trigger != null && trigger.gameObject.scene.isLoaded)
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                resetCount++;
                Debug.Log($"[CheckpointManager] Reset zone trigger: {trigger.gameObject.name}");
            }
        }
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} trigger(s) in scene");
        }
        else
        {
            Debug.LogWarning("[CheckpointManager] No triggers found to reset!");
        }
    }

    private List<IBossResettable> GetCachedBosses(string sceneName)
    {
        // Kiểm tra cache có còn hiệu lực không
        if (cachedBosses.ContainsKey(sceneName) && 
            Time.time - lastCacheTime < CACHE_DURATION)
        {
            // Lọc bỏ null references
            cachedBosses[sceneName].RemoveAll(boss => boss == null);
            return cachedBosses[sceneName];
        }

        // Tạo cache mới
        List<IBossResettable> foundBosses = new List<IBossResettable>();
        
        // Tìm tất cả objects implement IBossResettable
        var allResettables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allResettables)
        {
            if (obj is IBossResettable boss)
            {
                foundBosses.Add(boss);
            }
        }

        // Cập nhật cache
        cachedBosses[sceneName] = foundBosses;
        lastCacheTime = Time.time;
        
        return foundBosses;
    }

    private bool ShouldResetBoss(IBossResettable boss)
    {
        if (!resetOnlyActiveBosses) return true;
        
        // Kiểm tra boss có đang active không
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
        
        // QUAN TRỌNG: Không gọi ResetAllCombatZones ở đây nữa vì đã gọi trong RespawnPlayer
        
        // Reset triggers (but DON'T trigger warning automatically)
        // ResetTriggersInCurrentScene(); // Đã gọi trong RespawnPlayer
        
        // NOTE: Không gọi warning ở đây nữa, để Combat Zone tự quản lý

        // 👉 Reset hệ thống tường và minigame nếu có
        BombDefuseMiniGame[] allMiniGames = Object.FindObjectsByType<BombDefuseMiniGame>(FindObjectsSortMode.None);
        foreach (var miniGame in allMiniGames)
        {
            miniGame.ResetState();
        }

        // ✅ Reset các Laser Trap (không cần interface)
        LaserManagerTrap[] laserTraps = Object.FindObjectsByType<LaserManagerTrap>(FindObjectsSortMode.None);

        foreach (var trap in laserTraps)
        {
            trap.ResetTrap();
        }

        // ✅ Reset Falling Blocks
        FallingBlockManager.ResetAllBlocks();

        // ✅ Reset lại các LaserActivator trigger
        LaserActivator[] activators = Object.FindObjectsByType<LaserActivator>(FindObjectsSortMode.None);
        foreach (var activator in activators)
        {
            activator.ResetTrigger();
        }
        
        // THÊM: Kiểm tra và reset SceneController nếu có mini game đang chạy
        ResetMiniGameIfActive();
    }
    
    // Method để reset tất cả Combat Zones
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
        
        // QUAN TRỌNG: Reset cả BossWarningManager để đảm bảo warning có thể trigger lại
        BossWarningManager warningManager = FindFirstObjectByType<BossWarningManager>();
        if (warningManager != null)
        {
            // Reset warning manager để có thể trigger lại warning khi vào combat zone
            Debug.Log("[CheckpointManager] Reset BossWarningManager for respawn");
        }
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} Combat Zone(s)");
        }
    }

    private void LoadSceneWithCleanup(string sceneName, GameObject player)
    {
        // Clear cache khi chuyển scene
        cachedBosses.Clear();
        
        // Cleanup các đối tượng không cần giữ lại
        CleanupScene();

        // Load scene mới
        SceneManager.LoadSceneAsync(sceneName).completed += (op) =>
        {
            // Scene mới sẽ có bosses ở trạng thái ban đầu
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

            // Cleanup logic tùy chỉnh
            if (obj.name.Contains("UIRoot") || obj.name.Contains("AudioManager"))
            {
                Destroy(obj);
            }
        }
    }

    // Method để force refresh cache (gọi khi có boss mới spawn)
    public void RefreshBossCache()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (cachedBosses.ContainsKey(currentScene))
        {
            cachedBosses.Remove(currentScene);
        }
        lastCacheTime = 0f; // Force refresh
    }

    // Method để manual reset một boss cụ thể
    public void ResetSpecificBoss(IBossResettable boss)
    {
        if (boss != null && enableBossReset)
        {
            boss.ResetBoss();
            Debug.Log($"[CheckpointManager] Manually reset boss: {(boss as MonoBehaviour)?.name}");
        }
    }

    // Method để bật/tắt boss reset
    public void SetBossResetEnabled(bool enabled)
    {
        enableBossReset = enabled;
        Debug.Log($"[CheckpointManager] Boss reset {(enabled ? "enabled" : "disabled")}");
    }

    // Method để reset những boss được spawn bởi trigger system có sẵn của bạn
    private void ResetTriggeredBosses()
    {
        // Tìm tất cả các spawner trong scene (giả sử bạn có BossSpawner hoặc tương tự)
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (var spawner in spawners)
        {
            // Kiểm tra nếu spawner có method ResetSpawnedBoss
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
    
    // Method mới để reset mini game nếu đang active
    private void ResetMiniGameIfActive()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && sceneController.IsInMiniGame())
        {
            Debug.Log("[CheckpointManager] Mini game was active during respawn - resetting to normal gameplay");
            
            // Force return to normal gameplay
            sceneController.ReturnControlToPlayer();
            
            // Reset Lyra health để đảm bảo không còn game over panel
            LyraHealth lyraHealth = FindFirstObjectByType<LyraHealth>();
            if (lyraHealth != null)
            {
                lyraHealth.ResetLyra();
            }
        }
    }
}