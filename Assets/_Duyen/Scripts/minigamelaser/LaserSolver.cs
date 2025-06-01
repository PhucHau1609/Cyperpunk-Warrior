using System.Collections.Generic;
using UnityEngine;

public class LaserSolver : MonoBehaviour
{
    public MinigameGridGenerator gridGen;
    public LineRenderer lineRendererPrefab;

    private List<LineRenderer> laserLines = new List<LineRenderer>();

    public void CheckLaserPath()
    {
        ClearLaserLines();

        Vector2Int emitterPos = Vector2Int.zero;
        Vector2Int targetPos = Vector2Int.zero;

        // Tìm emitter & target
        for (int x = 0; x < gridGen.cols; x++)
        {
            for (int y = 0; y < gridGen.rows; y++)
            {
                var obj = gridGen.grid[x, y].currentObject;
                if (obj == null) continue;

                if (obj.GetComponent<LaserEmitter>())
                    emitterPos = new Vector2Int(x, y);
                if (obj.name.Contains("Target"))
                    targetPos = new Vector2Int(x, y);
            }
        }

        // Bắn từ emitter theo 4 hướng
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            if (SimulateLaser(emitterPos, dir, targetPos))
            {
                Debug.Log("✅ Trúng đích! Bạn thắng!");
                return;
            }
        }

        Debug.Log("❌ Chưa trúng đích.");
    }

    bool SimulateLaser(Vector2Int start, Vector2Int dir, Vector2Int target)
    {
        Vector2Int pos = start;
        Vector3 worldStart = gridGen.grid[pos.x, pos.y].transform.position;
        Vector3 worldEnd;

        for (int i = 0; i < 100; i++)
        {
            pos += dir;

            if (!gridGen.IsValid(pos)) return false;

            var cell = gridGen.grid[pos.x, pos.y];
            var obj = cell.currentObject;

            worldEnd = cell.transform.position;
            DrawLaserLine(worldStart, worldEnd);
            worldStart = worldEnd;

            if (obj == null) continue;

            if (obj.name.Contains("Wall")) return false;

            if (obj.name.Contains("Target")) return true;

            MirrorType mirror = obj.GetComponent<MirrorType>();
            if (mirror != null)
            {
                dir = ReflectDirection(dir, mirror.kind);
            }
        }

        return false;
    }

    Vector2Int ReflectDirection(Vector2Int dir, MirrorKind kind)
    {
        // Slash (/) mirror
        if (kind == MirrorKind.Slash)
        {
            if (dir == Vector2Int.up) return Vector2Int.left;
            if (dir == Vector2Int.down) return Vector2Int.right;
            if (dir == Vector2Int.left) return Vector2Int.up;
            if (dir == Vector2Int.right) return Vector2Int.down;
        }
        // Backslash (\) mirror
        else
        {
            if (dir == Vector2Int.up) return Vector2Int.right;
            if (dir == Vector2Int.down) return Vector2Int.left;
            if (dir == Vector2Int.left) return Vector2Int.down;
            if (dir == Vector2Int.right) return Vector2Int.up;
        }

        return dir;
    }

    void DrawLaserLine(Vector3 start, Vector3 end)
    {
        LineRenderer line = Instantiate(lineRendererPrefab, transform);
        line.positionCount = 2;
        line.SetPosition(0, start + Vector3.back * 0.1f); // tránh z-fighting
        line.SetPosition(1, end + Vector3.back * 0.1f);
        laserLines.Add(line);
    }

    void ClearLaserLines()
    {
        foreach (var line in laserLines)
        {
            Destroy(line.gameObject);
        }
        laserLines.Clear();
    }
}
