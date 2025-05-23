using UnityEngine;

public class GridSlot : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Block currentBlock;

    public void SetBlock(Block block)
    {
        currentBlock = block;
        block.transform.position = transform.position;
        block.transform.SetParent(transform);
    }
}
