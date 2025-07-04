using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Tọa độ di chuyển")]
    public Vector3 positionA;  // Tọa độ điểm A
    public Vector3 positionB;  // Tọa độ điểm B

    [Header("Tốc độ")]
    public float speed = 2f;

    private Vector3 target;

    void Start()
    {
        // Platform bắt đầu đi tới B
        target = positionB;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // Đảo hướng
            target = (target == positionA) ? positionB : positionA;
        }
    }

    // Đảm bảo player đi theo platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
