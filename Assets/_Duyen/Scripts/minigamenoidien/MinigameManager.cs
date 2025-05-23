using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject[] levelPanels;
    public Button openMinigameButton;
    public Button closeMinigameButton;

    [Header("Grid Setup")]
    public GridSlot[] allSlots; // Kéo toàn bộ slot của màn chơi hiện tại vào đây trong Unity
    public int gridWidth = 5;
    public int gridHeight = 5;

    private GridSlot[,] grid;
    private int currentLevel = 0;
    private bool isCompleted = false;

    void Start()
    {
        SetupGrid();
        CloseAll();
        openMinigameButton.onClick.AddListener(OpenMinigame);
        closeMinigameButton.onClick.AddListener(CloseMinigame);
    }

    void Update()
    {
        if (!openMinigameButton.interactable &&
            Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
        {
            ResetMinigame();
        }
    }

    void SetupGrid()
    {
        grid = new GridSlot[gridHeight, gridWidth];

        foreach (GridSlot slot in allSlots)
        {
            Vector2Int pos = slot.gridPosition;
            if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
            {
                grid[pos.y, pos.x] = slot;
            }
            else
            {
                Debug.LogWarning($"⚠️ GridSlot vị trí {pos} vượt ngoài kích thước lưới {gridWidth}x{gridHeight}");
            }
        }
    }

    public void OpenMinigame()
    {
        if (isCompleted) return;

        CloseAll();
        levelPanels[currentLevel].SetActive(true);
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
        }
    }

    public int GetCurrentLevel() => currentLevel;

    private bool CheckVictory()
    {
        int usedLegs = 0;
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var slot = grid[y, x];
                if (slot == null || slot.currentBlock == null) continue;

                Block block = slot.currentBlock;

                foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                {
                    int leg = block.GetLeg(dir);
                    if (leg == 0) continue;

                    Vector2Int offset = dir switch
                    {
                        Direction.Up => new Vector2Int(0, 1),
                        Direction.Down => new Vector2Int(0, -1),
                        Direction.Left => new Vector2Int(-1, 0),
                        Direction.Right => new Vector2Int(1, 0),
                        _ => Vector2Int.zero
                    };

                    Vector2Int targetPos = slot.gridPosition + offset;
                    if (targetPos.x < 0 || targetPos.x >= cols || targetPos.y < 0 || targetPos.y >= rows)
                        return false;

                    var neighborSlot = grid[targetPos.y, targetPos.x];
                    if (neighborSlot == null || neighborSlot.currentBlock == null)
                        return false;

                    int neighborLeg = neighborSlot.currentBlock.GetLeg(Opposite(dir));
                    if (neighborLeg != leg)
                        return false;

                    usedLegs++;
                }
            }
        }

        return usedLegs % 2 == 0;
    }

    private Direction Opposite(Direction dir) => dir switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => dir
    };
}
