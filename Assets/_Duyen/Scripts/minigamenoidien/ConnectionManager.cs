using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public bool CheckConnection(Block a, Block b, Direction sideA)
    {
        Direction opposite = sideA switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => Direction.Left
        };

        return a.GetLeg(sideA) == b.GetLeg(opposite);
    }

    public void TryConnect(Block a, Block b, Direction sideA)
    {
        if (CheckConnection(a, b, sideA))
        {
            Debug.Log($"✔ Kết nối thành công giữa {a.name} và {b.name} tại cạnh {sideA}");
            // TODO: hiệu ứng hoặc khóa vị trí
        }
        else
        {
            Debug.Log($"✘ Không thể kết nối {a.name} và {b.name}");
        }
    }
}
