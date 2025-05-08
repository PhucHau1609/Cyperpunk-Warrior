using UnityEngine;
using UnityEngine.Rendering.Universal; // Quan trọng để truy cập Light2D

public class FlickeringLight : MonoBehaviour
{
    public Light2D light2D; // Gán trong Inspector hoặc tự tìm
    public float minDelay = 0.05f;
    public float maxDelay = 0.3f;

    public float minIntensity = 0.3f;
    public float maxIntensity = 1.0f;

    private void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        StartCoroutine(Flicker());
    }

    private System.Collections.IEnumerator Flicker()
    {
        while (true)
        {
            // Bật/tắt đèn ngẫu nhiên
            light2D.intensity = Random.Range(minIntensity, maxIntensity);

            // Đợi trong khoảng thời gian ngẫu nhiên
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Có thể tắt hẳn đôi lúc để tạo hiệu ứng "tắt hẳn"
            if (Random.value > 0.7f)
            {
                light2D.intensity = 0f;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }
}
