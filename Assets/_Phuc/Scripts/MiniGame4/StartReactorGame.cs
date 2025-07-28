using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartReactorGame : MonoBehaviour
{
    public GameObject panel;
    public GameObject laserBlock;
    public Button openMiniGameButton;

    public Button[] inputButtons;
    public Image[] displayPattern;

    public Image[] levelIndicators;
    public Image[] progressIndicators;

    public Color highlightColor = Color.cyan;
    public Color greenColor = Color.green;
    public Color redColor = Color.red;
    public Color buttonDefaultColor = Color.white;
    public Color displayDefaultColor = Color.black;

    public TMP_Text failedText;
    public TMP_Text completedText;

    private List<int> pattern = new List<int>();
    private int inputIndex = 0;
    private int currentLevel = 1;

    private bool canStartGame = true;

    public PlayerMovement player;

    void Start()
    {
        panel.SetActive(false);

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerMovement>();

        for (int i = 0; i < inputButtons.Length; i++)
        {
            int idx = i;
            inputButtons[i].onClick.AddListener(() => OnButtonPressed(idx));
        }
    }

    public void OpenMiniGame()
    {

        GameStateManager.Instance.SetState(GameState.MiniGame);

        panel.SetActive(true);

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerMovement>();

        if (player != null)
            player.SetCanMove(false);

        canStartGame = true; // Cho phép bấm Start 1 lần khi mở panel
    }

    public void CloseMiniGame()
    {
        if (!panel.activeSelf) return;
        GameStateManager.Instance.ResetToGameplay();

        panel.SetActive(false);

        if (player != null)
            player.SetCanMove(true);
    }

    public void StartGame()
    {
        if (!canStartGame) return;

        canStartGame = false; // Ngăn bấm lại
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
            int randomIndex = Random.Range(0, displayPattern.Length);
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
            yield return new WaitForSeconds(1.5f);

            CloseMiniGame();

            if (laserBlock != null)
                laserBlock.SetActive(false);

            if (openMiniGameButton != null)
                openMiniGameButton.interactable = false;

            canStartGame = false; // Không được chơi lại sau khi thắng
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

        ResetAll(); // ✅ Reset trạng thái nhưng KHÔNG gán lại `canStartGame = true`
        StartCoroutine(ShowPattern()); // Tự động chơi lại
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
