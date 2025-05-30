using UnityEngine;

public class CannonOrbit : MonoBehaviour
{
    public Transform centerPoint;     // Tháp (tâm xoay)
    public float radius = 2f;         // Bán kính xoay quanh tháp
    public float angularSpeed = 50f;  // Tốc độ quay (độ/giây)
    public float angle = 0f;          // Góc ban đầu (dành cho nhiều pháo)

    void Update()
    {
        // Xoay cùng chiều kim đồng hồ
        angle -= angularSpeed * Time.deltaTime;
        if (angle < 0f) angle += 360f;

        // Đổi độ sang radian để tính vị trí
        float rad = angle * Mathf.Deg2Rad;

        // Tính vị trí mới dựa theo góc và bán kính
        float x = centerPoint.position.x + Mathf.Cos(rad) * radius;
        float y = centerPoint.position.y + Mathf.Sin(rad) * radius;

        // Cập nhật vị trí khẩu pháo
        transform.position = new Vector3(x, y, 0f);

        // Tính hướng từ tâm tháp ra khẩu pháo
        Vector2 dir = (transform.position - centerPoint.position).normalized;

        // Tính góc quay từ vector hướng
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Quay pháo +90 độ để chỉnh sprite nằm ngang
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + 90f);
    }
}
