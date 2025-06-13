using DG.Tweening;
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

        // Nếu slot này đã có block, từ chối nhận block mới
        if (currentBlock != null)
        {
            dropped.ResetToOriginalPosition();
            return;
        }
        //if (currentBlock != null)
        //{
        //    // Đưa block về lại chỗ cũ
        //    dropped.transform.SetParent(dropped.originalParent);
        //    dropped.GetComponent<RectTransform>()
        //           .DOAnchorPos(dropped.originalPosition, 0.25f)
        //           .SetEase(Ease.OutBack);
        //    return;
        //}

        // Nếu slot trống -> gán block
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

        MinigameManager.Instance?.CheckLevel();

    }
}