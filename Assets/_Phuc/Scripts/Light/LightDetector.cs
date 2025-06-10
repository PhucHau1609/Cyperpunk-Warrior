using UnityEngine;
using System.Collections.Generic;

public class LightDetector : MonoBehaviour
{
    private AlarmManager alarmManager;
    private HashSet<GameObject> detectedObjects = new HashSet<GameObject>();
    public List<EnemyMini> enemyMinis; // Gán qua Inspector
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
                    foreach (var enemy in enemyMinis)
                    {
                        enemy.Activate(other.transform);
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
