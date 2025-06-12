using UnityEngine;
using System.Collections.Generic;

public class LightDetector : MonoBehaviour
{
    private AlarmManager alarmManager;
    private HashSet<GameObject> detectedObjects = new HashSet<GameObject>();

    [Header("Enemy Mini sẽ bay vào nổ")]
    public List<EnemyMini> enemyMinis; // Gán trong Inspector

    [Header("Enemy Shooter sẽ bắn đạn")]
    public List<EnemyShooter> enemyShooters; // Gán trong Inspector

    private void Start()
    {
        alarmManager = FindFirstObjectByType<AlarmManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!detectedObjects.Contains(other.gameObject))
            {
                PlayerMovement player = other.GetComponent<PlayerMovement>();
                if (player != null && !player.IsInvisible())
                {
                    detectedObjects.Add(other.gameObject);
                    alarmManager.StartAlarm();

                    // Gọi tất cả EnemyMini
                    foreach (var enemy in enemyMinis)
                    {
                        if (enemy != null)
                            enemy.Activate(other.transform);
                    }

                    // Gọi tất cả EnemyShooter
                    foreach (var shooter in enemyShooters)
                    {
                        if (shooter != null)
                            shooter.Activate(other.transform);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (detectedObjects.Contains(other.gameObject))
            {
                detectedObjects.Remove(other.gameObject);
                alarmManager.StopAlarm();
            }
        }
    }
}
