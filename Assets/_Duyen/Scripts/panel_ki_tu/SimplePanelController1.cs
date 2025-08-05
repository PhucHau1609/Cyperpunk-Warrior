using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SimplePanelController1 : MonoBehaviour
{
    public RectTransform panel;
    public float tweenDuration = 0.3f;

    private Image[] panelImages;
    public float imageHoverScaleAmount = 0.3f;
    public float imageHoverDuration = 0.2f;

    [Header("Player Control")]
    public PlayerMovement playerMovement; // ⚠️ Kéo từ scene vào Inspector

    void Start()
    {
        panel.localScale = Vector3.zero;
        panel.gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        panel.gameObject.SetActive(true);
        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack);
        // ⚠️ Khóa di chuyển
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }
    }

    public void ClosePanel()
    {
        panel.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            panel.gameObject.SetActive(false);
            // ⚠️ Mở lại di chuyển
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(true);
            }
        });
    }
}
