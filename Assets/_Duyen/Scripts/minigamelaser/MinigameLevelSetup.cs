using UnityEngine;

public class MinigameLevelSetup : MonoBehaviour
{
    public MinigameGridGenerator gridGen;

    public GameObject emitterPrefab;
    public GameObject targetPrefab;
    public GameObject wallPrefab;

    void Start()
    {
        // Đặt emitter tại (0, 2)
        PlaceObject(emitterPrefab, new Vector2Int(0, 2));

        // Đặt target tại (5, 2)
        PlaceObject(targetPrefab, new Vector2Int(5, 2));

        // Đặt một vài bức tường
        PlaceObject(wallPrefab, new Vector2Int(2, 2));
        PlaceObject(wallPrefab, new Vector2Int(3, 2));
    }

    void PlaceObject(GameObject prefab, Vector2Int pos)
    {
        CellData cell = gridGen.grid[pos.x, pos.y];
        cell.SetObject(prefab);
    }
}
