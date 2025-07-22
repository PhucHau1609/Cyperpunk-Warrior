using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance { get; private set; } // ✅ Singleton

    [Header("UI Components")]
    public Image energyBar;
    public Image qImage;
    public Image eImage;
    public Image rImage;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy = 100f;
    public float energyRegenAmount = 1f;
    public float energyRegenInterval = 2f;

    private void Awake()
    {
        // ✅ Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        this.Start();
    }

    private void Start()
    {
        StartCoroutine(RegenerateEnergy());
    }

    private void Update()
    {
        UpdateBars(); // ✅ Chỉ cập nhật UI, không xử lý input
    }

    /// <summary>
    /// Dùng năng lượng nếu đủ. Trả về true nếu dùng thành công.
    /// </summary>
    public bool UseEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            UpdateBars();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gọi hiệu ứng nhấp nháy lên UI cụ thể
    /// </summary>
    public void TriggerBlink(Image target)
    {
        if (target != null)
            StartCoroutine(BlinkImage(target));
    }

    /// <summary>
    /// Phục hồi năng lượng nếu chưa đầy
    /// </summary>
    private IEnumerator RegenerateEnergy()
    {
        while (true)
        {
            yield return new WaitForSeconds(energyRegenInterval);

            if (currentEnergy < maxEnergy)
            {
                currentEnergy = Mathf.Clamp(currentEnergy + energyRegenAmount, 0f, maxEnergy);
                UpdateBars();
            }
        }
    }

    /// <summary>
    /// Cập nhật thanh năng lượng
    /// </summary>
    private void UpdateBars()
    {
        if (energyBar != null)
            energyBar.fillAmount = currentEnergy / maxEnergy;
    }

    /// <summary>
    /// Hiệu ứng nhấp nháy UI
    /// </summary>
    private IEnumerator BlinkImage(Image img)
    {
        Color originalColor = img.color;

        for (int i = 0; i < 3; i++)
        {
            img.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            img.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
