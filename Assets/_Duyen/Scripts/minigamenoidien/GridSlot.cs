using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour, IDropHandler
{
    [Header("Requirement")]
    public string requiredBlockName;
    public Vector3 requiredEulerAngles;

    public BlockController currentBlock;

    public void OnDrop(PointerEventData eventData)
    {
        BlockController dropped = eventData.pointerDrag?.GetComponent<BlockController>();

        if (dropped == null) return;

        // Gán vào slot
        SetBlock(dropped);
    }

    public void SetBlock(BlockController block)
    {
        if (block == null)
        {
            currentBlock = null;
            return;
        }

        currentBlock = block;
        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
