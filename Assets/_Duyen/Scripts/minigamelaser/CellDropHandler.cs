using UnityEngine;
using UnityEngine.EventSystems;

public class CellDropHandler : MonoBehaviour, IDropHandler
{
    public CellData cellData;

    public void OnDrop(PointerEventData eventData)
    {
        if (cellData.currentObject != null) return;

        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        MirrorDragItem mirrorItem = dragged.GetComponent<MirrorDragItem>();
        if (mirrorItem == null) return;

        cellData.SetObject(mirrorItem.mirrorPrefab);
    }
}
