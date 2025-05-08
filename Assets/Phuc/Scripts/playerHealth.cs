using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [Header("Máu Player")]
    public float health = 5f;
    public float maxHealth = 5f;

    [Header("UI Thanh Máu")]
    public Image healthBar;

    void Start()
    {
        // Khởi tạo giá trị máu tối đa bằng máu hiện tại nếu chưa thiết lập
        if (maxHealth <= 0) maxHealth = health;
    }

    void Update()
    {
        // Cập nhật UI thanh máu
        healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            health = 0;
            Debug.Log("Player chết!");
            // TODO: Thêm xử lý chết (chết animation, load scene, v.v.)
        }
    }
}
