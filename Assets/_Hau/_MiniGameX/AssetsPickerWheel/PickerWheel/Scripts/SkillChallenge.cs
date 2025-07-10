using DG.Tweening;
using EasyUI.PickerWheelUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SkillChallenge : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillChallengePanel;
    [SerializeField] private Transform arrowSequenceParent;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Button skipButton;

    [Header("Arrow Sprites")]
    [SerializeField] private Sprite upArrowSprite;
    [SerializeField] private Sprite downArrowSprite;
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;

    [Header("Settings")]
    [SerializeField] private int[] sequenceLengths = { 3, 4, 5 }; // Length cho từng level
    [SerializeField] private float[] timeLimits = { 5f, 4f, 3f }; // Time limit cho từng level

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inputSoundClip;
    [SerializeField] private AudioClip successSoundClip;
    [SerializeField] private AudioClip failSoundClip;
    [SerializeField] private AudioClip level1CompleteClip;
    [SerializeField] private AudioClip level2CompleteClip;
    [SerializeField] private AudioClip level3CompleteClip;

    // Events
    public System.Action<bool> OnChallengeComplete;

    private ArrowSequence currentSequence;
    private int currentInputIndex = 0;
    private float currentTimer = 0f;
    private bool isActive = false;
    private List<GameObject> arrowObjects = new List<GameObject>();
    private int currentLevel = 0;
    private bool[] levelResults = new bool[3]; // Track kết quả 3 level


    private void Start()
    {
        skillChallengePanel.SetActive(false);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipChallenge);
    }

    private void Update()
    {
        if (!isActive) return;

        HandleInput();
        UpdateTimer();
    }

    public void StartChallenge(int level)
    {
        currentLevel = level;
        currentInputIndex = 0;

        // Reset kết quả level hiện tại
        if (level < levelResults.Length)
        {
            levelResults[level] = false;
        }

        // Tạo sequence dựa trên level
        int sequenceLength = sequenceLengths[Mathf.Min(level, sequenceLengths.Length - 1)];
        float timeLimit = timeLimits[Mathf.Min(level, timeLimits.Length - 1)];

        currentSequence = new ArrowSequence(sequenceLength, timeLimit);
        currentTimer = timeLimit;

        SetupUI();
        skillChallengePanel.SetActive(true);
        isActive = true;

        // Animation hiện panel
        skillChallengePanel.transform.localScale = Vector3.zero;
        skillChallengePanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    private void SetupUI()
    {
        // Clear previous arrows
        foreach (GameObject arrow in arrowObjects)
        {
            DestroyImmediate(arrow);
        }
        arrowObjects.Clear();

        // Create arrow sequence UI
        for (int i = 0; i < currentSequence.arrows.Length; i++)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, arrowSequenceParent);
            Image arrowImage = arrowObj.GetComponent<Image>();

            // Set sprite based on direction
            switch (currentSequence.arrows[i])
            {
                case ArrowDirection.Up:
                    arrowImage.sprite = upArrowSprite;
                    break;
                case ArrowDirection.Down:
                    arrowImage.sprite = downArrowSprite;
                    break;
                case ArrowDirection.Left:
                    arrowImage.sprite = leftArrowSprite;
                    break;
                case ArrowDirection.Right:
                    arrowImage.sprite = rightArrowSprite;
                    break;
            }

            arrowObjects.Add(arrowObj);

            // Animation xuất hiện
            arrowObj.transform.localScale = Vector3.zero;
            arrowObj.transform.DOScale(1f, 0.2f).SetDelay(i * 0.1f);
        }

        // Update UI
        if (levelText != null)
            levelText.text = $"Level {currentLevel + 1}";

        if (timerSlider != null)
        {
            timerSlider.maxValue = currentSequence.timeLimit;
            timerSlider.value = currentTimer;
        }
    }

    private void HandleInput()
    {
        ArrowDirection inputDirection = ArrowDirection.Up;
        bool hasInput = false;

        // Kiểm tra input
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            inputDirection = ArrowDirection.Up;
            hasInput = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            inputDirection = ArrowDirection.Down;
            hasInput = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            inputDirection = ArrowDirection.Left;
            hasInput = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            inputDirection = ArrowDirection.Right;
            hasInput = true;
        }

        if (hasInput)
        {
            ProcessInput(inputDirection);
        }
    }

    // Thay đổi method ProcessInput:
    private void ProcessInput(ArrowDirection inputDirection)
    {
        // Phát âm thanh khi ấn phím
        PlaySound(inputSoundClip);

        // Kiểm tra input có đúng không
        if (inputDirection == currentSequence.arrows[currentInputIndex])
        {
            // Đúng - highlight arrow
            HighlightArrow(currentInputIndex, true);
            currentInputIndex++;

            // Kiểm tra đã hoàn thành sequence chưa
            if (currentInputIndex >= currentSequence.arrows.Length)
            {
                CompleteChallenge(true);
                return;
            }
        }
        else
        {
            // Sai - highlight arrow sai và kết thúc
            HighlightArrow(currentInputIndex, false);
            CompleteChallenge(false);
        }
    }

    /*private void ProcessInput(ArrowDirection inputDirection)
    {
        // Kiểm tra input có đúng không
        if (inputDirection == currentSequence.arrows[currentInputIndex])
        {
            // Đúng - highlight arrow
            HighlightArrow(currentInputIndex, true);
            currentInputIndex++;

            // Kiểm tra đã hoàn thành sequence chưa
            if (currentInputIndex >= currentSequence.arrows.Length)
            {
                CompleteChallenge(true);
                return;
            }
        }
        else
        {
            // Sai - highlight arrow sai và kết thúc
            HighlightArrow(currentInputIndex, false);
            CompleteChallenge(false);
        }
    }*/

    private void HighlightArrow(int index, bool isCorrect)
    {
        if (index < arrowObjects.Count)
        {
            Image arrowImage = arrowObjects[index].GetComponent<Image>();
            arrowImage.color = isCorrect ? Color.green : Color.red;

            // Animation feedback
            arrowObjects[index].transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        }
    }

    private void UpdateTimer()
    {
        currentTimer -= Time.deltaTime;

        if (timerText != null)
            timerText.text = $"{currentTimer:F1}s";

        if (timerSlider != null)
            timerSlider.value = currentTimer;

        // Hết thời gian
        if (currentTimer <= 0)
        {
            CompleteChallenge(false);
        }
    }

    // Thay đổi method CompleteChallenge:
    private void CompleteChallenge(bool success)
    {
        isActive = false;

        // Lưu kết quả level
        if (currentLevel < levelResults.Length)
        {
            levelResults[currentLevel] = success;
        }

        // Phát âm thanh dựa trên kết quả
        if (success)
        {
            // Phát âm thanh hoàn thành level
            switch (currentLevel)
            {
                case 0:
                    PlaySound(level1CompleteClip);
                    break;
                case 1:
                    PlaySound(level2CompleteClip);
                    break;
                case 2:
                    PlaySound(level3CompleteClip);
                    break;
                default:
                    PlaySound(successSoundClip);
                    break;
            }
        }
        else
        {
            PlaySound(failSoundClip);
        }

        // Animation ẩn panel
        skillChallengePanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => skillChallengePanel.SetActive(false));

        // Gọi event
        OnChallengeComplete?.Invoke(success);
    }

    // Thêm method PlaySound:
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /*  private void CompleteChallenge(bool success)
      {
          isActive = false;

          // Animation ẩn panel
          skillChallengePanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
              .OnComplete(() => skillChallengePanel.SetActive(false));

          // Gọi event
          OnChallengeComplete?.Invoke(success);
      }*/

    private void SkipChallenge()
    {
        CompleteChallenge(false);
    }

    // Thêm method để lấy số level đã hoàn thành:
    public int GetCompletedLevels()
    {
        int completed = 0;
        for (int i = 0; i < levelResults.Length; i++)
        {
            if (levelResults[i]) completed++;
        }
        return completed;
    }

    // Thêm method để trả về kết quả chi tiết của từng level:
    public bool[] GetLevelResults()
    {
        return levelResults;
    }
}
