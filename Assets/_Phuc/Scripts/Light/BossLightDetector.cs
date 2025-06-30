using UnityEngine;
using System.Collections.Generic;

public class LightDetectorBoss : MonoBehaviour
{
    private AlarmManager alarmManager;
    private HashSet<GameObject> detectedObjects = new HashSet<GameObject>();

    [Header("Enemy Mini sẽ bay vào nổ")]
    public List<EnemyMini> enemyMinis; // OK

    [Header("Enemy Shooter sẽ bắn Boss")]
    public List<EnemyShooter> enemyShooters; // ← CHẮC CHẮN PHẢI CÙNG KIỂU VỚI SCRIPT PREFAB
    // Hoặc nếu bạn đang dùng EnemyShooterBossTarget thì đổi thành:
    // public List<EnemyShooterBossTarget> enemyShooters;

    private void Start()
    {
        alarmManager = FindFirstObjectByType<AlarmManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            if (!detectedObjects.Contains(other.gameObject))
            {
                detectedObjects.Add(other.gameObject);
                alarmManager.StartAlarm();

                foreach (var enemy in enemyMinis)
                {
                    if (enemy != null)
                        enemy.Activate(other.transform);
                }

                foreach (var shooter in enemyShooters)
                {
                    if (shooter != null)
                        shooter.Activate(other.transform); // ⚠️ Phải khớp kiểu phương thức
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            if (detectedObjects.Contains(other.gameObject))
            {
                detectedObjects.Remove(other.gameObject);
                alarmManager.StopAlarm();
            }
        }
    }
}
