using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block_guongController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum BlockType { Obstacle, MirrorSlash, MirrorBackslash }
    public BlockType blockType;
    public Block_guong block;

    private Vector3 originalScale;

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
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;

        // Tween scale riêng, không DOKill toàn bộ transform nữa
        transform.DOScale(originalScale * 0.7f, 0.1f).SetEase(Ease.OutSine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (blockType == BlockType.Obstacle) return;

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
            rectTransform.DOAnchorPos(originalPosition, 0.25f).SetEase(Ease.OutBack);
        }
        AudioManager.Instance?.PlayBlockInteractSFX();
        LaserManager laserManager = Object.FindFirstObjectByType<LaserManager>();
        laserManager?.FireLaser();
    }
}
