using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EnemyDestroyTracker : MonoBehaviour
{
    [Header("Enemy Tracking")]
    [SerializeField] private List<GameObject> targetEnemies = new List<GameObject>();
    [SerializeField] private bool trackByTag = false;
    [SerializeField] private string enemyTag = "Enemy";
    
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private TimelineAsset timelineAsset;
    [SerializeField] private bool playTimelineOnComplete = true;
    
    [Header("GameObject Control")]
    [SerializeField] private List<GameObject> gameObjectsToDeactivate = new List<GameObject>();
    [SerializeField] private List<GameObject> gameObjectsToActivate = new List<GameObject>();
    
    [Header("Legacy Script Support (Optional)")]
    [SerializeField] private MonoBehaviour scriptToActivate;
    [SerializeField] private string methodToCall = "";
    [SerializeField] private bool disableScriptAfterComplete = true;
    
    [Header("Events")]
    public UnityEvent OnAllEnemiesDestroyed;
    public UnityEvent<int> OnEnemyDestroyed; // Truyền số enemy còn lại
    
    private HashSet<GameObject> aliveEnemies = new HashSet<GameObject>();
    private int totalEnemyCount;
    private bool isComplete = false;
    
    void Start()
    {
        InitializeEnemyTracking();
        
        // Auto-find PlayableDirector if not assigned
        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }
    }
    
    void InitializeEnemyTracking()
    {
        aliveEnemies.Clear();
        
        if (trackByTag)
        {
            // Tìm tất cả enemies theo tag
            GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemy in foundEnemies)
            {
                if (enemy != null)
                {
                    aliveEnemies.Add(enemy);
                    RegisterEnemyForDestruction(enemy);
                }
            }
        }
        else
        {
            // Sử dụng danh sách enemies được chỉ định
            foreach (GameObject enemy in targetEnemies)
            {
                if (enemy != null)
                {
                    aliveEnemies.Add(enemy);
                    RegisterEnemyForDestruction(enemy);
                }
            }
        }
        
        totalEnemyCount = aliveEnemies.Count;
        Debug.Log($"[EnemyDestroyTracker] Tracking {totalEnemyCount} enemies");
    }
    
    void RegisterEnemyForDestruction(GameObject enemy)
    {
        // Thêm component DestroyNotifier nếu chưa có
        DestroyNotifier notifier = enemy.GetComponent<DestroyNotifier>();
        if (notifier == null)
        {
            notifier = enemy.AddComponent<DestroyNotifier>();
        }
        
        // Đăng ký sự kiện khi enemy bị destroy
        notifier.OnDestroyed.AddListener(() => OnEnemyDestroyed_Internal(enemy));
    }
    
    void OnEnemyDestroyed_Internal(GameObject destroyedEnemy)
    {
        if (isComplete) return;
        
        if (aliveEnemies.Contains(destroyedEnemy))
        {
            aliveEnemies.Remove(destroyedEnemy);
            int remainingCount = aliveEnemies.Count;
            
            Debug.Log($"[EnemyDestroyTracker] Enemy destroyed! Remaining: {remainingCount}");
            
            // Kích hoạt event khi có enemy bị destroy
            OnEnemyDestroyed?.Invoke(remainingCount);
            
            // Kiểm tra nếu tất cả enemies đã bị destroy
            if (remainingCount == 0 && totalEnemyCount > 0)
            {
                CompleteObjective();
            }
        }
    }
    
    void CompleteObjective()
    {
        if (isComplete) return;
        
        isComplete = true;
        Debug.Log("[EnemyDestroyTracker] All enemies destroyed! Completing objective...");
        
        // Kích hoạt Events
        OnAllEnemiesDestroyed?.Invoke();
        
        // Kích hoạt Timeline
        PlayTimeline();
        
        // Quản lý GameObjects
        ManageGameObjects();
        
        // Legacy: Kích hoạt script được chỉ định (nếu có)
        ActivateTargetScript();
        
        // Tắt script này nếu được cấu hình
        if (disableScriptAfterComplete)
        {
            enabled = false;
        }
    }
    
    void PlayTimeline()
    {
        if (!playTimelineOnComplete) return;
        
        if (timelineDirector != null)
        {
            // Set timeline asset nếu có
            if (timelineAsset != null)
            {
                timelineDirector.playableAsset = timelineAsset;
            }
            
            // Play timeline
            timelineDirector.Play();
            Debug.Log("[EnemyDestroyTracker] Timeline started playing");
        }
        else
        {
            Debug.LogWarning("[EnemyDestroyTracker] No PlayableDirector assigned, cannot play timeline!");
        }
    }
    
    void ManageGameObjects()
    {
        // Tắt các GameObjects được chỉ định
        foreach (GameObject obj in gameObjectsToDeactivate)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                obj.SetActive(false);
                Debug.Log($"[EnemyDestroyTracker] Deactivated GameObject: {obj.name}");
            }
        }
        
        // Bật các GameObjects được chỉ định
        foreach (GameObject obj in gameObjectsToActivate)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.SetActive(true);
                Debug.Log($"[EnemyDestroyTracker] Activated GameObject: {obj.name}");
            }
        }
    }
    
    void ActivateTargetScript()
    {
        if (scriptToActivate == null) return;

        // Enable script nếu nó bị disable
        if (!scriptToActivate.enabled)
        {
            scriptToActivate.enabled = true;
            Debug.Log($"[EnemyDestroyTracker] Enabled script: {scriptToActivate.GetType().Name}");
        }
        
        // Gọi method cụ thể nếu được chỉ định
        if (!string.IsNullOrEmpty(methodToCall))
        {
            try
            {
                scriptToActivate.Invoke(methodToCall, 0f);
                Debug.Log($"[EnemyDestroyTracker] Called method: {methodToCall}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemyDestroyTracker] Failed to call method {methodToCall}: {e.Message}");
            }
        }
    }
    
    // Public methods để sử dụng từ bên ngoài
    public void AddEnemy(GameObject enemy)
    {
        if (enemy != null && !aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Add(enemy);
            RegisterEnemyForDestruction(enemy);
            totalEnemyCount++;
            Debug.Log($"[EnemyDestroyTracker] Added enemy: {enemy.name}. Total: {totalEnemyCount}");
        }
    }
    
    public void RemoveEnemyFromTracking(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
            totalEnemyCount--;
            Debug.Log($"[EnemyDestroyTracker] Removed enemy from tracking: {enemy.name}");
        }
    }
    
    public void ResetTracker()
    {
        isComplete = false;
        enabled = true;
        InitializeEnemyTracking();
        Debug.Log("[EnemyDestroyTracker] Tracker reset");
    }
    
    public void ForceCompleteObjective()
    {
        Debug.Log("[EnemyDestroyTracker] Force completing objective...");
        CompleteObjective();
    }
    
    public int GetRemainingEnemyCount()
    {
        return aliveEnemies.Count;
    }
    
    public int GetTotalEnemyCount()
    {
        return totalEnemyCount;
    }
    
    public bool IsComplete()
    {
        return isComplete;
    }
    
    // Method để enemies tự báo cáo khi sắp bị destroy
    public void NotifyEnemyWillBeDestroyed(GameObject enemy)
    {
        OnEnemyDestroyed_Internal(enemy);
    }
    
    // Methods to control GameObjects from external scripts
    public void AddGameObjectToDeactivate(GameObject obj)
    {
        if (obj != null && !gameObjectsToDeactivate.Contains(obj))
        {
            gameObjectsToDeactivate.Add(obj);
        }
    }
    
    public void AddGameObjectToActivate(GameObject obj)
    {
        if (obj != null && !gameObjectsToActivate.Contains(obj))
        {
            gameObjectsToActivate.Add(obj);
        }
    }
    
    public void RemoveGameObjectFromDeactivate(GameObject obj)
    {
        if (gameObjectsToDeactivate.Contains(obj))
        {
            gameObjectsToDeactivate.Remove(obj);
        }
    }
    
    public void RemoveGameObjectFromActivate(GameObject obj)
    {
        if (gameObjectsToActivate.Contains(obj))
        {
            gameObjectsToActivate.Remove(obj);
        }
    }
    
    // Method to set timeline at runtime
    public void SetTimeline(TimelineAsset timeline)
    {
        timelineAsset = timeline;
        if (timelineDirector != null)
        {
            timelineDirector.playableAsset = timeline;
        }
    }
    
    public void SetTimelineDirector(PlayableDirector director)
    {
        timelineDirector = director;
    }
}

// Component helper để theo dõi khi GameObject bị destroy
public class DestroyNotifier : MonoBehaviour
{
    [System.NonSerialized]
    public UnityEvent OnDestroyed = new UnityEvent();
    
    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}