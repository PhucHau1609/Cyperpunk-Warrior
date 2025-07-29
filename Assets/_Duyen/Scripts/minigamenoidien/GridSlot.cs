using UnityEngine;
using UnityEngine.EventSystems;

public class GridSlot : MonoBehaviour, IDropHandler
{
    public string requiredBlockName;
    public Vector3 requiredEulerAngles;
    public BaseBlockController currentBlock;

    private void Start()
    {
        // Gán block ban đầu nếu có sẵn (FixedBlock đặt trước trong hierarchy)
        var existingBlock = GetComponentInChildren<BaseBlockController>();
        if (existingBlock != null)
        {
            currentBlock = existingBlock;
            //Debug.Log($"[GridSlot] {name} gán lại currentBlock: {existingBlock.name}");
        }
        else
        {
            //Debug.LogWarning($"[GridSlot] {name} KHÔNG tìm thấy block con");
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        var dropped = eventData.pointerDrag?.GetComponent<BlockController>();
        if (dropped == null) return;

        // Nếu ô đã có block (cả Fixed hoặc đang chứa Move block), không cho thả
        if (HasBlock())
        {
            dropped.ResetToOriginalPosition();
            return;
        }

        SetBlock(dropped);
    }

    public void SetBlock(BaseBlockController block)
    {
        currentBlock = block;
        if (block == null) return;

        block.transform.SetParent(transform);
        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public bool HasBlock()
    {
        return GetComponentInChildren<BaseBlockController>() != null;
    }


}


//using UnityEngine;
//using UnityEngine.EventSystems;

//public class GridSlot : MonoBehaviour, IDropHandler
//{
//    public string requiredBlockName;
//    public Vector3 requiredEulerAngles;
//    public BlockController currentBlock;

//    public void OnDrop(PointerEventData eventData)
//    {
//        var dropped = eventData.pointerDrag?.GetComponent<BlockController>();
//        if (dropped == null) return;

//        if (currentBlock != null)
//        {
//            dropped.ResetToOriginalPosition();
//            return;
//        }

//        SetBlock(dropped);
//    }

//    public void SetBlock(BlockController block)
//    {
//        currentBlock = block;
//        if (block == null) return;

//        block.transform.SetParent(transform);
//        block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//    }
//}