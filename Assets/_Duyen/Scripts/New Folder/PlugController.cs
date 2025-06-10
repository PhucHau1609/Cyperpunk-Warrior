using UnityEngine;

public class PlugController : MonoBehaviour
{
    public GameObject lightBulb; // Bóng đèn
    public GameObject winUI;     // UI hoặc hiệu ứng thắng

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Socket"))
        {
            // Snap vào ổ cắm
            transform.position = other.transform.position;

            // Sáng bóng đèn (ví dụ: đổi sprite)
            lightBulb.GetComponent<SpriteRenderer>().color = Color.yellow;

            // Hiển thị UI thắng
            winUI.SetActive(true);

            // Vô hiệu hoá điều khiển
            enabled = false;
        }
    }
}
