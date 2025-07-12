using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombDefuseMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject miniGamePanel;
    public Button btnOpenMiniGame;
    public Button btnCloseMiniGame;

    public Button btnStartGame;
    public Button btnStopTime;
    public Button btnConfirm;

    public TextMeshProUGUI txtTargetTime;
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtResult;
    public GameObject imgMask;

    [Header("Wall Controller")]
    public WallShrinker wallShrinker;

    [Header("Reveal Object After Success")]
    public GameObject objectToShowAfterWin;

    private float timer = 0f;
    private int targetTime;
    private bool isRunning = false;
    private bool hasStopped = false;
    private float stoppedTime = 0f;
    private bool gameInProgress = false;
    private bool gameWon = false;

    void Start()
    {
        btnOpenMiniGame.onClick.AddListener(OpenMiniGame);
        btnCloseMiniGame.onClick.AddListener(CloseMiniGame);

        btnStartGame.onClick.AddListener(StartGame);
        btnStopTime.onClick.AddListener(StopTimer);
        btnConfirm.onClick.AddListener(CheckResult);

        miniGamePanel.SetActive(false);
        btnOpenMiniGame.gameObject.SetActive(false);

        if (objectToShowAfterWin != null)
        {
            objectToShowAfterWin.SetActive(false);
        }
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

            if (timer >= 3f && !imgMask.activeSelf)
            {
                imgMask.SetActive(true);
            }

            UpdateTimerDisplay(timer);
        }
    }

    public void OpenMiniGame()
    {
        if (gameWon) return;

        miniGamePanel.SetActive(true);
        gameInProgress = false;
        ResetGame();
        wallShrinker.PauseShrinking();
    }

    public void CloseMiniGame()
    {
        miniGamePanel.SetActive(false);
        if (!gameWon)
        {
            wallShrinker.ResumeShrinking();
        }
        else if (objectToShowAfterWin != null)
        {
            objectToShowAfterWin.SetActive(true);
        }
        ResetGame();
    }

    void StartGame()
    {
        if (gameInProgress) return;

        ResetGame();

        targetTime = Random.Range(5, 11);
        txtTargetTime.text = "Defuse Time: " + targetTime.ToString("00") + ":00";

        timer = 0f;
        isRunning = true;
        gameInProgress = true;
        imgMask.SetActive(false);

        wallShrinker.ResumeShrinking();
    }

    void StopTimer()
    {
        if (!isRunning || hasStopped) return;

        hasStopped = true;
        isRunning = false;
        stoppedTime = timer;

        imgMask.SetActive(false);
        UpdateTimerDisplay(stoppedTime);
    }

    void CheckResult()
    {
        if (!hasStopped) return;

        int seconds = (int)stoppedTime;
        int centiseconds = (int)((stoppedTime - seconds) * 100);

        if (seconds == targetTime && centiseconds < 100)
        {
            txtResult.text = "Task Complete!";
            txtResult.color = Color.green;

            wallShrinker.StopShrinking();
            gameWon = true;
            Invoke(nameof(CloseMiniGame), 1.5f);
        }
        else
        {
            txtResult.text = "Failed!";
            txtResult.color = Color.red;
        }

        gameInProgress = false;
    }

    void UpdateTimerDisplay(float time)
    {
        int seconds = (int)time;
        int centiseconds = (int)((time - seconds) * 100);
        txtTimer.text = seconds.ToString("00") + ":" + centiseconds.ToString("00");
    }

    void ResetGame()
    {
        isRunning = false;
        hasStopped = false;
        timer = 0f;

        txtTimer.text = "00:00";
        txtResult.text = "";
        txtResult.color = Color.white;
        imgMask.SetActive(false);
    }
}
