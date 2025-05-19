using UnityEngine;

public class AlarmManager : MonoBehaviour
{
    public AudioSource alarmSound;
    public float stopDelay = 2f;

    private int detectorCount = 0;
    private float stopTimer = 0f;
    private bool isStopping = false;

    private void Update()
    {
        if (isStopping)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDelay)
            {
                alarmSound.Stop();
                isStopping = false;
                stopTimer = 0f;
            }
        }
    }

    public void StartAlarm()
    {
        if (!alarmSound.isPlaying)
            alarmSound.Play();

        isStopping = false;
        stopTimer = 0f;
        detectorCount++;
    }

    public void StopAlarm()
    {
        detectorCount--;

        if (detectorCount <= 0)
        {
            detectorCount = 0;
            isStopping = true;
        }
    }
}
