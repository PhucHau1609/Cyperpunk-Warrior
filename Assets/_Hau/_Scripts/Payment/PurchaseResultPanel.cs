using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PurchaseResultPanel : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup rootCanvas;        // để fade toàn panel
    public RectTransform content;         // panel chính
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI extraText;
    public Button closeButton;

    [Header("Colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private Tween currentTween;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    public void Show(string itemName, int amountVnd, bool success, string provider, long orderCode)
    {
        gameObject.SetActive(true);

        if (titleText) titleText.text = "Kết quả thanh toán";
        if (itemNameText) itemNameText.text = $"Vật phẩm: {itemName}";
        if (amountText) amountText.text = $"Số tiền: {amountVnd:N0} VND";
        if (statusText)
        {
            statusText.text = success ? "Trạng thái: Thành công" : "Trạng thái: Thất bại";
            statusText.color = success ? successColor : failColor;
        }
        if (extraText)
        {
            extraText.text = $"Cổng: {provider}\nMã đơn: {orderCode}";
        }

        // reset trạng thái trước anim
        rootCanvas.alpha = 0;
        content.localScale = Vector3.zero;

        // anim DOTween
        currentTween?.Kill();
        currentTween = DOTween.Sequence()
            .Append(rootCanvas.DOFade(1, 0.3f))
            .Join(content.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
    }

    public void Hide()
    {
        currentTween?.Kill();
        currentTween = DOTween.Sequence()
            .Append(content.DOScale(0f, 0.25f).SetEase(Ease.InBack))
            .Join(rootCanvas.DOFade(0, 0.25f))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
