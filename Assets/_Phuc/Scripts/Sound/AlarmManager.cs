using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AlarmManager : MonoBehaviour
{
    public AudioSource alarmSound;
    public Light2D globalLight;

    public float stopDelay = 2f;
    public float normalIntensity = 0.4f;
    public float alertIntensity = 0.1f;
    public float blinkSpeed = 4f;

    private int detectorCount = 0;
    private float stopTimer = 0f;
    private bool isStopping = false;

    private bool isBlinking = false;
    private float blinkTimer = 0f;

    private void Update()
    {
        if (isBlinking && globalLight != null)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            globalLight.intensity = Mathf.Lerp(alertIntensity, normalIntensity, Mathf.PingPong(blinkTimer, 1f));
        }

        if (isStopping)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= stopDelay)
            {
                if (alarmSound.isPlaying)
                    alarmSound.Stop();

                isStopping = false;
                stopTimer = 0f;

                StopBlinking();
            }
        }
    }

    public void StartAlarm()
    {
        if (detectorCount == 0)
        {
            if (!alarmSound.isPlaying)
                alarmSound.Play();

            StartBlinking();
        }

        detectorCount++;
        isStopping = false;
        stopTimer = 0f;
    }

    public void StopAlarm()
    {
        detectorCount--;

        if (detectorCount <= 0)
        {
            detectorCount = 0;
            isStopping = true;
            stopTimer = 0f;
        }
    }

    private void StartBlinking()
    {
        isBlinking = true;
        blinkTimer = 0f;
    }

    private void StopBlinking()
    {
        isBlinking = false;

        if (globalLight != null)
        {
            globalLight.intensity = normalIntensity;
        }
    }
}
