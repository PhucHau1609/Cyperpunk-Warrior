using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlotLaser : MonoBehaviour, IDropHandler
{
    [Header("Requirement")]
    public string requiredBlockName;

    public Block_guongController currentBlock;

    public void OnDrop(PointerEventData eventData)
    {
        Block_guongController dropped = eventData.pointerDrag?.GetComponent<Block_guongController>();

        if (dropped == null) return;

        // Gán vào slot
        SetBlock(dropped);
    }

    public void SetBlock(Block_guongController block)
    {
        if (block == null)
        {
            currentBlock = null;
            return;
        }

        currentBlock = block;
        block.transform.SetParent(transform);    }
}
