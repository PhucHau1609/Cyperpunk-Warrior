using UnityEngine;
using UnityEngine.UI;

public class MinigameGridGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridParent;
    public int rows = 6;
    public int cols = 6;

    public CellData[,] grid;

    void Start()
    {
        grid = new CellData[cols, rows];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject newCell = Instantiate(cellPrefab, gridParent);
                CellData cellData = newCell.GetComponent<CellData>();
                cellData.gridPosition = new Vector2Int(x, y);
                grid[x, y] = cellData;
            }
        }
    }
    public bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < cols && pos.y >= 0 && pos.y < rows;
    }

}
