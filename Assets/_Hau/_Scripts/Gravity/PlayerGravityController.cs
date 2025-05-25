// PlayerGravityController.cs
using UnityEngine;

public class PlayerGravityController : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGravityInverted = false;
    private float originalGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    public void InvertGravity()
    {
        isGravityInverted = !isGravityInverted;
        rb.gravityScale = isGravityInverted ? -originalGravityScale : originalGravityScale;

        // Lật sprite để nhân vật ngược đầu
        Vector3 localScale = transform.localScale;
        localScale.y *= -1;
        transform.localScale = localScale;
    }
}
