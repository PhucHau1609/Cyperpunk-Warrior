using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CheckpointManager : HauSingleton<CheckpointManager>
{
    public CheckPointEnum lastCheckpointID;
    public Vector3 lastCheckpointPosition;
    public string lastCheckpointScene;

    [Header("Boss Reset Settings")]
    public bool enableBossReset = true;
    public bool resetOnlyActiveBosses = true;

    [Header("MapBoss_01Test Special Settings")]
    public bool miniGameCompleted = false;
    private Dictionary<string, List<IBossResettable>> cachedBosses = new Dictionary<string, List<IBossResettable>>();
    private float lastCacheTime = 0f;
    private const float CACHE_DURATION = 2f;

    private float _lastSaveTime;
    [SerializeField] private float saveCooldown = 1.0f; // 1s tránh spam

    public void SetCurrentCheckpoint(CheckPointEnum id, Vector3 position, string sceneName)
    {
        lastCheckpointID = id;
        lastCheckpointPosition = position;
        lastCheckpointScene = sceneName;
        if (sceneName == "MapBoss_01Test")
        {
            HandleMapBoss01TestCheckpoint(id);
        }

        // >>> GỬI SAVE LÊN SERVER
        if (Time.unscaledTime - _lastSaveTime > saveCooldown)
        {
            _lastSaveTime = Time.unscaledTime;
            TrySaveToServer(); 
        }
    }

    // ví dụ triển khai nhanh TrySaveToServer:
    private void TrySaveToServer()
    {
        if (UserSession.Instance == null) return;

        var dto = new PlayerSaveService.SaveGameDTO
        {
            userId = UserSession.Instance.UserId,
            posX = lastCheckpointPosition.x,
            posY = lastCheckpointPosition.y,
            posZ = lastCheckpointPosition.z,
            lastCheckpointID = (int)lastCheckpointID,
            lastCheckpointScene = lastCheckpointScene,
            health = CurrentHealthProvider()?.life ?? 100f,
            maxHealth = CurrentHealthProvider()?.maxLife ?? 100f,
            unlockedSkills = SkillManagerUI.Instance
                .GetUnlockedSkills()
                .Distinct()
                .Select(s => (int)s)
                .ToList()
        };

        StartCoroutine(CoSave(dto));
    }

    private IEnumerator CoSave(PlayerSaveService.SaveGameDTO dto)
    {
        var task = PlayerSaveService.SaveGameAsync(dto);
        while (!task.IsCompleted) yield return null;

        if (!task.Result) Debug.LogWarning("[Checkpoint] Save failed");
    }

    private void HandleMapBoss01TestCheckpoint(CheckPointEnum checkpointID)
    {
        if (checkpointID == CheckPointEnum.CheckPoint_1)
        {
            miniGameCompleted = true;
            DisableAutoMiniGameTriggers();
            DisablePetFollowing();
            ForceResetGameStateToGameplay();
        }
        else if (checkpointID == CheckPointEnum.CheckPoint_0)
        {
        }
    }
    private void DisableAutoMiniGameTriggers()
    {
        int disabledCount = 0;
        PlayerToNPCTrigger[] npcTriggers = FindObjectsByType<PlayerToNPCTrigger>(FindObjectsSortMode.None);
        foreach (var trigger in npcTriggers)
        {
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
                disabledCount++;
            }
        }

        Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var collider in allColliders)
        {
            if (collider != null && collider.isTrigger && collider.gameObject.activeInHierarchy)
            {
                if (collider.GetComponent<NPCDisableTrigger2D>() != null ||
                    collider.GetComponent<BombDefuseMiniGame>() != null)
                {
                    continue;
                }

                string objName = collider.gameObject.name.ToLower();
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
                    disabledCount++;
                }
            }
        }

        MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in allScripts)
        {
            if (script != null && script.gameObject.activeInHierarchy)
            {
                if (script is NPCDisableTrigger2D || script is BombDefuseMiniGame)
                {
                    continue;
                }

                string scriptType = script.GetType().Name.ToLower();
                bool isMiniGameScript = (scriptType.Contains("minigame") ||
                                        scriptType.Contains("petcontrol") ||
                                        scriptType.Contains("npccontrol") ||
                                        scriptType.Contains("triggertopet") ||
                                        scriptType.Contains("switchtonpc")) &&
                                    script != this;
                if (isMiniGameScript)
                {
                    script.gameObject.SetActive(false);
                    disabledCount++;
                }
            }
        }

        Component[] allComponents = FindObjectsByType<Component>(FindObjectsSortMode.None);
        foreach (var component in allComponents)
        {
            if (component != null && component.gameObject.activeInHierarchy)
            {
                if (component is NPCDisableTrigger2D || component is BombDefuseMiniGame)
                {
                    continue;
                }

                var componentType = component.GetType();
                if (componentType.Name.Contains("Trigger") &&
                    !componentType.Name.Contains("Collider") &&
                    component.gameObject.GetComponent<Collider2D>() != null &&
                    component.gameObject.GetComponent<Collider2D>().isTrigger)
                {
                    string objName = component.gameObject.name.ToLower();
                    if (objName.Contains("start") || objName.Contains("begin") || objName.Contains("init"))
                    {
                        component.gameObject.SetActive(false);
                        disabledCount++;
                    }
                }
            }
        }

        try
        {
            GameObject[] triggerZones = GameObject.FindGameObjectsWithTag("TriggerZone");
            GameObject[] miniGameTriggers = GameObject.FindGameObjectsWithTag("MiniGameTrigger");

            foreach (var zone in triggerZones)
            {
                if (zone != null && zone.activeInHierarchy)
                {
                    if (zone.GetComponent<NPCDisableTrigger2D>() != null ||
                        zone.GetComponent<BombDefuseMiniGame>() != null)
                    {
                        continue;
                    }

                    zone.SetActive(false);
                    disabledCount++;
                }
            }

            foreach (var trigger in miniGameTriggers)
            {
                if (trigger != null && trigger.activeInHierarchy)
                {
                    if (trigger.GetComponent<NPCDisableTrigger2D>() != null ||
                        trigger.GetComponent<BombDefuseMiniGame>() != null)
                    {
                        continue;
                    }

                    trigger.SetActive(false);
                    disabledCount++;
                }
            }
        }
        catch
        {
        }

        ForceDisableActiveMiniGameComponents();
    }
    private void ForceDisableActiveMiniGameComponents()
    {
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            PetManualControl petControl = pet.GetComponent<PetManualControl>();
            if (petControl != null && petControl.enabled)
            {
                petControl.enabled = false;
            }

            PetShooting petShooting = pet.GetComponent<PetShooting>();
            if (petShooting != null && petShooting.enabled)
            {
                petShooting.enabled = false;
            }

            LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
            if (lyraHealth != null && lyraHealth.enabled)
            {
                lyraHealth.enabled = false;
                if (lyraHealth.healthBarUI != null)
                    lyraHealth.healthBarUI.gameObject.SetActive(false);
            }
        }
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
    }

    private void DisablePetFollowing()
    {
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            pet.Disappear();
        }
    }

    public void RespawnPlayer(GameObject player)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        ForceResetGameStateToGameplay();
        
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
        
        HandleNormalRespawn(player, currentScene);
    }
    private void ForceResetGameStateToGameplay()
    {
        if (GameStateManager.Instance != null)
        {
            GameState currentState = GameStateManager.Instance.CurrentState;
            if (currentState != GameState.Gameplay)
            {
                GameStateManager.Instance.ForceResetToGameplay();
                GameState newState = GameStateManager.Instance.CurrentState;
                if (newState != GameState.Gameplay)
                {
                    GameStateManager.Instance.ForceSetState(GameState.Gameplay);
                }
            }
            else
            {
            }
        }
        else
        {
        }
    }

    private void HandleMapBoss01TestRespawn(GameObject player)
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (sceneController != null)
        {
            sceneController.DebugCurrentState();
        }
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Gameplay)
        {
            GameStateManager.Instance.ForceResetToGameplay();
        }
        bool isCurrentlyInMiniGame = false;
        if (miniGameCompleted)
        {
            isCurrentlyInMiniGame = false;
        }
        else
        {
            isCurrentlyInMiniGame = (sceneController != null && coreManager != null &&
                                    sceneController.IsInMiniGame());
        }
        if (isCurrentlyInMiniGame && !miniGameCompleted)
        {
            RestartMiniGame();
            return;
        }
        if (miniGameCompleted && lastCheckpointID == CheckPointEnum.CheckPoint_1)
        {
            RespawnAtBossCheckpoint(player);
            return;
        }
        if (miniGameCompleted)
        {
            Checkpoint bossCheckpoint = FindBossCheckpoint();
            if (bossCheckpoint != null)
            {
                player.transform.position = bossCheckpoint.spawnPoint.position;
                SetCurrentCheckpoint(bossCheckpoint.checkpointID, bossCheckpoint.spawnPoint.position, "MapBoss_01Test");
                RespawnAtBossCheckpoint(player);
            }
            else
            {
                HandleNormalRespawn(player, "MapBoss_01Test");
            }
            return;
        }
        HandleNormalRespawn(player, "MapBoss_01Test");
    }
    private void RespawnAtBossCheckpoint(GameObject player)
    {
        ForceResetAllMiniGameStates();
        DisableAutoMiniGameTriggers();
        SetMiniGameProtection(true);
        ResetAllCombatZonesForBossArea();
        if (enableBossReset)
        {
            ResetBossesInCurrentScenePreserveState();
        }
        ResetTriggersInCurrentScene();
        player.transform.position = lastCheckpointPosition;
        DisablePetFollowing();
        FinishRespawn(player);
        StartCoroutine(DelayedResetProtection());
    }
    private bool miniGameProtectionActive = false;
    private void SetMiniGameProtection(bool active)
    {
        miniGameProtectionActive = active;
    }
    public bool IsMiniGameProtected()
    {
        return miniGameProtectionActive;
    }
    private System.Collections.IEnumerator DelayedResetProtection()
    {
        yield return new WaitForSeconds(3f);
        SetMiniGameProtection(false);
    }
    private void ForceResetAllMiniGameStates()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            sceneController.ForceResetMiniGameState();
            if (sceneController.IsInMiniGame())
            {
                sceneController.ReturnControlToPlayer(true);
            }
        }
        if (GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.CurrentState == GameState.MiniGame)
            {
                GameStateManager.Instance.ResetToGameplay();
            }
        }
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            PetManualControl petControl = pet.GetComponent<PetManualControl>();
            if (petControl != null && petControl.enabled)
            {
                petControl.enabled = false;
            }
            PetShooting petShooting = pet.GetComponent<PetShooting>();
            if (petShooting != null && petShooting.enabled)
            {
                petShooting.enabled = false;
            }
            LyraHealth lyraHealth = pet.GetComponent<LyraHealth>();
            if (lyraHealth != null && lyraHealth.enabled)
            {
                lyraHealth.enabled = false;
                if (lyraHealth.healthBarUI != null)
                    lyraHealth.healthBarUI.gameObject.SetActive(false);
            }
        }
        if (InventoryUIHandler.Instance != null)
        {
            InventoryUIHandler.Instance.ToggleIconWhenPlayMiniGame(); // This should reset to normal
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Target = playerObj.transform;
        }
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
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

    private void HandleNormalRespawn(GameObject player, string currentScene)
    {
        if (lastCheckpointScene != currentScene)
        {
            LoadSceneWithCleanup(lastCheckpointScene, player);
        }
        else
        {
            ResetAllCombatZones();
            if (enableBossReset)
            {
                ResetBossesInCurrentScene();
            }
            ResetTriggersInCurrentScene();
            player.transform.position = lastCheckpointPosition;
            FinishRespawn(player);
        }
    }
    public void RestartMiniGame()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null && sceneController.IsInMiniGame())
        {
            sceneController.ReturnControlToPlayer(false);
        }
        ResetLyraForMiniGame();
        ResetMiniGameObstacles();
        CoreManager coreManager = FindFirstObjectByType<CoreManager>();
        if (coreManager != null)
        {
            coreManager.ResetCoreManagerKeepProgress(); // Method mới
        }
        ResetTriggersInCurrentScene();
        ResetAllCombatZones();
        StartCoroutine(DelayedRestartMiniGame(sceneController));
    }
    public void ResetMiniGameCompletion()
    {
        miniGameCompleted = false;
    }
    public void DebugMapBoss01TestStatus()
    {
        if (SceneManager.GetActiveScene().name == "MapBoss_01Test")
        {
            SceneController sceneController = FindFirstObjectByType<SceneController>();
            CoreManager coreManager = FindFirstObjectByType<CoreManager>();
            if (sceneController != null)
            {
                sceneController.DebugCurrentState();
                bool isInMiniGame = sceneController.IsInMiniGame();
                if (coreManager != null)
                {
                    coreManager.DebugProgress();
                }
            }
            DebugBossColliderStates();
        }
    }
    public void ManualForceResetGameStateToGameplay()
    {
        ForceResetGameStateToGameplay();
    }
    public void TestRespawnLogic()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RespawnPlayer(player);
        }
    }
    private void DebugBossColliderStates()
    {
        MiniBoss[] miniBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);
        foreach (var miniBoss in miniBosses)
        {
            if (miniBoss != null)
            {
                var colliders = miniBoss.GetComponents<Collider2D>();
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
                }
            }
        }
    }
   

    private CharacterController2D CurrentHealthProvider()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player ? player.GetComponent<CharacterController2D>() : null;
    }
    private System.Collections.IEnumerator DelayedRestartMiniGame(SceneController sceneController)
    {
        yield return null;
        if (sceneController != null)
        {
            sceneController.RestartMiniGame();
        }
    }
    private void ResetLyraForMiniGame()
    {
        LyraHealth lyraHealth = FindFirstObjectByType<LyraHealth>();
        if (lyraHealth != null)
        {
            lyraHealth.ResetLyra();
        }
        FloatingFollower pet = FindFirstObjectByType<FloatingFollower>();
        if (pet != null)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("spw");
            if (spawnPoint != null)
            {
                pet.transform.position = spawnPoint.transform.position;
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
    }
    public void ResetMiniGameCompletely()
    {
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
                    }
                }
                resetCount++;
            }
        }
        ResetTriggeredBosses();
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
            if (obj.CompareTag("Player")) continue; // ✅ đừng destroy player
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
        }
    }
    public void SetBossResetEnabled(bool enabled)
    {
        enableBossReset = enabled;
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
                catch 
                {
                }
            }
        }
    }
    
    private void HandleMapBoss02Respawn(GameObject player)
    {
        ResetAllCombatZones();
        
        ResetBoss2AndHands();
        
        ResetTriggersInCurrentScene();
        
        player.transform.position = lastCheckpointPosition;

        FinishRespawnWithoutPet(player);
    }

    private void ResetBoss2AndHands()
    {
        Boss2Controller boss2 = FindFirstObjectByType<Boss2Controller>();
        if (boss2 != null)
        {
            boss2.ResetBoss();
        }

        Boss2HandController[] hands = FindObjectsByType<Boss2HandController>(FindObjectsSortMode.None);
        foreach (var hand in hands)
        {
            if (hand != null)
            {
                hand.gameObject.SetActive(true);
                hand.ResetHand();
            }
        }

        HandDamageReceiver[] handDamageReceivers = FindObjectsByType<HandDamageReceiver>(FindObjectsSortMode.None);
        foreach (var handDR in handDamageReceivers)
        {
            if (handDR != null)
            {
                handDR.ResetHandHealth();
            }
        }
    }

    private void FinishRespawnWithoutPet(GameObject player)
    {
        CharacterController2D controller = player.GetComponent<CharacterController2D>();
        if (controller != null)
        {
            controller.RestoreFullLife();
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
    }
}