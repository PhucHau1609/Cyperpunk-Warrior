using UnityEngine;
using UnityEngine.UI;

public class HealthBarEnemy : MonoBehaviour
{
    public static HealthBarEnemy Instance;

    public Slider slider;
    public Gradient gradient;
    public Image fillImage;

    private Transform target;
    private Vector3 offset = new Vector3(0, 1.5f, 0);
    private Canvas canvas;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    public void ShowHealthBar(Transform enemyTransform, float healthPercent)
    {
        target = enemyTransform;
        slider.value = healthPercent;
        fillImage.color = gradient.Evaluate(healthPercent);
        canvas.enabled = true;
    }

    public void HideHealthBar()
    {
        canvas.enabled = false;
        target = null;
    }
}
