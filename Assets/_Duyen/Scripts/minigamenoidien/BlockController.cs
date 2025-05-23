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

    private GridSlot originalSlot; // lưu slot gốc khi bắt đầu kéo



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
            // Lưu slot gốc nếu có
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);
            if (hit.collider != null)
            {
                var slot = hit.collider.GetComponent<GridSlot>();
                if (slot != null)
                {
                    originalSlot = slot;
                }
            }

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            dragOffset = transform.position - new Vector3(worldPoint.x, worldPoint.y, transform.position.z);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging && Vector2.Distance(eventData.position, pointerDownPos) > 5f)
            isDragging = true;

        if (isDragging && (blockType == BlockType.MoveOnly || blockType == BlockType.MoveRotate))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            transform.position = new Vector3(worldPoint.x, worldPoint.y, transform.position.z) + dragOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            if (blockType == BlockType.RotateOnly || blockType == BlockType.MoveRotate)
                Rotate90();
            return;
        }

        // Raycast tìm GridSlot ở vị trí thả chuột
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);
        if (hit.collider != null)
        {
            GridSlot slot = hit.collider.GetComponent<GridSlot>();
            if (slot != null)
            {
                // Gán block vào slot mới
                slot.SetBlock(GetComponent<Block>());

                // Đặt vị trí block tại (0,0,0) localPosition của slot
                transform.localPosition = Vector3.zero;

                // Nếu cần, update slot gốc để xóa tham chiếu
                if (originalSlot != null && originalSlot != slot)
                {
                    originalSlot.currentBlock = null;
                }
            }
            else
            {
                // Không thả đúng slot -> trả block về vị trí cũ
                ReturnToOriginalSlot();
            }
        }
        else
        {
            // Không thả đúng slot -> trả block về vị trí cũ
            ReturnToOriginalSlot();
        }

        isDragging = false;
    }

    private void ReturnToOriginalSlot()
    {
        if (originalSlot != null)
        {
            originalSlot.SetBlock(GetComponent<Block>());
            transform.localPosition = Vector3.zero;
        }
    }
}
