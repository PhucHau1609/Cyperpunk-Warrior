using UnityEngine;
using System.Collections;

public class Boss2Laser : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SweepLaser());
    }

    private IEnumerator SweepLaser()
    {
        float startAngle = 300f;      // Góc khoảng 10 giờ (dưới trái)
        float endAngle = 60f;         // Góc khoảng 2 giờ (dưới phải)
        float sweepDuration = 2f;

        float elapsedTime = 0f;
        while (elapsedTime < sweepDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sweepDuration;
            float currentAngle = Mathf.LerpAngle(startAngle, endAngle, progress);

            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            yield return null;
        }

        Destroy(gameObject, 0.5f);
    }
}
