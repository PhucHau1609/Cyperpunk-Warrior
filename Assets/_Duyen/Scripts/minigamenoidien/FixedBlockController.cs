using UnityEngine;
using UnityEngine.EventSystems;

public class FixedBlockController : RotatableBlockBase, IPointerUpHandler
{
    public enum BlockType { Fixed, RotateOnly }
    public BlockType blockType;
    public Block block;

    public void OnPointerUp(PointerEventData eventData)
    {
        if (blockType == BlockType.RotateOnly)
        {
            Rotate();
        }
    }
}