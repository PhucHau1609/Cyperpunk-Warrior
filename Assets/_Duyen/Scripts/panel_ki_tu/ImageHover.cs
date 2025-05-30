using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ImageHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public float hoverScaleAmount = 0.3f;
    [HideInInspector] public float duration = 0.2f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale + Vector3.one * hoverScaleAmount, duration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
    }
}
