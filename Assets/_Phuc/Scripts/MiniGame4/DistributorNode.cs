using UnityEngine;
using UnityEngine.UI; // Cần cho Image (nếu bạn muốn thay đổi màu Node)

public class DistributorNode : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 50f;
    public Transform centerPoint; // GameObject cha (RingAndNode_X) làm tâm xoay
    public float targetAngleDegrees = 0f; // Góc mục tiêu (0 độ là bên phải)
    public float successToleranceDegrees = 10f; // Độ dung sai để coi là thành công (ví dụ: +/- 10 độ)
    public float nearTargetToleranceDegrees = 20f; // Độ dung sai để coi là "gần" mục tiêu (cho bar chập chờn)

    [Header("References")]
    public Image correspondingBar; // Thanh bar tương ứng với node này

    private bool _isRotating = false;
    private bool _isActive = false;
    private CalibrateDistributorGame _gameManager; // Tham chiếu đến game manager

    void Update()
    {
        if (_isRotating && _isActive && centerPoint != null)
        {
            // Xoay node ngược chiều kim đồng hồ quanh centerPoint
            // Vector3.forward cho 2D UI (trục Z hướng ra màn hình)
            transform.RotateAround(centerPoint.position, Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Cập nhật thanh bar nếu node này đang active và xoay
        if (_isActive && correspondingBar != null)
        {
            UpdateBarProximity();
        }
    }

    public void Initialize(CalibrateDistributorGame manager)
    {
        _gameManager = manager;
        // Đặt vị trí ban đầu nếu cần, ví dụ ở góc 90 độ (phía trên)
        if (centerPoint != null)
        {
            transform.position = centerPoint.position + Quaternion.Euler(0, 0, 90) * Vector3.right * Vector3.Distance(transform.position, centerPoint.position);
        }
    }

    public void Activate(float speed)
    {
        rotationSpeed = speed;
        _isActive = true;
        _isRotating = true;
        if (correspondingBar != null) correspondingBar.fillAmount = 0.01f; // Reset bar
    }

    public void Deactivate()
    {
        _isActive = false;
        _isRotating = false;
    }

    public bool IsActive()
    {
        return _isActive;
    }

    // Được gọi bởi nút bấm chính thông qua GameManager
    public bool TryStopNode()
    {
        if (!_isActive) return false;

        _isRotating = false;
        float currentAngle = GetCurrentAngleDegrees();
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngleDegrees); // Tính sự khác biệt góc nhỏ nhất

        Debug.Log($"Node {gameObject.name}: Stopped at {currentAngle} deg. Target: {targetAngleDegrees} deg. Diff: {angleDifference} deg");

        if (Mathf.Abs(angleDifference) <= successToleranceDegrees)
        {
            SnapToTarget();
            if (correspondingBar != null) correspondingBar.fillAmount = 1f; // 100%
            return true;
        }
        else
        {
            // Không thành công, có thể reset lại thanh bar nếu muốn hoặc để nguyên
            if (correspondingBar != null) correspondingBar.fillAmount = 0.01f;
            // Quyết định: cho xoay lại ngay hay chờ GameManager xử lý
            // _isRotating = true; // Nếu muốn tự động xoay lại
            return false;
        }
    }

    void SnapToTarget()
    {
        if (centerPoint == null) return;
        // Tính toán vị trí chính xác tại targetAngleDegrees
        float radius = Vector3.Distance(transform.position, centerPoint.position);
        Vector3 targetDirection = Quaternion.Euler(0, 0, targetAngleDegrees) * Vector3.right;
        transform.position = centerPoint.position + targetDirection * radius;
    }

    float GetCurrentAngleDegrees()
    {
        if (centerPoint == null) return 0f;
        Vector3 direction = (transform.position - centerPoint.position).normalized;
        // Góc được tính từ Vector3.right (0 độ)
        // Atan2 trả về radians, sau đó chuyển sang degrees.
        // Trục Y trước, X sau cho Atan2 khi dùng với góc trong mặt phẳng XY (2D).
        // Dấu trừ cho Y để góc tăng theo chiều kim đồng hồ nếu Y dương là lên trên.
        // Hoặc đơn giản là Vector3.SignedAngle
        float angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
        return angle;
    }

    void UpdateBarProximity()
    {
        if (!_isRotating) return; // Không cập nhật bar nếu đã dừng

        float currentAngle = GetCurrentAngleDegrees();
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngleDegrees);

        if (Mathf.Abs(angleDifference) <= nearTargetToleranceDegrees)
        {
            // "Chập chờn" 99-100% - có thể dùng Mathf.PingPong hoặc một giá trị gần 1
            correspondingBar.fillAmount = Mathf.Lerp(0.90f, 1f, (nearTargetToleranceDegrees - Mathf.Abs(angleDifference)) / nearTargetToleranceDegrees);
        }
        else
        {
            correspondingBar.fillAmount = 0.02f; // 1-2%
        }
    }
}