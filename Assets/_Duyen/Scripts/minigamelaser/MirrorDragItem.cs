using UnityEngine;
using UnityEngine.EventSystems;

public class MirrorDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject mirrorPrefab;
    private GameObject draggingObject;

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingObject = Instantiate(mirrorPrefab, transform.root); // t?o mirror d�?i Canvas
        draggingObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggingObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(draggingObject); // n?u kh�ng ��?c nh?n th? xo�

        // Vi?c �?t v�o Cell s? do Cell x? l?
    }
}
