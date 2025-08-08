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

    [Header("Player Reference")]
    public PlayerMovement player;

    [Header("Interaction Settings")]
    public Transform interactionPoint;
    public float interactionDistance = 3f;

    private float timer = 0f;
    private int targetTime;
    private bool isRunning = false;
    private bool hasStopped = false;
    private float stoppedTime = 0f;
    private bool gameInProgress = false;
    private bool gameWon = false;

    // ✅ THÊM: Lưu trạng thái ban đầu của objectToShowAfterWin
    private bool originalObjectState;

    // Animation mask
    private bool isMaskAnimating = false;
    private float maskAnimationDuration = 1f;
    private float maskAnimationTimer = 0f;

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
            // ✅ THÊM: Lưu trạng thái ban đầu của object
            originalObjectState = objectToShowAfterWin.activeSelf;
            objectToShowAfterWin.SetActive(false);
        }

        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerMovement>();
        }
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

            // Sau 3s thì bắt đầu animation trượt mask từ trên xuống
            if (timer >= 3f && !imgMask.activeSelf)
            {
                imgMask.SetActive(true);
                isMaskAnimating = true;
                maskAnimationTimer = 0f;

                Image maskImage = imgMask.GetComponent<Image>();
                if (maskImage != null)
                {
                    maskImage.fillAmount = 0f; // Bắt đầu trượt từ trên xuống
                }
            }

            UpdateTimerDisplay(timer);
        }

        // Cập nhật hiệu ứng trượt mask
        if (isMaskAnimating)
        {
            maskAnimationTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(maskAnimationTimer / maskAnimationDuration);

            Image maskImage = imgMask.GetComponent<Image>();
            if (maskImage != null)
            {
                maskImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            }

            if (progress >= 1f)
            {
                isMaskAnimating = false;
            }
        }

        // Tự động vô hiệu hóa nút nếu player ở xa
        if (btnOpenMiniGame != null && interactionPoint != null && player != null)
        {
            float dist = Vector3.Distance(player.transform.position, interactionPoint.position);
            btnOpenMiniGame.interactable = (dist <= interactionDistance && !gameWon);
        }
    }

    public void OpenMiniGame()
    {
        if (gameWon) return;

        // Chặn mở nếu ở xa
        if (interactionPoint != null && player != null)
        {
            float dist = Vector3.Distance(player.transform.position, interactionPoint.position);
            if (dist > interactionDistance)
                return;
        }

        miniGamePanel.SetActive(true);
        gameInProgress = false;
        ResetGame();
        wallShrinker.PauseShrinking();

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerMovement>();

        if (player != null)
            player.SetCanMove(false);
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
            Invoke(nameof(ShowWinObject), 2f);
        }

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerMovement>();

        if (player != null)
            player.SetCanMove(true);

        ResetGame();
    }

    void StartGame()
    {
        if (gameInProgress || gameWon) return;

        ResetGame();

        targetTime = Random.Range(5, 11);
        txtTargetTime.text = "Thời Gian Vô Hiệu Hoá: " + targetTime.ToString("00") + ":00";

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
            txtResult.text = "THÀNH CÔNG!";
            txtResult.color = Color.green;

            wallShrinker.StopShrinking();
            gameWon = true;
            Invoke(nameof(CloseMiniGame), 1.5f);
        }
        else
        {
            txtResult.text = "THẤT BẠI!";
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

        isMaskAnimating = false;
        maskAnimationTimer = 0f;

        Image maskImage = imgMask.GetComponent<Image>();
        if (maskImage != null)
            maskImage.fillAmount = 0f; // đặt lại về 0 để sẵn sàng trượt lại
    }

    void ShowWinObject()
    {
        objectToShowAfterWin.SetActive(true);
    }

    // ✅ CHỈNH SỬA: ResetState() để xử lý 2 trường hợp
    public void ResetState()
    {
        gameInProgress = false;

        // ✅ LOGIC MỚI: Reset objectToShowAfterWin về trạng thái ban đầu
        // Trường hợp 1: Chưa thắng game → object vẫn ẩn (originalObjectState = false)
        // Trường hợp 2: Đã thắng game → object cũng bị reset về trạng thái ban đầu (ẩn)
        if (objectToShowAfterWin != null)
        {
            objectToShowAfterWin.SetActive(originalObjectState);
        }

        // Reset game state
        gameWon = false;
        ResetGame();

        miniGamePanel.SetActive(false);

        if (wallShrinker != null)
            wallShrinker.ResetState();
    }

    // ✅ THÊM: Method để kiểm tra trạng thái game (optional - cho debugging)
    public bool IsGameCompleted()
    {
        return gameWon;
    }

    // ✅ THÊM: Method để force reset object state (optional - cho trường hợp đặc biệt)
    public void ForceResetObjectState()
    {
        if (objectToShowAfterWin != null)
        {
            objectToShowAfterWin.SetActive(originalObjectState);
        }
    }
}