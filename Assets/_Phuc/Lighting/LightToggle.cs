using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightToggle : MonoBehaviour
{
    [Header("Light")]
    public Light2D light2D;
    [Tooltip("Độ sáng bình thường khi bật")]
    public float baseIntensity = 1f;

    [Header("Kiểu nháy (random mỗi đợt)")]
    [Range(0f, 1f)]
    [Tooltip("Xác suất chọn kiểu nháy đúng 2 lần (đèn bị hư)")]
    public float doubleBlinkChance = 0.5f;

    [Header("Khoảng nghỉ giữa các đợt nháy")]
    [Tooltip("Sau khi kết thúc 1 đợt nháy, chờ ngẫu nhiên trước khi nháy lại")]
    public Vector2 idleBetweenBursts = new Vector2(0.8f, 2.5f);

    [Header("Thông số chớp tắt")]
    [Tooltip("Thời gian TẮT mỗi nhịp")]
    public Vector2 offTime = new Vector2(0.05f, 0.20f);
    [Tooltip("Thời gian BẬT sau mỗi lần tắt")]
    public Vector2 onTime = new Vector2(0.05f, 0.15f);

    [Header("Nháy ngẫu nhiên (không phải 2 lần)")]
    [Tooltip("Số lần nháy trong một đợt khi KHÔNG chọn double-blink")]
    public Vector2Int randomBlinkCount = new Vector2Int(3, 6);

    [Header("Khởi động")]
    public bool playOnStart = true;

    float _savedBaseIntensity;
    Coroutine _runner;

    void Awake()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
        if (light2D == null)
        {
            Debug.LogError("[BrokenLightSimple] Không tìm thấy Light2D.");
            enabled = false;
            return;
        }
        _savedBaseIntensity = baseIntensity <= 0f ? light2D.intensity : baseIntensity;
        if (_savedBaseIntensity <= 0f) _savedBaseIntensity = 1f;
    }

    void OnEnable()
    {
        if (playOnStart && _runner == null)
            _runner = StartCoroutine(Run());
    }

    void OnDisable()
    {
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
        if (light2D != null)
        {
            light2D.enabled = true;
            light2D.intensity = _savedBaseIntensity;
        }
    }

    IEnumerator Run()
    {
        // đảm bảo bật đèn lúc đầu
        light2D.enabled = true;
        light2D.intensity = _savedBaseIntensity;

        while (true)
        {
            // nghỉ trước mỗi đợt nháy
            yield return new WaitForSeconds(Random.Range(idleBetweenBursts.x, idleBetweenBursts.y));

            // chọn kiểu: double blink hay nháy ngẫu nhiên
            bool useDoubleBlink = Random.value < doubleBlinkChance;

            if (useDoubleBlink)
            {
                // kiểu 2: chớp tắt đúng 2 lần
                yield return StartCoroutine(BlinkOnce());
                yield return StartCoroutine(BlinkOnce());
            }
            else
            {
                // kiểu 1: chớp tắt ngẫu nhiên vài cái
                int count = Random.Range(randomBlinkCount.x, randomBlinkCount.y + 1);
                for (int i = 0; i < count; i++)
                    yield return StartCoroutine(BlinkOnce());
            }
        }
    }

    IEnumerator BlinkOnce()
    {
        // TẮT
        light2D.enabled = false;
        yield return new WaitForSeconds(Random.Range(offTime.x, offTime.y));

        // BẬT (hơi lóe nhẹ)
        light2D.enabled = true;
        light2D.intensity = _savedBaseIntensity * Random.Range(1.0f, 1.15f);
        yield return new WaitForSeconds(Random.Range(onTime.x, onTime.y));

        // trả về cường độ bình thường
        light2D.intensity = _savedBaseIntensity;
    }
}
