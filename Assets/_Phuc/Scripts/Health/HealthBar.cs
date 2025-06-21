using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Image fillImage; // Gán ảnh thanh máu (nên là Filled-Horizontal)

    [Header("Color Settings")]
    public Gradient colorGradient = new Gradient(); // Chuyển màu tùy phần trăm máu

    public void SetHealth(float current, float max)
    {
        if (fillImage == null || max <= 0) return;

        // Clamp để tránh lỗi âm/NaN
        float percent = Mathf.Clamp01(current / max);
        fillImage.fillAmount = percent;

        // Chỉ đổi màu nếu fill kiểu Horizontal
        if (fillImage.type == Image.Type.Filled &&
            fillImage.fillMethod == Image.FillMethod.Horizontal)
        {
            fillImage.color = colorGradient.Evaluate(percent);
        }
    }
}
