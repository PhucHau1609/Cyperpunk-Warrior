using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDestroyTracker : MonoBehaviour
{
    [Header("Enemy Tracking")]
    [SerializeField] private List<GameObject> targetEnemies = new List<GameObject>();
    [SerializeField] private bool trackByTag = false;
    [SerializeField] private string enemyTag = "Enemy";
    
    [Header("Completion Settings")]
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
        
        // Kích hoạt Events
        OnAllEnemiesDestroyed?.Invoke();
        
        // Kích hoạt script được chỉ định
        ActivateTargetScript();
        
        // Tắt script này nếu được cấu hình
        if (disableScriptAfterComplete)
        {
            enabled = false;
        }
    }
    
    void ActivateTargetScript()
    {
        if (scriptToActivate == null) return;

        // Enable script nếu nó bị disable
        if (!scriptToActivate.enabled)
        {
            scriptToActivate.enabled = true;
            
        }
        
        // Gọi method cụ thể nếu được chỉ định
        if (!string.IsNullOrEmpty(methodToCall))
        {
            try
            {
                scriptToActivate.Invoke(methodToCall, 0f);
            }
            catch (System.Exception e)
            {
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
        }
    }
    
    public void RemoveEnemyFromTracking(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Remove(enemy);
            totalEnemyCount--;
        }
    }
    
    public void ResetTracker()
    {
        isComplete = false;
        enabled = true;
        InitializeEnemyTracking();
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