using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fallSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Bắt đầu rơi
        rb.gravityScale = 1f;
    }

    void Update()
    {
        // Không cần xử lý gì thêm trong Update
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            //rb.isKinematic = true;

            anim.SetTrigger("Explode");
        }
    }

    // Gọi từ Event trong animation nổ
    public void DestroyBomb()
    {
        Destroy(gameObject);
    }
}
