using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour, IDropHandler
{
    public string requiredBlockName;
    public Vector3 requiredEulerAngles;
    public BlockController currentBlock;

    public void OnDrop(PointerEventData eventData)
    {
        var dropped = eventData.pointerDrag?.GetComponent<BlockController>();
        if (dropped == null) return;

        if (currentBlock != null)
        {
            dropped.ResetToOriginalPosition();
            return;
        }

        SetBlock(dropped);
    }

    public void SetBlock(BlockController block)
    {
        currentBlock = block;
        if (block == null) return;

        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}