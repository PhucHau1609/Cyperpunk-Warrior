using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockController : BaseBlockController,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public enum BlockType { MoveOnly, MoveRotate }
    public BlockType blockType;

    public Transform originalParent { get; private set; }
    public Vector2 originalPosition { get; private set; }

    private Vector3 originalScale;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private bool isDragging = false;
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
        pointerDownTime = Time.time;
        transform.DOScale(originalScale * 0.7f, 0.1f).SetEase(Ease.OutSine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);

        if (Time.time - pointerDownTime < clickThreshold && blockType == BlockType.MoveRotate)
        {
            Rotate();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        if (originalParent.TryGetComponent(out GridSlot parentSlot))
            parentSlot.currentBlock = null;

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
            ResetToOriginalPosition();
        }

        AudioManager.Instance?.PlayBlockInteractSFX();
        MinigameManager.Instance?.CheckLevel();
    }

    public void ResetToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.DOAnchorPos(originalPosition, 0.25f).SetEase(Ease.OutBack);
        if (originalParent.TryGetComponent(out GridSlot originSlot))
        {
            originSlot.SetBlock(this);
        }
    }
}
