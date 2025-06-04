using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartReactorGame : MonoBehaviour
{
    [Header("Buttons & Displays")]
    public Button[] inputButtons;                // 9 nút bên phải
    public Image[] displayPattern;               // 9 ô bên trái để hiển thị thứ tự

    [Header("Indicators")]
    public Image[] levelIndicators;              // 5 nút tròn bên trái (level)
    public Image[] progressIndicators;           // 5 nút tròn bên phải (bấm đúng)

    [Header("Colors")]
    public Color highlightColor = Color.cyan;
    public Color greenColor = Color.green;
    public Color redColor = Color.red;
    public Color buttonDefaultColor = Color.white;      // Cho nút & indicator
    public Color displayDefaultColor = Color.black;     // Cho các ô hiển thị bên trái

    [Header("Text Display")]
    public TMP_Text failedText;
    public TMP_Text completedText;

    private List<int> pattern = new List<int>();
    private int inputIndex = 0;
    private int currentLevel = 1;

    void Start()
    {
        for (int i = 0; i < inputButtons.Length; i++)
        {
            int idx = i;
            inputButtons[i].onClick.AddListener(() => OnButtonPressed(idx));
        }

        // ❌ Không tự chạy mini game ở đây nữa
        // ResetAll();
        // StartCoroutine(ShowPattern());
    }

    // ✅ Gọi hàm này từ nút "Start"
    public void StartGame()
    {
        Debug.Log("▶️ StartGame được gọi"); // Kiểm tra bằng log
        ResetAll();
        StartCoroutine(ShowPattern());
    }

    void ResetAll()
    {
        ResetLevelIndicators();
        ResetProgressIndicators();
        failedText.gameObject.SetActive(false);
        completedText.gameObject.SetActive(false);
        currentLevel = 1;
    }

    IEnumerator ShowPattern()
    {
        pattern.Clear();
        inputIndex = 0;
        ResetProgressIndicators();
        failedText.gameObject.SetActive(false);
        completedText.gameObject.SetActive(false);

        EnableInput(false);

        int patternLength = currentLevel;

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < patternLength; i++)
        {
            int randomIndex = Random.Range(0, 9);
            pattern.Add(randomIndex);

            displayPattern[randomIndex].color = highlightColor;
            SoundMiniGame4.Instance?.PlayPatternSound();

            yield return new WaitForSeconds(0.5f);
            displayPattern[randomIndex].color = displayDefaultColor;
            yield return new WaitForSeconds(0.2f);
        }

        EnableInput(true);
    }

    public void OnButtonPressed(int index)
    {
        SoundMiniGame4.Instance?.PlayButtonPressSound();

        if (index == pattern[inputIndex])
        {
            progressIndicators[inputIndex].color = greenColor;
            inputIndex++;

            if (inputIndex >= pattern.Count)
            {
                EnableInput(false);
                StartCoroutine(NextLevel());
            }
        }
        else
        {
            EnableInput(false);
            StartCoroutine(HandleFail());
        }
    }

    IEnumerator NextLevel()
    {
        levelIndicators[currentLevel - 1].color = greenColor;

        yield return new WaitForSeconds(1f);

        currentLevel++;

        if (currentLevel > 5)
        {
            completedText.gameObject.SetActive(true);
            failedText.gameObject.SetActive(false);
            SoundMiniGame4.Instance?.PlayWinSound();
            yield break;
        }

        StartCoroutine(ShowPattern());
    }

    IEnumerator HandleFail()
    {
        failedText.gameObject.SetActive(true);
        completedText.gameObject.SetActive(false);
        SoundMiniGame4.Instance?.PlayFailSound();

        for (int i = 0; i < 2; i++)
        {
            SetAllButtonsColor(redColor);
            SetProgressIndicatorsColor(redColor);
            yield return new WaitForSeconds(0.2f);

            SetAllButtonsColor(buttonDefaultColor);
            SetProgressIndicatorsColor(buttonDefaultColor);
            yield return new WaitForSeconds(0.2f);
        }

        ResetAll();
        StartCoroutine(ShowPattern());
    }

    void EnableInput(bool enable)
    {
        foreach (var btn in inputButtons)
        {
            btn.interactable = enable;
        }
    }

    void SetAllButtonsColor(Color c)
    {
        foreach (var btn in inputButtons)
        {
            btn.image.color = c;
        }
    }

    void SetProgressIndicatorsColor(Color c)
    {
        foreach (var img in progressIndicators)
        {
            img.color = c;
        }
    }

    void ResetLevelIndicators()
    {
        foreach (var img in levelIndicators)
        {
            img.color = buttonDefaultColor;
        }
    }

    void ResetProgressIndicators()
    {
        foreach (var img in progressIndicators)
        {
            img.color = buttonDefaultColor;
        }
    }
}
