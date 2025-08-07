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
        
        // Reset Combat Zones first (this will deactivate all bosses)
        ResetAllCombatZones();
        
        // Reset triggers (but DON'T trigger warning automatically)
        ResetTriggersInCurrentScene();
        
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
        
        if (resetCount > 0)
        {
            Debug.Log($"[CheckpointManager] Reset {resetCount} Combat Zone(s)");
        }
    }
    
    // Method để đảm bảo tất cả boss scripts bị tắt sau respawn
    private void DisableAllBossScripts()
    {
        // Tắt tất cả MiniBoss
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
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
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        
        // Tắt tất cả boss controller khác (nếu có)
        MonoBehaviour[] allBossControllers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var controller in allBossControllers)
        {
            if (controller.GetType().Name.Contains("Boss") && 
                !controller.GetType().Name.Contains("Manager") &&
                !(controller is MiniBoss))
            {
                controller.enabled = false;
            }
        }
        
        Debug.Log("[CheckpointManager] All boss scripts disabled after respawn");
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
}