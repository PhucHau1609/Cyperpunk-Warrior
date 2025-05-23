using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject[] levelPanels;
    public Button openMinigameButton;
    public Button closeMinigameButton;

    [Header("Prefabs")]
    public GameObject blockFixedPrefab;
    public GameObject blockMoveOnlyPrefab;
    public GameObject blockRotateOnlyPrefab;
    public GameObject blockMoveRotatePrefab;

    private int currentLevel = 0;
    private bool isCompleted = false;

    // Danh sách block cho mỗi level
    private Dictionary<int, List<BlockSpawnData>> levelBlockMap;

    void Start()
    {
        CloseAll();
        openMinigameButton.onClick.AddListener(OpenMinigame);
        closeMinigameButton.onClick.AddListener(CloseMinigame);

        SetupLevelData();
    }

    void Update()
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

        CloseAll();
        levelPanels[currentLevel].SetActive(true);
        SetupBlocksForCurrentLevel();
    }

    public void CloseMinigame() => CloseAll();

    public void CompleteCurrentLevel()
    {
        levelPanels[currentLevel].SetActive(false);
        currentLevel++;

        if (currentLevel >= levelPanels.Length)
        {
            isCompleted = true;
            openMinigameButton.interactable = false;
            Debug.Log("🎉 Đã hoàn thành toàn bộ minigame!");
        }
        else
        {
            levelPanels[currentLevel].SetActive(true);
            SetupBlocksForCurrentLevel();
        }
    }

    void ResetMinigame()
    {
        currentLevel = 0;
        isCompleted = false;
        openMinigameButton.interactable = true;
        CloseAll();
    }

    void CloseAll()
    {
        foreach (var panel in levelPanels)
        {
            panel.SetActive(false);
            foreach (Transform child in panel.transform)
            {
                GridSlot slot = child.GetComponent<GridSlot>();
                if (slot != null && slot.currentBlock != null)
                {
                    Destroy(slot.currentBlock.gameObject);
                    slot.currentBlock = null;
                }
            }
        }
    }

    public int GetCurrentLevel() => currentLevel;

    // Tạo dữ liệu cho từng level
    void SetupLevelData()
    {
        levelBlockMap = new Dictionary<int, List<BlockSpawnData>>();

        levelBlockMap[0] = new List<BlockSpawnData> {
            new BlockSpawnData { prefab = blockFixedPrefab, gridPos = new Vector2Int(0, 0) },
            new BlockSpawnData { prefab = blockMoveOnlyPrefab, gridPos = new Vector2Int(1, 0) },
        };

        levelBlockMap[1] = new List<BlockSpawnData> {
            new BlockSpawnData { prefab = blockFixedPrefab, gridPos = new Vector2Int(0, 0) },
            new BlockSpawnData { prefab = blockRotateOnlyPrefab, gridPos = new Vector2Int(1, 0) },
        };

        levelBlockMap[2] = new List<BlockSpawnData> {
            new BlockSpawnData { prefab = blockFixedPrefab, gridPos = new Vector2Int(0, 0) },
            new BlockSpawnData { prefab = blockMoveRotatePrefab, gridPos = new Vector2Int(1, 0) },
        };

        levelBlockMap[3] = new List<BlockSpawnData> {
            new BlockSpawnData { prefab = blockMoveRotatePrefab, gridPos = new Vector2Int(0, 0) },
            new BlockSpawnData { prefab = blockMoveRotatePrefab, gridPos = new Vector2Int(1, 0) },
            new BlockSpawnData { prefab = blockMoveRotatePrefab, gridPos = new Vector2Int(2, 0) },

            new BlockSpawnData { prefab = blockMoveOnlyPrefab, gridPos = new Vector2Int(0, 1) },
            new BlockSpawnData { prefab = blockMoveOnlyPrefab, gridPos = new Vector2Int(1, 1) },
            new BlockSpawnData { prefab = blockMoveOnlyPrefab, gridPos = new Vector2Int(2, 1) },
            new BlockSpawnData { prefab = blockMoveOnlyPrefab, gridPos = new Vector2Int(3, 1) },

            new BlockSpawnData { prefab = blockRotateOnlyPrefab, gridPos = new Vector2Int(1, 2) },
        };
    }

    void SetupBlocksForCurrentLevel()
    {
        if (!levelBlockMap.ContainsKey(currentLevel)) return;

        GridSlot[] slots = levelPanels[currentLevel].GetComponentsInChildren<GridSlot>();

        foreach (BlockSpawnData data in levelBlockMap[currentLevel])
        {
            GridSlot targetSlot = System.Array.Find(slots, s => s.gridPosition == data.gridPos);
            if (targetSlot == null)
            {
                Debug.LogWarning($"⚠ Không tìm thấy GridSlot tại {data.gridPos} trong level {currentLevel}");
                continue;
            }

            GameObject obj = Instantiate(data.prefab);
            Block blockComp = obj.GetComponent<Block>();
            if (blockComp == null)
            {
                Debug.LogWarning("⚠ Prefab không có component Block!");
                continue;
            }

            targetSlot.SetBlock(blockComp); // Gắn vào GridSlot (set parent + position)

            // Cài đặt loại block nếu cần (có thể mở rộng)
            BlockController ctrl = obj.GetComponent<BlockController>();
            if (ctrl != null)
            {
                if (data.prefab == blockFixedPrefab) ctrl.blockType = BlockController.BlockType.Fixed;
                else if (data.prefab == blockMoveOnlyPrefab) ctrl.blockType = BlockController.BlockType.MoveOnly;
                else if (data.prefab == blockRotateOnlyPrefab) ctrl.blockType = BlockController.BlockType.RotateOnly;
                else if (data.prefab == blockMoveRotatePrefab) ctrl.blockType = BlockController.BlockType.MoveRotate;
            }
        }

    }
}
