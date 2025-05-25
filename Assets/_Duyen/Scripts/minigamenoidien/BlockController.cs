using UnityEngine;
using UnityEngine.EventSystems;

public class BlockController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum BlockType { Fixed, RotateOnly, MoveOnly, MoveRotate }
    public BlockType blockType;
    public Block block;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private Vector3 originalPosition;
    private Transform originalParent;
    private float pointerDownTime;
    private const float clickThreshold = 0.15f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.time - pointerDownTime < clickThreshold &&
            (blockType == BlockType.RotateOnly || blockType == BlockType.MoveRotate))
        {
            Rotate();

            // Gọi check level sau khi xoay
            MinigameManager.Instance?.CheckLevel();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (blockType == BlockType.Fixed || blockType == BlockType.RotateOnly) return;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        GridSlot parentSlot = originalParent.GetComponent<GridSlot>();
        if (parentSlot != null) parentSlot.currentBlock = null;

        transform.SetParent(mainCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == mainCanvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
        AudioManager.Instance?.PlayBlockInteractSFX();
        MinigameManager.Instance?.CheckLevel();
    }

    private void Rotate()
    {
        block?.Rotate();
        AudioManager.Instance?.PlayBlockInteractSFX();
    }
}
