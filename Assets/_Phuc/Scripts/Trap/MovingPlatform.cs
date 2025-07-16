using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Tọa độ di chuyển")]
    public Vector3 positionA;  // Tọa độ điểm A
    public Vector3 positionB;  // Tọa độ điểm B

    [Header("Tốc độ")]
    public float speed = 2f;

    [HideInInspector] public Vector3 platformVelocity;
    private Vector3 target;
    private Vector3 lastPosition;

    void Start()
    {
        target = positionB;
        lastPosition = transform.position;
    }

    void Update()
    {
        // Di chuyển platform
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Tính vận tốc platform
        platformVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Đổi hướng
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == positionA) ? positionB : positionA;
        }
    }
}
