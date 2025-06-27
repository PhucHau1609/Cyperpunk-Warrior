using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Boss2Laser : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool isSweeping = false;

    private void Awake()
    {
        
    }

    private void Start()
    {
        StartCoroutine(SweepLaser());
    }

    private IEnumerator SweepLaser()
    {
        isSweeping = true;
        lineRenderer.enabled = true;

        float startAngle = 240f;        // 8 giờ
        float endAngle = 120f;          // 4 giờ
        float sweepDuration = 3f;       // Thời gian quét
        float laserLength = 15f;        // Độ dài laser

        float elapsedTime = 0f;
        while (elapsedTime < sweepDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sweepDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);

            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            UpdateLaserVisual(laserLength);

            yield return null;
        }

        isSweeping = false;
        lineRenderer.enabled = false;
        Destroy(gameObject, 0.5f);
    }

    private void UpdateLaserVisual(float laserLength)
    {
        Vector3 startPos = transform.position;
        Vector3 direction = transform.right;
        Vector3 endPos = startPos + direction * laserLength;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
