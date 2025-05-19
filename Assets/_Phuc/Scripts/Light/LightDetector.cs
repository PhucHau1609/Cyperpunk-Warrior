using UnityEngine;

public class LightDetector : MonoBehaviour
{
    private AlarmManager alarmManager;

    private void Start()
    {
        alarmManager = FindObjectOfType<AlarmManager>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && !player.IsInvisible())
            {
                alarmManager.StartAlarm();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            alarmManager.StopAlarm();
        }
    }
}
