using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("UI")]
    public GameObject levelPanel;               // Panel chứa các ô Grid và blocks
    public GameObject canvasUI;
    public Button openMinigameButton;
    public Button closeMinigameButton;
    public Button checkLevelButton;

    [Header("Gameplay")]
    public GridSlot[] gridSlots;                // Các ô cần kiểm tra đúng block

    private bool isCompleted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        levelPanel.SetActive(false);
        canvasUI.SetActive(false);

        openMinigameButton.onClick.AddListener(OpenMinigame);
        closeMinigameButton.onClick.AddListener(CloseMinigame);
        checkLevelButton.onClick.AddListener(CheckLevel);
    }

    private void Update()
    {
        if (!openMinigameButton.interactable &&
            Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
        {
            ResetMinigame();
        }
    }

    public void OpenMinigame()
    {
        if (isCompleted) return;

        canvasUI.SetActive(true);
        levelPanel.SetActive(true);
        AssignExistingBlocks();
    }

    public void CloseMinigame()
    {
        canvasUI.SetActive(false);
        levelPanel.SetActive(false);
    }

    private void AssignExistingBlocks()
    {
        foreach (var slot in gridSlots)
        {
            if (slot.transform.childCount > 0)
            {
                var block = slot.transform.GetChild(0).GetComponent<BlockController>();
                if (block != null)
                    slot.SetBlock(block);
            }
            else
            {
                slot.SetBlock(null);
            }
        }
    }

    public void CheckLevel()
    {
        if (IsLevelCompleted())
        {
            Debug.Log("Đã hoàn thành minigame!");
            isCompleted = true;
            openMinigameButton.interactable = false;
        }
        else
        {
            Debug.Log("Sai rồi, thử lại nhé!");
        }
    }

    private bool IsLevelCompleted()
    {
        foreach (var slot in gridSlots)
        {
            // Bỏ qua slot không yêu cầu block
            if (string.IsNullOrWhiteSpace(slot.requiredBlockName))
                continue;

            if (slot.currentBlock == null)
            {
                Debug.Log("Thiếu block ở slot yêu cầu.");
                return false;
            }

            string currentName = slot.currentBlock.block.blockName.Trim();
            string requiredName = slot.requiredBlockName.Trim();

            if (currentName != requiredName)
            {
                Debug.Log($"Tên sai: {currentName} ≠ {requiredName}");
                return false;
            }

            float currentZ = slot.currentBlock.transform.localEulerAngles.z;
            float requiredZ = slot.requiredEulerAngles.z;
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentZ, requiredZ));

            if (diff > 5f)
            {
                Debug.Log($"Góc sai: {currentZ}° ≠ {requiredZ}° (lệch {diff}°)");
                return false;
            }
        }

        return true;
    }




    private void ResetMinigame()
    {
        isCompleted = false;
        openMinigameButton.interactable = true;
        canvasUI.SetActive(true);
        levelPanel.SetActive(true);

        foreach (var slot in gridSlots)
        {
            slot.SetBlock(null);
        }

        AssignExistingBlocks();
    }

    // Gọi thủ công sau mỗi lần kéo block nếu cần
    public void CheckLevelClear()
    {
        if (isCompleted) return;
        if (IsLevelCompleted())
        {
            Debug.Log("Bạn đã hoàn thành level!");
            isCompleted = true;
            openMinigameButton.interactable = false;
        }
    }
}
