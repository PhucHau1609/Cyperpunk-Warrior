using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Management")]
    [SerializeField] private List<GameObject> enemiesInScene = new List<GameObject>();
    [SerializeField] private int requiredKills = 0; // S? enemy c?n tiêu di?t (0 = t?t c?)
    [SerializeField] private int currentKills = 0;

    [Header("Scene Barriers")]
    [SerializeField] private List<SceneBarrier> barriers = new List<SceneBarrier>();

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    public static EnemyManager Instance { get; private set; }

    // Events
    public System.Action<int, int> OnKillCountChanged; // (current, required)
    public System.Action OnAllEnemiesKilled;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeEnemyList();
        UpdateBarriers();

        if (showDebugInfo)
        {
            Debug.Log($"[EnemyManager] Scene initialized with {enemiesInScene.Count} enemies. Required kills: {GetRequiredKills()}");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// T? ð?ng t?m t?t c? enemy trong scene
    /// </summary>
    [ContextMenu("Auto Find Enemies")]
    public void InitializeEnemyList()
    {
        enemiesInScene.Clear();

        // T?m t?t c? object có EnemyDamageReceiver
        EnemyDamageReceiver[] enemies = FindObjectsByType<EnemyDamageReceiver>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemiesInScene.Add(enemy.gameObject);

            // Subscribe to death event
            enemy.OnDeathEvent += OnEnemyKilled;
        }

        // N?u requiredKills = 0, set b?ng t?ng s? enemy
        if (requiredKills <= 0)
            requiredKills = enemiesInScene.Count;

        if (showDebugInfo)
        {
            Debug.Log($"[EnemyManager] Found {enemiesInScene.Count} enemies in scene");
        }
    }

    /// <summary>
    /// Thêm enemy th? công
    /// </summary>
    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemiesInScene.Contains(enemy))
        {
            enemiesInScene.Add(enemy);

            var damageReceiver = enemy.GetComponent<EnemyDamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.OnDeathEvent += OnEnemyKilled;
            }
        }
    }

    /// <summary>
    /// Xóa enemy kh?i danh sách
    /// </summary>
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemiesInScene.Contains(enemy))
        {
            enemiesInScene.Remove(enemy);

            var damageReceiver = enemy.GetComponent<EnemyDamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.OnDeathEvent -= OnEnemyKilled;
            }
        }
    }

    /// <summary>
    /// Ðý?c g?i khi enemy ch?t
    /// </summary>
    private void OnEnemyKilled(GameObject enemy)
    {
        currentKills++;

        if (showDebugInfo)
        {
            Debug.Log($"[EnemyManager] Enemy killed! Progress: {currentKills}/{GetRequiredKills()}");
        }

        OnKillCountChanged?.Invoke(currentKills, GetRequiredKills());

        // Ki?m tra xem ð? ð? kills chýa
        if (currentKills >= GetRequiredKills())
        {
            OnAllEnemiesKilled?.Invoke();
            UpdateBarriers();

            if (showDebugInfo)
            {
                Debug.Log("[EnemyManager] All required enemies killed! Barriers disabled.");
            }
        }
    }

    /// <summary>
    /// C?p nh?t tr?ng thái barriers
    /// </summary>
    private void UpdateBarriers()
    {
        bool shouldBlock = currentKills < GetRequiredKills();

        foreach (var barrier in barriers)
        {
            if (barrier != null)
            {
                if (shouldBlock)
                    barrier.EnableBarrier();
                else
                    barrier.DisableBarrier();
            }
        }
    }

    /// <summary>
    /// Thêm barrier vào danh sách qu?n l?
    /// </summary>
    public void RegisterBarrier(SceneBarrier barrier)
    {
        if (!barriers.Contains(barrier))
        {
            barriers.Add(barrier);
            UpdateBarriers();
        }
    }

    /// <summary>
    /// Xóa barrier kh?i danh sách
    /// </summary>
    public void UnregisterBarrier(SceneBarrier barrier)
    {
        barriers.Remove(barrier);
    }

    /// <summary>
    /// Reset ti?n tr?nh (ð? test)
    /// </summary>
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        currentKills = 0;
        UpdateBarriers();
        OnKillCountChanged?.Invoke(currentKills, GetRequiredKills());

        if (showDebugInfo)
        {
            Debug.Log("[EnemyManager] Progress reset!");
        }
    }

    /// <summary>
    /// Force complete (ð? test)
    /// </summary>
    [ContextMenu("Force Complete")]
    public void ForceComplete()
    {
        currentKills = GetRequiredKills();
        OnAllEnemiesKilled?.Invoke();
        UpdateBarriers();

        if (showDebugInfo)
        {
            Debug.Log("[EnemyManager] Force completed!");
        }
    }

    // Getters
    public int GetCurrentKills() => currentKills;
    public int GetRequiredKills() => requiredKills > 0 ? requiredKills : enemiesInScene.Count;
    public int GetRemainingKills() => Mathf.Max(0, GetRequiredKills() - currentKills);
    public bool IsCompleted() => currentKills >= GetRequiredKills();
    public float GetProgress() => GetRequiredKills() > 0 ? (float)currentKills / GetRequiredKills() : 0f;

    // Setters (cho Editor Tool)
    public void SetRequiredKills(int kills) => requiredKills = kills;
    public void SetEnemiesList(List<GameObject> enemies) => enemiesInScene = enemies;
    public void SetBarriersList(List<SceneBarrier> barriersList) => barriers = barriersList;
}