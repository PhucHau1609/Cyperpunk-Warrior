// PurchaseResultPanel.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PurchaseResultPanel : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup rootCanvas;
    public RectTransform content;
    public Text titleText;
    public Text itemNameText;
    public Text amountText;
    public Text statusText;
    public Text extraText;
    public Button closeButton;

    [Header("Colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private Tween currentTween;

    void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    // ⬇⬇⬇ ĐỔI: orderCode từ long -> string và có giá trị mặc định = "" ⬇⬇⬇
    public void Show(string itemName, int amountVnd, bool success, string provider, string orderCode = "")
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
            // orderCode có thể rỗng nếu cổng không trả mã
            extraText.text = string.IsNullOrEmpty(orderCode)
                ? $"Cổng: {provider}"
                : $"Cổng: {provider}\nMã đơn: {orderCode}";
        }

        rootCanvas.alpha = 0;
        content.localScale = Vector3.zero;

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
            .OnComplete(() => gameObject.SetActive(false));
    }
}
