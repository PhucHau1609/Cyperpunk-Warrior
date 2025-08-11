using System;
using UnityEngine;

public class EnemyDeathManager : MonoBehaviour
{
    public static EnemyDeathManager Instance;
    public static event Action AllEnemiesDead;   // 🔔 Sự kiện

    private int totalEnemies;
    private int deadEnemies = 0;

    private void Awake() => Instance = this;

    void Start()
    {
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    public void OnEnemyDied()
    {
        deadEnemies++;
        if (deadEnemies >= totalEnemies)
        {
            AllEnemiesDead?.Invoke();            // 🔔 bắn sự kiện, KHÔNG mở cửa ở đây
        }
    }

    public bool AreAllEnemiesDead => deadEnemies >= totalEnemies; // tiện cho nơi khác cần check
}
