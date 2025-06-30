using UnityEngine;
using UnityEngine.UI;

public class Boss2HealthBar : MonoBehaviour
{
    [Header("Health Bar References")]
    public Canvas healthBarCanvas;
    public Slider healthSlider;
    public Gradient healthBarGradient;

    private void Awake()
    {
        // Khởi tạo Gradient
        InitializeHealthBarGradient();
    }

    private void Start()
    {
        // Ẩn canvas khi bắt đầu
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = true;
        }
    }

    private void InitializeHealthBarGradient()
    {
        healthBarGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0].color = Color.green;    // 100% máu
        colorKeys[0].time = 1f;
        colorKeys[1].color = Color.yellow;   // 50% máu
        colorKeys[1].time = 0.5f;
        colorKeys[2].color = Color.red;      // 1% máu
        colorKeys[2].time = 0.01f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = 1f;
            alphaKeys[i].time = colorKeys[i].time;
        }

        healthBarGradient.SetKeys(colorKeys, alphaKeys);
    }

    public void ShowHealthBar(float healthPercentage)
    {
        if (healthBarCanvas == null || healthSlider == null) return;

        // Hiện canvas khi % máu < 1 (không phải full máu)
        healthBarCanvas.enabled = healthPercentage < 1f;

        // Cập nhật giá trị slider
        healthSlider.value = healthPercentage;

        // Cập nhật màu sắc dựa trên gradient
        if (healthSlider.fillRect != null)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            fillImage.color = healthBarGradient.Evaluate(healthPercentage);
        }
    }

    public void HideHealthBar()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = false;
        }
    }
}