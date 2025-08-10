using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AlarmManager : MonoBehaviour
{
    public AudioSource alarmSound;
    public Light2D globalLight;

    [Header("Light Settings")]
    public float stopDelay = 2f;
    public float normalIntensity = 0.4f;
    public float alertIntensity = 0.4f;
    public float colorChangeSpeed = 0.5f; // tốc độ đổi màu chậm/mượt

    private int detectorCount = 0;
    private float stopTimer = 0f;
    private bool isStopping = false;

    private bool isBlinking = false;
    private float colorTimer = 0f;

    private Color normalColor = Color.white; // FFFFFF
    private Color alertColor = new Color32(0xF8, 0x00, 0x00, 0xFF); // F80000

    private void Start()
    {
        if (globalLight != null)
        {
            normalColor = globalLight.color;
        }
    }

    private void Update()
    {
        if (isBlinking && globalLight != null)
        {
            colorTimer += Time.deltaTime * colorChangeSpeed;

            // t dao động 0 -> 1 -> 0 mượt mà
            float t = (Mathf.Sin(colorTimer) + 1f) / 2f;

            // đổi màu liên tục giữa trắng và đỏ
            globalLight.color = Color.Lerp(normalColor, alertColor, t);

            // giữ intensity cố định
            globalLight.intensity = normalIntensity;
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
        colorTimer = 0f;
    }

    private void StopBlinking()
    {
        isBlinking = false;

        if (globalLight != null)
        {
            globalLight.intensity = normalIntensity;
            globalLight.color = normalColor; // trả về trắng
        }
    }
}
