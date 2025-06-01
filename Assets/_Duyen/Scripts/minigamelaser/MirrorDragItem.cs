using UnityEngine;
using UnityEngine.EventSystems;

public class MirrorDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject mirrorPrefab;
    private GameObject draggingObject;

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingObject = Instantiate(mirrorPrefab, transform.root); // t?o mirror dý?i Canvas
        draggingObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggingObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(draggingObject); // n?u không ðý?c nh?n th? xoá

        // Vi?c ð?t vào Cell s? do Cell x? l?
    }
}
