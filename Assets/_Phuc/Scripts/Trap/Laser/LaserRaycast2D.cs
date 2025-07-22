using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserRaycast2D : MonoBehaviour
{
    public Transform laserStartPoint;
    public float laserLength = 20f;
    public LayerMask obstacleLayers; // Lớp vật thể sẽ chặn tia laser (ví dụ Ground, Enemy,...)

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector2 start = laserStartPoint.position;
        Vector2 direction = -laserStartPoint.up; // Hoặc up/down tùy hướng của bạn

        RaycastHit2D hit = Physics2D.Raycast(start, direction, laserLength, obstacleLayers);

        Vector2 endPoint = hit.collider != null ? hit.point : start + direction * laserLength;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, endPoint);
    }
}
