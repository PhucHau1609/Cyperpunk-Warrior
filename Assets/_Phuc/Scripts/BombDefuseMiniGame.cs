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

    private float timer = 0f;
    private int targetTime;
    private bool isRunning = false;
    private bool hasStopped = false;
    private float stoppedTime = 0f;

    void Start()
    {
        // Gắn sự kiện nút mở/đóng mini game
        btnOpenMiniGame.onClick.AddListener(OpenMiniGame);
        btnCloseMiniGame.onClick.AddListener(CloseMiniGame);

        // Gắn sự kiện nút trong game
        btnStartGame.onClick.AddListener(StartGame);
        btnStopTime.onClick.AddListener(StopTimer);
        btnConfirm.onClick.AddListener(CheckResult);

        // Ban đầu mini game sẽ ẩn
        miniGamePanel.SetActive(false);
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

            if (timer >= 3f && !imgMask.activeSelf)
            {
                imgMask.SetActive(true); // Che timer sau 3 giây
            }

            UpdateTimerDisplay(timer);
        }
    }

    void StartGame()
    {
        ResetGame();

        targetTime = Random.Range(5, 11); // Ngẫu nhiên từ 5 đến 10
        txtTargetTime.text = "Defuse Time: " + targetTime.ToString("00") + ":00";

        timer = 0f;
        isRunning = true;
        imgMask.SetActive(false);
    }

    void StopTimer()
    {
        if (!isRunning || hasStopped) return;

        hasStopped = true;
        isRunning = false;
        stoppedTime = timer;

        imgMask.SetActive(false); // Hiện lại timer
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
        }
        else
        {
            txtResult.text = "Failed!";
            txtResult.color = Color.red;
        }
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

    public void OpenMiniGame()
    {
        miniGamePanel.SetActive(true);
        ResetGame();
    }

    public void CloseMiniGame()
    {
        miniGamePanel.SetActive(false);
        ResetGame();
    }
}
