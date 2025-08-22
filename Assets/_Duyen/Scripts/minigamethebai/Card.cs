using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] public Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;
    public bool isMatched;
    public bool IsFlipped => iconImage != null && iconImage.sprite == iconSprite;

    public Sprite IconSprite => iconSprite; // thêm property an toàn

    public CardsController controller;

    public void OnCardClick()
    {
        controller.OnCardClicked(this);
    }

    public void SetIconSprite(Sprite sp, bool faceUp = false)
    {
        iconSprite = sp;
        isSelected = false;
        isMatched = false;

        if (faceUp)
            iconImage.sprite = iconSprite;
        else
            iconImage.sprite = hiddenIconSprite;
    }

    public void Show()
    {
        transform.DORotate(new Vector3(0f, 180f, 0f), .2f);
        DOVirtual.DelayedCall(0.1f, () => iconImage.sprite = iconSprite);
        isSelected = true;
    }

    public void Hide()
    {
        transform.DORotate(new Vector3(0f, 0f, 0f), .2f);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            isSelected = false;
        });
    }

    public void Match()
    {
        isMatched = true;
    }

    public void FadeOut()
    {
        isMatched = true;
        isSelected = true;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();

        cg.blocksRaycasts = false;
        cg.DOFade(0f, 0.3f)
        .SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            if (this != null && gameObject != null) // kiểm tra an toàn
                Destroy(gameObject);
        });
    }

    public void FlipUpWithAnim()
    {
        transform.DORotate(new Vector3(0f, 180f, 0f), .3f);
        DOVirtual.DelayedCall(0.15f, () => iconImage.sprite = iconSprite);
        isSelected = false;
    }
    void OnDestroy()
    {
        DOTween.Kill(this); // hoặc DOTween.Kill(gameObject);
    }
    public void Initialize(Sprite sp, Sprite hidden = null)
    {
        iconSprite = sp;
        isSelected = false;
        isMatched = false;

        if (hidden != null)
            hiddenIconSprite = hidden;

        // Nếu prefab chưa có Image thì thêm vào
        if (iconImage == null)
            iconImage = gameObject.AddComponent<Image>();

        // mặc định úp xuống
        iconImage.sprite = hiddenIconSprite != null ? hiddenIconSprite : sp;
    }

}
