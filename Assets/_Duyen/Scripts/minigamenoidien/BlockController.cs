using UnityEngine;
using UnityEngine.EventSystems;

public class BlockController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public enum BlockType { Fixed, RotateOnly, MoveOnly, MoveRotate }

    public BlockType blockType;

    private Vector3 dragOffset;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 pointerDownPos;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void Rotate90()
    {
        GetComponent<Block>()?.Rotate();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPos = eventData.position;
        isDragging = false;

        if (blockType == BlockType.Fixed) return;

        if (blockType == BlockType.RotateOnly || blockType == BlockType.MoveRotate)
            Rotate90();

        if (blockType == BlockType.MoveOnly || blockType == BlockType.MoveRotate)
        {
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(eventData.position);
            dragOffset = transform.position - new Vector3(worldPoint.x, worldPoint.y, transform.position.z);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging && Vector2.Distance(eventData.position, pointerDownPos) > 5f)
            isDragging = true;

        if (isDragging && (blockType == BlockType.MoveOnly || blockType == BlockType.MoveRotate))
        {
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(eventData.position);
            transform.position = new Vector3(worldPoint.x, worldPoint.y, transform.position.z) + dragOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging && (blockType == BlockType.RotateOnly || blockType == BlockType.MoveRotate))
            Rotate90();
    }
}
