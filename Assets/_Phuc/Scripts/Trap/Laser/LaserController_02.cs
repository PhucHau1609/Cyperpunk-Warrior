using System.Collections;
using UnityEngine;

public class LaserController_02 : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private float toggleInterval = 1f;

    // Thêm các biến mới
    [SerializeField] private float maxLaserDistance = 20f; // Độ dài tối đa của laser
    [SerializeField] private LayerMask blockingLayers = -1; // Layer của các vật thể có thể chắn laser
    [SerializeField] private Transform laserStartPoint; // Điểm bắt đầu bắn laser
    [SerializeField] private Transform laserDirection; // Hướng bắn laser (có thể là transform con)

    private LineRenderer lineRenderer;
    private BoxCollider2D laserCollider;

    private void Start()
    {
        // Lấy component LineRenderer và BoxCollider2D từ child object
        lineRenderer = GetComponentInChildren<LineRenderer>();
        laserCollider = GetComponentInChildren<BoxCollider2D>();

        StartCoroutine(ToggleRoutine());
    }

    private IEnumerator ToggleRoutine()
    {
        while (true)
        {
            if (targetObject != null)
            {
                bool shouldActivate = !targetObject.activeSelf;
                targetObject.SetActive(shouldActivate);

                // Cập nhật laser khi bật
                if (shouldActivate)
                {
                    UpdateLaserLength();
                }
            }

            yield return new WaitForSeconds(toggleInterval);
        }
    }

    private void UpdateLaserLength()
    {
        if (laserStartPoint == null || lineRenderer == null) return;
        Vector2 startPos = laserStartPoint.position;
        Vector2 direction = laserDirection != null ? laserDirection.up * -1 : Vector2.down;

        // Hiển thị tia raycast trong Scene view
        Debug.DrawRay(startPos, direction * maxLaserDistance, Color.red, 0.1f);

        // Raycast để tìm vật thể chắn
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxLaserDistance, blockingLayers);
        float laserLength;
        Vector2 endPos;
        if (hit.collider != null)
        {
            // Có vật thể chắn - rút ngắn laser
            Debug.Log(hit.collider.name);
            laserLength = hit.distance;
            endPos = hit.point;

            // Hiển thị điểm va chạm
            Debug.DrawLine(startPos, hit.point, Color.green, 1f);
            Debug.DrawRay(hit.point, Vector3.forward, Color.yellow, 1f); // Điểm va chạm
        }
        else
        {
            // Không có vật thể chắn - laser dài tối đa
            laserLength = maxLaserDistance;
            endPos = startPos + direction * maxLaserDistance;

            // Hiển thị tia raycast đầy đủ không bị chặn
            Debug.DrawLine(startPos, endPos, Color.blue, 0.1f);
        }
        // Cập nhật LineRenderer
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        // Cập nhật BoxCollider2D (damage area)
        UpdateLaserCollider(startPos, endPos, laserLength);
    }

    private void UpdateLaserCollider(Vector2 startPos, Vector2 endPos, float length)
    {
        if (laserCollider == null) return;

        // Chỉ cập nhật kích thước, không di chuyển GameObject
        laserCollider.size = new Vector2(0.5f, length);

        // Điều chỉnh offset để collider bao phủ đúng vùng laser
        // Giả sử laser bắt đầu từ vị trí GameObject Line
        laserCollider.offset = new Vector2(0, -(length / 2f));
    }

    // Gọi hàm này khi cần cập nhật laser (ví dụ khi có vật thể di chuyển)
    private void Update()
    {
        if (targetObject != null && targetObject.activeSelf)
        {
            UpdateLaserLength();
        }
    }
}
