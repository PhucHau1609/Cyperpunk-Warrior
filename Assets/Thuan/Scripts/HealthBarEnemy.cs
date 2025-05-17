using UnityEngine;
using UnityEngine.UI;

public class HealthBarEnemy : MonoBehaviour
{
    public Slider Slider;
    public Color Low;
    public Color High;
    public Vector3 Offset;

    private bool hasTakenDamage = false; // Flag: đã bị đánh chưa

    public void SetHealth(float health, float maxHealth)
    {
        // Nếu máu < max → bị đánh → set flag
        if (health < maxHealth)
        {
            hasTakenDamage = true;
        }

        // Nếu chưa bị đánh, ẩn
        if (!hasTakenDamage)
        {
            Slider.gameObject.SetActive(false);
            return;
        }

        // Nếu bị đánh: hiện lên, cập nhật màu và giá trị
        if (health > 0)
        {
            Slider.gameObject.SetActive(true);
            Slider.maxValue = maxHealth;
            Slider.value = health;
            Slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(Low, High, Slider.normalizedValue);
        }
        else
        {
            Slider.gameObject.SetActive(false); // máu hết → ẩn luôn
        }
    }

    void Update()
    {
        Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }
}
