using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyProgressUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool hideWhenComplete = true;
    [SerializeField] private float hideDelay = 2f;

    [Header("Animation")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationSpeed = 2f;

    private CanvasGroup canvasGroup;
    private float targetProgress = 0f;
    private bool isComplete = false;

    void Awake()
    {
        // Auto-find components if not assigned
        if (progressPanel == null)
            progressPanel = gameObject;

        if (progressBar == null)
            progressBar = GetComponentInChildren<Slider>();

        if (progressText == null)
            progressText = GetComponentInChildren<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // Subscribe to EnemyManager events
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnKillCountChanged += UpdateProgress;
            EnemyManager.Instance.OnAllEnemiesKilled += OnAllEnemiesKilled;

            // Initialize display
            UpdateProgress(EnemyManager.Instance.GetCurrentKills(), EnemyManager.Instance.GetRequiredKills());
        }

        // Set initial visibility
        if (!showOnStart)
        {
            SetVisibility(false);
        }

        // Initialize title
        if (titleText != null)
        {
            titleText.text = "Tiêu diệt Enemy";
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnKillCountChanged -= UpdateProgress;
            EnemyManager.Instance.OnAllEnemiesKilled -= OnAllEnemiesKilled;
        }
    }

    void Update()
    {
        // Smooth progress bar animation
        if (useAnimation && progressBar != null)
        {
            float currentProgress = progressBar.value;
            if (Mathf.Abs(currentProgress - targetProgress) > 0.01f)
            {
                progressBar.value = Mathf.MoveTowards(currentProgress, targetProgress, animationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Cập nhật tiến trình
    /// </summary>
    private void UpdateProgress(int currentKills, int requiredKills)
    {
        if (isComplete) return;

        targetProgress = requiredKills > 0 ? (float)currentKills / requiredKills : 0f;

        // Update progress bar
        if (progressBar != null)
        {
            if (!useAnimation)
                progressBar.value = targetProgress;
        }

        // Update text
        if (progressText != null)
        {
            progressText.text = $"{currentKills}/{requiredKills}";
        }

        // Show UI if hidden
        if (!progressPanel.activeInHierarchy && showOnStart)
        {
            SetVisibility(true);
        }

        Debug.Log($"[EnemyProgressUI] Progress updated: {currentKills}/{requiredKills} ({targetProgress:P0})");
    }

    /// <summary>
    /// Khi hoàn thành nhiệm vụ
    /// </summary>
    private void OnAllEnemiesKilled()
    {
        isComplete = true;

        // Update final progress
        if (progressBar != null)
            progressBar.value = 1f;

        if (progressText != null)
        {
            if (EnemyManager.Instance != null)
            {
                int required = EnemyManager.Instance.GetRequiredKills();
                progressText.text = $"{required}/{required}";
            }
            else
            {
                progressText.text = "Hoàn thành!";
            }
        }

        // Change title color to green
        if (titleText != null)
        {
            titleText.color = Color.green;
            titleText.text = "Hoàn thành!";
        }

        // Hide after delay
        if (hideWhenComplete)
        {
            Invoke(nameof(HidePanel), hideDelay);
        }

        Debug.Log("[EnemyProgressUI] All enemies killed! Mission complete.");
    }

    /// <summary>
    /// Hiển thị/ẩn panel
    /// </summary>
    public void SetVisibility(bool visible)
    {
        if (progressPanel != null)
            progressPanel.SetActive(visible);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    /// <summary>
    /// Ẩn panel (được gọi bởi Invoke)
    /// </summary>
    private void HidePanel()
    {
        SetVisibility(false);
    }

    /// <summary>
    /// Show panel với animation
    /// </summary>
    public void ShowWithAnimation()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            SetVisibility(true);
        }
    }

    /// <summary>
    /// Hide panel với animation
    /// </summary>
    public void HideWithAnimation()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            SetVisibility(false);
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        progressPanel.SetActive(true);
        canvasGroup.alpha = 0f;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += animationSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= animationSpeed * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        progressPanel.SetActive(false);
    }

    /// <summary>
    /// Reset UI về trạng thái ban đầu
    /// </summary>
    public void ResetUI()
    {
        isComplete = false;
        targetProgress = 0f;

        if (progressBar != null)
            progressBar.value = 0f;

        if (progressText != null)
            progressText.text = "0/0";

        if (titleText != null)
        {
            titleText.color = Color.white;
            titleText.text = "Tiêu diệt Enemy";
        }

        CancelInvoke();

        if (showOnStart)
            SetVisibility(true);
    }

    // Public methods for external control
    public void ForceShow() => SetVisibility(true);
    public void ForceHide() => SetVisibility(false);
    public void ToggleVisibility() => SetVisibility(!progressPanel.activeInHierarchy);
}