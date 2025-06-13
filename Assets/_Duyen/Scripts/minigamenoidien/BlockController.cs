using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unity.Collections.AllocatorManager;

public class BlockController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum BlockType { Fixed, RotateOnly, MoveOnly, MoveRotate }
    public BlockType blockType;
    public Block block;

    public Transform originalParent { get; private set; }
    public Vector2 originalPosition { get; private set; }

    private Vector3 originalScale;
    private bool isRotating = false;
    private Tween rotateTween; // để lưu tween xoay
    private bool isDragging = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private float pointerDownTime;
    private const float clickThreshold = 0.15f;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (blockType == BlockType.Fixed || blockType == BlockType.RotateOnly)
            return;

        pointerDownTime = Time.time;

        transform.DOScale(originalScale * 0.7f, 0.1f).SetEase(Ease.OutSine);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (blockType == BlockType.Fixed)
            return;

        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);

        // Chỉ RotateOnly và MoveRotate mới xoay được
        if (Time.time - pointerDownTime < clickThreshold &&
            (blockType == BlockType.RotateOnly || blockType == BlockType.MoveRotate))
        {
            Rotate();
            MinigameManager.Instance?.CheckLevel();
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable()) return;

        isDragging = true;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        GridSlot parentSlot = originalParent.GetComponent<GridSlot>();
        if (parentSlot != null) parentSlot.currentBlock = null;

        transform.SetParent(mainCanvas.transform);
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

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
        if (!isDragging) return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;

        if (transform.parent == mainCanvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.DOAnchorPos(originalPosition, 0.25f).SetEase(Ease.OutBack);
        }

        AudioManager.Instance?.PlayBlockInteractSFX();
        MinigameManager.Instance?.CheckLevel();
    }


    private bool IsDraggable()
    {
        return blockType == BlockType.MoveOnly || blockType == BlockType.MoveRotate;
    }


    private void Rotate()
    {
        if (isRotating || rotateTween != null && rotateTween.IsActive() && rotateTween.IsPlaying())
            return;

        isRotating = true;

        float duration = 0.25f;
        float angle = 90f;

        rotateTween = transform
            .DORotate(transform.eulerAngles + new Vector3(0, 0, -angle), duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isRotating = false;
                rotateTween = null;
                AudioManager.Instance?.PlayBlockInteractSFX();

                MinigameManager.Instance?.CheckLevel();
            });
    }
    public void ResetToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.DOAnchorPos(originalPosition, 0.25f).SetEase(Ease.OutBack);
    }
}