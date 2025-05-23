using UnityEngine;

public enum Direction { Up, Right, Down, Left }

[System.Serializable]
public class BlockLegData
{
    public int up;
    public int right;
    public int down;
    public int left;

    public int Get(Direction dir)
    {
        return dir switch
        {
            Direction.Up => up,
            Direction.Right => right,
            Direction.Down => down,
            Direction.Left => left,
            _ => 0
        };
    }

    public void RotateClockwise()
    {
        int temp = up;
        up = left;
        left = down;
        down = right;
        right = temp;
    }

    public int TotalLegs()
    {
        return up + right + down + left;
    }
}

public class Block : MonoBehaviour
{
    public BlockLegData legData;

    public void Rotate()
    {
        legData.RotateClockwise();
        transform.Rotate(0, 0, -90); // Xoay vật lý trong Unity
    }

    public int GetLeg(Direction dir)
    {
        return legData.Get(dir);
    }
}
