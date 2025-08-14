using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightToggle : MonoBehaviour
{
    [Header("Light")]
    public Light2D light2D;

    [Tooltip("Độ sáng gốc khi đèn bình thường")]
    public float baseIntensity = 1.0f;

    [Tooltip("Giới hạn dao động cường độ khi đang bật")]
    public float minIntensity = 0.6f;
    public float maxIntensity = 1.2f;

    [Header("Chập chờn (Perlin) khi bật")]
    [Tooltip("Biên độ chập chờn quanh baseIntensity")]
    public float noiseAmount = 0.15f;
    [Tooltip("Tốc độ thay đổi chập chờn")]
    public float noiseSpeed = 15f;

    [Header("Cụm nhấp nháy (burst)")]
    [Tooltip("Số nháy trong một cụm (ngẫu nhiên trong khoảng)")]
    public Vector2Int burstFlickerCount = new Vector2Int(3, 8);
    [Tooltip("Khoảng sáng mỗi nháy")]
    public Vector2 burstOnTime = new Vector2(0.02f, 0.12f);
    [Tooltip("Khoảng tắt mỗi nháy")]
    public Vector2 burstOffTime = new Vector2(0.05f, 0.25f);

    [Header("Blackout (tắt hẳn vài giây)")]
    [Range(0f, 1f)]
    public float blackoutChance = 0.25f;
    public Vector2 blackoutDuration = new Vector2(0.6f, 2.2f);

    [Header("Khoảng nghỉ giữa các cụm")]
    public Vector2 idleBetweenBursts = new Vector2(0.8f, 3.0f);

    [Header("Khởi động")]
    public bool playOnStart = true;

    float _seed;
    float _savedBaseIntensity;
    Coroutine _runner;

    void Awake()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
        if (light2D == null)
        {
            Debug.LogError("[BrokenLightFlicker] Không tìm thấy Light2D.");
            enabled = false;
            return;
        }

        _savedBaseIntensity = baseIntensity <= 0f ? light2D.intensity : baseIntensity;
        if (_savedBaseIntensity <= 0f) _savedBaseIntensity = 1f;

        // hạt giống noise ngẫu nhiên để mỗi đèn nháy khác nhau
        _seed = Random.value * 1000f;
    }

    void OnEnable()
    {
        if (playOnStart && _runner == null)
            _runner = StartCoroutine(RunBrokenFlicker());
    }

    void OnDisable()
    {
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
        // đảm bảo trả đèn về trạng thái sáng bình thường khi disable script
        if (light2D != null)
        {
            light2D.enabled = true;
            light2D.intensity = _savedBaseIntensity;
        }
    }

    void Update()
    {
        // Hiệu ứng chập chờn nhẹ khi đèn đang bật
        if (light2D != null && light2D.enabled)
        {
            float t = Time.time * noiseSpeed + _seed;
            float noise = Mathf.PerlinNoise(t, _seed) * 2f - 1f; // [-1..1]
            float flicker = _savedBaseIntensity + noise * noiseAmount;
            light2D.intensity = Mathf.Clamp(flicker, minIntensity, maxIntensity);
        }
    }

    IEnumerator RunBrokenFlicker()
    {
        // đảm bảo bật đèn lúc đầu
        light2D.enabled = true;
        light2D.intensity = _savedBaseIntensity;

        var wait = new WaitForEndOfFrame();

        while (true)
        {
            // 1) Nhấp nháy theo cụm
            int count = Random.Range(burstFlickerCount.x, burstFlickerCount.y + 1);
            for (int i = 0; i < count; i++)
            {
                // tắt nhanh
                light2D.enabled = false;
                yield return new WaitForSeconds(Random.Range(burstOffTime.x, burstOffTime.y));

                // bật nhanh
                light2D.enabled = true;
                // đẩy intensity lên chút để cảm giác “lóe sáng”
                light2D.intensity = Mathf.Clamp(_savedBaseIntensity * Random.Range(1.05f, 1.2f), minIntensity, maxIntensity);
                yield return new WaitForSeconds(Random.Range(burstOnTime.x, burstOnTime.y));

                // nhả cho Update lo “chập chờn” khung hình
                yield return wait;
            }

            // 2) Ngẫu nhiên blackout
            if (Random.value < blackoutChance)
            {
                light2D.enabled = false;
                yield return new WaitForSeconds(Random.Range(blackoutDuration.x, blackoutDuration.y));
                light2D.enabled = true;
                light2D.intensity = _savedBaseIntensity;
            }

            // 3) Nghỉ trước cụm tiếp theo
            yield return new WaitForSeconds(Random.Range(idleBetweenBursts.x, idleBetweenBursts.y));
        }
    }
}
