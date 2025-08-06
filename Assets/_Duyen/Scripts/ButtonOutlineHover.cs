using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
public class ButtonOutlineHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Outline outline;
    private Color originalColor;

    private void Start()
    {
        outline = GetComponent<Outline>();

        // Lưu màu gốc để khôi phục alpha
        originalColor = outline.effectColor;

        // Bắt đầu với alpha = 0 (ẩn viền)
        Color invisible = originalColor;
        invisible.a = 0f;
        outline.effectColor = invisible;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.DOFade(1f, 0.2f); // Hiện viền
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.DOFade(0f, 0.2f); // Ẩn viền
    }
}
