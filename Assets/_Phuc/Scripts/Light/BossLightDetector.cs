using UnityEngine;
using System.Collections.Generic;

public class LightDetectorBoss : MonoBehaviour
{
    private AlarmManager alarmManager;
    private HashSet<GameObject> detectedObjects = new HashSet<GameObject>();

    [Header("Máy bắn đạn sẽ được kích hoạt khi thấy NPC")]
    public TurretShooter turretShooter; // Gán từ Inspector

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

                if (turretShooter != null)
                {
                    turretShooter.Activate(other.transform);
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
