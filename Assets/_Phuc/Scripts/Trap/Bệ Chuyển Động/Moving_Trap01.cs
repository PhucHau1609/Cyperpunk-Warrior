using UnityEngine;

public class Moving_Trap01 : MonoBehaviour
{
    public float slideSpeed = 2f; // Tốc độ trượt

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Chỉ tác động theo chiều ngang
                rb.linearVelocity = new Vector2(slideSpeed, rb.linearVelocity.y);
            }
        }
    }
}
