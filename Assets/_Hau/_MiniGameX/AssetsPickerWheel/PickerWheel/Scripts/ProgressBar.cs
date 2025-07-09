using DG.Tweening;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject progressBarPanel;
    [SerializeField] private RectTransform progressBarFill;
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform[] checkpoints; // 3 checkpoints cho 3 level

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 200f;
    [SerializeField] private float progressBarWidth = 800f;

    public System.Action<int> OnReachCheckpoint;

    private bool isMoving = false;
    private int currentCheckpoint = 0;
    private float currentPosition = 0f;

    private void Start()
    {
        progressBarPanel.SetActive(false);
        SetupCheckpoints();
    }

    private void SetupCheckpoints()
    {
        // Setup vị trí các checkpoint
        for (int i = 0; i < checkpoints.Length; i++)
        {
            float xPos = (progressBarWidth / (checkpoints.Length + 1)) * (i + 1) - (progressBarWidth / 2);
            checkpoints[i].localPosition = new Vector3(xPos, 0, 0);
        }
    }

    public void StartProgress()
    {
        currentCheckpoint = 0;
        currentPosition = -progressBarWidth / 2;

        progressBarPanel.SetActive(true);
        arrow.localPosition = new Vector3(currentPosition, 0, 0);

        isMoving = true;

        // Animation hiện progress bar
        progressBarPanel.transform.localScale = Vector3.zero;
        progressBarPanel.transform.DOScale(1f, 0.3f);
    }

    private void Update()
    {
        if (!isMoving) return;

        // Di chuyển arrow
        currentPosition += moveSpeed * Time.deltaTime;
        arrow.localPosition = new Vector3(currentPosition, 0, 0);

        // Kiểm tra có đến checkpoint không
        if (currentCheckpoint < checkpoints.Length)
        {
            float checkpointX = checkpoints[currentCheckpoint].localPosition.x;
            if (currentPosition >= checkpointX)
            {
                // Đến checkpoint - dừng lại
                isMoving = false;
                arrow.localPosition = new Vector3(checkpointX, 0, 0);

                // Gọi event
                OnReachCheckpoint?.Invoke(currentCheckpoint);

                // Animation checkpoint
                checkpoints[currentCheckpoint].DOPunchScale(Vector3.one * 0.3f, 0.5f);
            }
        }

        // Kiểm tra đã đến cuối progress bar chưa
        if (currentPosition >= progressBarWidth / 2)
        {
            isMoving = false;
            CompleteProgress();
        }
    }

    public void ContinueProgress()
    {
        currentCheckpoint++;
        isMoving = true;
    }

    private void CompleteProgress()
    {
        // Animation ẩn progress bar
        progressBarPanel.transform.DOScale(0f, 0.3f)
            .OnComplete(() => progressBarPanel.SetActive(false));
    }
}
