using UnityEngine;

public class PedestalBounce : MonoBehaviour
{
    public float bounceAmount = 1f;     // Độ nhún
    public float bounceTime = 0.1f;       // Thời gian nhún

    private Vector3 originalPos;
    private bool isBouncing = false;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBouncing) return;

        if (collision.collider.CompareTag("Player"))
        {
            // Kiểm tra player đang rơi xuống pedestal
            if (collision.relativeVelocity.y <= 0f)
            {
                StartCoroutine(BounceEffect());
            }
        }
    }

    System.Collections.IEnumerator BounceEffect()
    {
        isBouncing = true;

        // Nhún xuống
        transform.localPosition = originalPos + Vector3.down * bounceAmount;
        yield return new WaitForSeconds(bounceTime);

        // Trở lại vị trí cũ
        transform.localPosition = originalPos;

        isBouncing = false;
    }
}
