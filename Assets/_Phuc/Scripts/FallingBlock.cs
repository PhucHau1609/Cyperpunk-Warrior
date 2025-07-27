using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = false;

    public float delayBeforeFall = 0.2f;
    public float rotateDuration = 0.5f;
    public float targetRotationZ = 90f;
    public GameObject explosionEffect;

    private float rotateTimer = 0f;
    private bool startRotate = false;
    private float startZ;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        if (startRotate)
        {
            rotateTimer += Time.deltaTime;
            float t = Mathf.Clamp01(rotateTimer / rotateDuration);
            float currentZ = Mathf.Lerp(startZ, targetRotationZ, t);
            transform.rotation = Quaternion.Euler(0f, 0f, currentZ);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && collision.collider.CompareTag("Player"))
        {
            StartCoroutine(StartFalling());
        }

        if (isFalling && collision.collider.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private System.Collections.IEnumerator StartFalling()
    {
        isFalling = true;
        yield return new WaitForSeconds(delayBeforeFall);

        // Bắt đầu rơi và xoay cùng lúc
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Bắt đầu xoay
        startRotate = true;
        startZ = transform.eulerAngles.z;
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
