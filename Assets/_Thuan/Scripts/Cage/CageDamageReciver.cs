using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CageDamageReceiver : DamageReceiver, IDamageResponder
{
    private IDamageResponder responder;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private List<GameObject> targetEnemies = new List<GameObject>();
    
    // Tracking enemies
    private HashSet<GameObject> aliveEnemies = new HashSet<GameObject>();
    private bool isTrackingEnemies = false;

    protected override void ResetValue()
    {
        base.ResetValue();
        this.maxHP = 50;
        this.currentHP = 50;
    }

    protected virtual void AddHp()
    {
        this.currentHP += 20;
    }

    protected override void Awake()
    {
        base.Awake();
        responder = GetComponent<IDamageResponder>();

        IsImmotal = true;
        InitializeEnemyTracking();
    }

    private void Start()
    {
        StartEnemyTracking();
    }

    private void InitializeEnemyTracking()
    {
        aliveEnemies.Clear();
        
        foreach (GameObject enemy in targetEnemies)
        {
            if (enemy != null)
            {
                aliveEnemies.Add(enemy);
            }
        }
        
        if (aliveEnemies.Count > 0)
        {
            isTrackingEnemies = true;
        }
    }

    private void StartEnemyTracking()
    {
        if (isTrackingEnemies && aliveEnemies.Count > 0)
        {
            InvokeRepeating(nameof(CheckEnemiesStatus), 0.5f, 0.5f);
        }
        else if (aliveEnemies.Count == 0)
        {
            // Nếu không có enemies nào để theo dõi, tắt IsImmotal ngay
            IsImmotal = false;
        }
    }

    private void CheckEnemiesStatus()
    {
        if (!isTrackingEnemies) return;

        List<GameObject> enemiesToRemove = new List<GameObject>();
        
        foreach (GameObject enemy in aliveEnemies)
        {
            // Kiểm tra nếu enemy đã bị destroy hoặc không còn trong scene
            if (enemy == null || !enemy.scene.IsValid())
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        // Xóa những enemies đã bị destroy
        foreach (GameObject enemy in enemiesToRemove)
        {
            aliveEnemies.Remove(enemy);
        }
        
        // Kiểm tra nếu tất cả enemies đã bị tiêu diệt
        if (aliveEnemies.Count == 0)
        {
            OnAllEnemiesDestroyed();
        }
    }

    private void OnAllEnemiesDestroyed()
    {
        isTrackingEnemies = false;
        CancelInvoke(nameof(CheckEnemiesStatus));
        
        // Tắt IsImmotal
        IsImmotal = false;
    }

    // Public methods để quản lý danh sách enemies
    public void AddEnemyToTrack(GameObject enemy)
    {
        if (enemy != null && !aliveEnemies.Contains(enemy))
        {
            aliveEnemies.Add(enemy);
            targetEnemies.Add(enemy);
            
            if (!isTrackingEnemies)
            {
                isTrackingEnemies = true;
                IsImmotal = true;
                StartEnemyTracking();
            }
        }
    }

    public void RemoveEnemyFromTrack(GameObject enemy)
    {
        if (enemy != null)
        {
            aliveEnemies.Remove(enemy);
            targetEnemies.Remove(enemy);
        }
    }

    public void ResetEnemyTracking()
    {
        CancelInvoke(nameof(CheckEnemiesStatus));
        InitializeEnemyTracking();
        StartEnemyTracking();
    }

    protected override void OnHurt()
    {
        responder?.OnHurt();
    }

    protected override void OnDead()
    {
        responder?.OnDead();
        Destroy(gameObject);
        playableDirector.Play();
    }

    void IDamageResponder.OnHurt()
    {
        //
    }

    void IDamageResponder.OnDead()
    {
       //
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(CheckEnemiesStatus));
    }
}