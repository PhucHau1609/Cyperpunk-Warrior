using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Panel trượt từ dưới lên giữa màn hình khi Show()
// Yêu cầu DOTween (DOAnchorPos)
public class PaymentMethodPanel : MonoBehaviour
{
    [Header("Panel Root")]
    public RectTransform panel;          // chính RectTransform của panel
    public CanvasGroup canvasGroup;      // để fade + block raycast

    [Header("Buttons")]
    public Button btnPayOS;
    public Button btnZaloPay;
    public Button btnClose;

    [Header("Anim")]
    public float showDuration = 0.35f;
    public float hideDuration = 0.25f;
    public float offscreenOffsetY = -600f; // vị trí bắt đầu (dưới màn)
    public float targetY = 0f;             // vị trí đích (giữa)

    private System.Action onPayOSSelected;
    private System.Action onZaloSelected;
    private Tween currentTween;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);

        btnPayOS.onClick.AddListener(() => { onPayOSSelected?.Invoke(); Hide(); });
        btnZaloPay.onClick.AddListener(() => { onZaloSelected?.Invoke(); Hide(); });
        btnClose.onClick.AddListener(HideImmediately);
    }

    public void Show(System.Action onPayOS, System.Action onZalo)
    {
        onPayOSSelected = onPayOS;
        onZaloSelected = onZalo;

        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = true;
        var anchored = panel.anchoredPosition;
        anchored.y = offscreenOffsetY;
        panel.anchoredPosition = anchored;

        currentTween?.Kill();
        var seq = DOTween.Sequence();
        seq.Join(panel.DOAnchorPosY(targetY, showDuration).SetEase(Ease.OutCubic));
        seq.Join(canvasGroup.DOFade(1f, showDuration * 0.8f));
        currentTween = seq;
    }

    public void Hide()
    {
        canvasGroup.blocksRaycasts = false;
        currentTween?.Kill();
        var seq = DOTween.Sequence();
        seq.Join(panel.DOAnchorPosY(offscreenOffsetY, hideDuration).SetEase(Ease.InCubic));
        seq.Join(canvasGroup.DOFade(0f, hideDuration * 0.8f));
        seq.OnComplete(() => gameObject.SetActive(false));
        currentTween = seq;
    }

    // Dùng cho nút X (đóng nhanh)
    public void HideImmediately()
    {
        currentTween?.Kill();
        gameObject.SetActive(false);
    }
}
