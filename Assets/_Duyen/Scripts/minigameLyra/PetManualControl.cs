using UnityEngine;

public class PetManualControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "mapdemo2")
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        rb.linearVelocity = new Vector2(h, v).normalized * moveSpeed;

        // Flip hướng bay nếu cần
        if (h != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = h > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
