using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CalibrateDistributorGame : MonoBehaviour
{
    [Header("Node Setup")]
    public DistributorNode[] nodes; // Gán 3 Node
    public float[] nodeSpeeds = { 60f, 80f, 100f };

    [Header("UI References")]
    // public Button actionButton; // << THAY ĐỔI: Bỏ dòng này
    public Button[] actionButtons; // << THAY ĐỔI: Mảng chứa 3 nút bấm
    public Text feedbackText;
    public GameObject minigamePanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip taskCompleteSound;

    private int _currentNodeIndex = 0;
    private int _successfulConnections = 0;

    void Start()
    {
        if (minigamePanel == null) minigamePanel = this.gameObject;

        // << THAY ĐỔI: Kiểm tra mảng actionButtons và gán sự kiện cho từng nút
        if (actionButtons == null || actionButtons.Length != nodes.Length)
        {
            Debug.LogError("Action Buttons array is not assigned correctly or does not match node count!");
            // Có thể return hoặc xử lý lỗi ở đây
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Initialize(this);
                nodes[i].Deactivate();
                if (nodes[i].correspondingBar != null) nodes[i].correspondingBar.fillAmount = 0f;
            }

            // Gán listener cho từng nút, truyền vào index của nút đó
            if (actionButtons != null && i < actionButtons.Length && actionButtons[i] != null)
            {
                int buttonIndex = i; // Cần thiết cho lambda closure
                actionButtons[i].onClick.AddListener(() => HandleNodeStopAttempt(buttonIndex));
                actionButtons[i].interactable = false; // Ban đầu vô hiệu hóa tất cả các nút
            }
        }
        // StartMinigame(); // Gọi khi bắt đầu task
    }

    public void StartMinigame()
    {
        minigamePanel.SetActive(true);
        _currentNodeIndex = 0;
        _successfulConnections = 0;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Deactivate();
                if (nodes[i].correspondingBar != null) nodes[i].correspondingBar.fillAmount = 0.01f;
            }
            if (actionButtons != null && i < actionButtons.Length && actionButtons[i] != null)
            {
                actionButtons[i].interactable = false; // Vô hiệu hóa tất cả nút khi bắt đầu
            }
        }
        if (feedbackText != null) feedbackText.text = "";

        if (nodes.Length > 0 && nodes[0] != null)
        {
            ActivateCurrentNode();
        }
        else
        {
            Debug.LogError("No nodes assigned to CalibrateDistributorGame!");
        }
    }

    void ActivateCurrentNode()
    {
        if (_currentNodeIndex < nodes.Length && nodes[_currentNodeIndex] != null)
        {
            // Vô hiệu hóa tất cả các nút bấm trước
            for (int i = 0; i < actionButtons.Length; i++)
            {
                if (actionButtons[i] != null) actionButtons[i].interactable = false;
            }

            // Kích hoạt node hiện tại và nút bấm tương ứng
            float speed = (_currentNodeIndex < nodeSpeeds.Length) ? nodeSpeeds[_currentNodeIndex] : nodeSpeeds[nodeSpeeds.Length - 1];
            nodes[_currentNodeIndex].Activate(speed);

            if (_currentNodeIndex < actionButtons.Length && actionButtons[_currentNodeIndex] != null)
            {
                actionButtons[_currentNodeIndex].interactable = true; // Kích hoạt nút của node hiện tại
            }
            Debug.Log($"Activating Node {_currentNodeIndex} with speed {speed}. Its button is now active.");
        }
        else
        {
            Debug.Log("Trying to activate a node out of bounds or null.");
        }
    }

    // << THAY ĐỔI: Phương thức này nhận buttonIndex
    void HandleNodeStopAttempt(int buttonIndex)
    {
        // Chỉ xử lý nếu nút được bấm tương ứng với node đang hoạt động
        if (buttonIndex != _currentNodeIndex)
        {
            Debug.LogWarning($"Button {buttonIndex} pressed, but current active node is {_currentNodeIndex}. Ignoring.");
            // Bạn có thể thêm âm thanh báo lỗi nhẹ ở đây nếu muốn
            return;
        }

        if (_currentNodeIndex < nodes.Length && nodes[_currentNodeIndex] != null && nodes[_currentNodeIndex].IsActive())
        {
            bool success = nodes[_currentNodeIndex].TryStopNode();

            if (success)
            {
                PlaySound(successSound);
                _successfulConnections++;
                nodes[_currentNodeIndex].Deactivate(); // Node dừng hoàn toàn
                if (actionButtons[_currentNodeIndex] != null)
                {
                    actionButtons[_currentNodeIndex].interactable = false; // Vô hiệu hóa nút vừa bấm thành công
                }

                _currentNodeIndex++;
                if (_successfulConnections >= nodes.Length)
                {
                    TaskComplete();
                }
                else
                {
                    ActivateCurrentNode(); // Kích hoạt node tiếp theo và nút tương ứng
                }
            }
            else
            {
                PlaySound(failSound);
                // Cho phép thử lại node hiện tại bằng cách kích hoạt lại nó
                nodes[_currentNodeIndex].Activate(nodes[_currentNodeIndex].rotationSpeed);
                // Nút actionButtons[_currentNodeIndex] vẫn interactable để người chơi thử lại
            }
        }
    }

    void TaskComplete()
    {
        if (feedbackText != null) feedbackText.text = "TASK COMPLETE!";
        PlaySound(taskCompleteSound);
        Debug.Log("Task Complete!");

        // Vô hiệu hóa tất cả các nút bấm khi nhiệm vụ hoàn thành
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] != null) actionButtons[i].interactable = false;
        }
        // StartCoroutine(CloseMinigameAfterDelay(2f));
    }

    public void CloseMinigame()
    {
        minigamePanel.SetActive(false);
    }

    IEnumerator CloseMinigameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseMinigame();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}