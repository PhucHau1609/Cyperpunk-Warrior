using UnityEngine;
using System.Collections;

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

    // ✅ Lưu trạng thái ban đầu
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // ✅ Đăng ký vào hệ thống quản lý block
        FallingBlockManager.Register(this);
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

    private IEnumerator StartFalling()
    {
        isFalling = true;
        yield return new WaitForSeconds(delayBeforeFall);

        rb.bodyType = RigidbodyType2D.Dynamic;
        startRotate = true;
        startZ = transform.eulerAngles.z;
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // ❌ KHÔNG Destroy — chỉ ẩn object để có thể reset lại
        gameObject.SetActive(false);
    }

    // ✅ Gọi từ FallingBlockManager để hồi sinh block
    public void ResetBlock()
    {
        gameObject.SetActive(true); // ✅ Bật lại object nếu đã bị tắt

        StopAllCoroutines();
        isFalling = false;
        startRotate = false;
        rotateTimer = 0f;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
