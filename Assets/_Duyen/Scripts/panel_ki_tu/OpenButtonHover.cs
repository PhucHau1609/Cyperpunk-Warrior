using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class OpenButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float hoverScale = 1.1f;
    public float duration = 0.2f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(hoverScale, duration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
    }
}
