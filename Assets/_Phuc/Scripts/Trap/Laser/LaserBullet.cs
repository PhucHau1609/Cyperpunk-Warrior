using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Cho đạn bay xuống dưới
        rb.velocity = Vector2.down * speed;

        // Quay đầu đạn xuống nếu sprite đang nằm ngang
        transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Bullet hit: " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Ground") || other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
